using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Eon.Collections;
using Eon.ComponentModel;
using Eon.Context;
using Eon.Diagnostics;
using Eon.Linq;
using Eon.Runtime.Options;
using Eon.Threading.Tasks;

using itrlck = Eon.Threading.InterlockedUtilities;
using vlt = Eon.Threading.VolatileUtilities;

namespace Eon.Threading {

	[DebuggerDisplay("{ToString(),nq}")]
	public sealed class StatefulSynchronizationContext<TCtxState>
		:Disposable, IStatefulSynchronizationContext<TCtxState> {

		#region Nested types

		abstract class P_InvokeItemBase {

			readonly StatefulSynchronizationContext<TCtxState> _ctx;

			protected P_InvokeItemBase(StatefulSynchronizationContext<TCtxState> ctx) {
				ctx.EnsureNotNull(nameof(ctx));
				//
				_ctx = ctx;
			}

			public abstract void Invoke(StatefulSynchronizationContext<TCtxState> ctx, IVh<TCtxState> ctxState);

			public abstract void TrySetInvokeResultAsCanceled();

			public abstract void TryCancel();

			public abstract bool IsInvokeCompleted { get; }

			public sealed override string ToString()
				=> $"Invoke:{GetDisplayInfoText().FmtStr().GNLI()}{Environment.NewLine}\tContext:{_ctx.FmtStr().GNLI2()}";

			protected abstract string GetDisplayInfoText();

		}

		abstract class P_InvokeItemBase<TResult>
			:P_InvokeItemBase {

			readonly CancellationToken _ct;

			readonly CancellationTokenSource _cts;

			readonly CancellationTokenRegistration _ctRegistration;

			readonly TaskCompletionSource<TResult> _invokeResultAwaitable;

			readonly TaskCompletionSource<TResult> _userInvokeAwaitable;

			int _isInvokeCompleted;

			protected P_InvokeItemBase(StatefulSynchronizationContext<TCtxState> ctx, CancellationToken ctxCt, CancellationToken userCt)
				: base(ctx: ctx) {
				_ct = ctxCt.SingleOrLinked(ct2: userCt, linkedCts: out _cts);
				if (_cts is null) {
					_cts = new CancellationTokenSource();
					_ct = _cts.Token;
				}
				//
				_invokeResultAwaitable = new TaskCompletionSource<TResult>(creationOptions: TaskCreationOptions.None);
				_userInvokeAwaitable = new TaskCompletionSource<TResult>(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);
				//
				if (_ct.CanBeCanceled)
					_ctRegistration = _ct.Register(callback: P_OnCtCallback);
				_invokeResultAwaitable
					.Task
					.ContinueWith(
						continuationAction: P_InvokeResultContinuation,
						cancellationToken: CancellationToken.None,
						continuationOptions: TaskContinuationOptions.PreferFairness,
						scheduler: TaskScheduler.Default);
			}

			protected CancellationToken Ct
				=> _ct;

			public sealed override bool IsInvokeCompleted
				=> Interlocked.CompareExchange(ref _isInvokeCompleted, 0, 0) == 1;

			public Task<TResult> UserInvokeAwaitable
				=> _userInvokeAwaitable.Task;

			void P_OnCtCallback()
				=> _invokeResultAwaitable.TrySetCanceled(cancellationToken: _ct);

			void P_InvokeResultContinuation(Task<TResult> invokeResultAwaitable) {
				Interlocked.Exchange(ref _isInvokeCompleted, 1);
				_ctRegistration.Dispose();
				_cts?.Dispose();
				//
				if (!TaskUtilities.TryApplyResultTo(fromTask: invokeResultAwaitable, toTaskProxy: _userInvokeAwaitable) && invokeResultAwaitable.IsFaulted)
					throw invokeResultAwaitable.Exception.Flatten();
			}

			public sealed override void TrySetInvokeResultAsCanceled() {
				if (!IsInvokeCompleted)
					_invokeResultAwaitable.TrySetCanceled();
			}

			public sealed override void TryCancel() {
				if (!IsInvokeCompleted && !_cts.IsCancellationRequested) {
					try {
						_cts.Cancel(throwOnFirstException: false);
					}
					catch (ObjectDisposedException) { }
				}
			}

			public sealed override void Invoke(StatefulSynchronizationContext<TCtxState> ctx, IVh<TCtxState> ctxState) {
				try {
					if (Ct.IsCancellationRequested)
						_invokeResultAwaitable.TrySetCanceled(cancellationToken: Ct);
					else {
						DoInvoke(ctx: ctx, ctxState: ctxState, result: out var result);
						_invokeResultAwaitable.TrySetResult(result: result);
					}
				}
				catch (Exception exception) {
					if (exception.IsOperationCancellationOf(ct: Ct))
						_invokeResultAwaitable.TrySetCanceled(cancellationToken: Ct);
					else if (exception.IsOperationCancellation())
						_invokeResultAwaitable.TrySetCanceled();
					else if (!_invokeResultAwaitable.TrySetException(exception: exception))
						throw;
				}
			}

			protected abstract void DoInvoke(StatefulSynchronizationContext<TCtxState> ctx, IVh<TCtxState> ctxState, out TResult result);

		}

		sealed class P_InvokeItem<TUserState, TResult>
			:P_InvokeItemBase<TResult> {

			public readonly TUserState UserState;

			public readonly StatefulSynchronizationContextExecute<TCtxState, TUserState, TResult> Delegate;

			internal P_InvokeItem(
				StatefulSynchronizationContext<TCtxState> ctx,
				StatefulSynchronizationContextExecute<TCtxState, TUserState, TResult> @delegate,
				TUserState userState,
				CancellationToken contextCt,
				CancellationToken userCt)
				: base(ctx: ctx, ctxCt: contextCt, userCt: userCt) {
				//
				@delegate.EnsureNotNull(nameof(@delegate));
				//
				Delegate = @delegate;
				UserState = userState;
			}

			protected override void DoInvoke(StatefulSynchronizationContext<TCtxState> ctx, IVh<TCtxState> ctxState, out TResult result)
				=> result = Delegate(ctx: ctx, ctxState: ctxState, state: UserState, ct: Ct);

			protected override string GetDisplayInfoText()
				=> $"{nameof(TUserState)}:{typeof(TUserState).Name.FmtStr().GNLI()}{Environment.NewLine}{nameof(TResult)}:{typeof(TResult).Name.FmtStr().GNLI()}";

		}

		sealed class P_LoopStartState {

			public readonly StatefulSynchronizationContext<TCtxState> Context;

			public readonly ManualResetEventSlim LoopStartedEvent;

			internal P_LoopStartState(StatefulSynchronizationContext<TCtxState> context, ManualResetEventSlim loopStartedEvent) {
				context.EnsureNotNull(nameof(context));
				loopStartedEvent.EnsureNotNull(nameof(loopStartedEvent));
				//
				Context = context;
				LoopStartedEvent = loopStartedEvent;
			}

		}

		#endregion

		#region Static members

		static void P_Loop(object state) {
			var startState = state.EnsureNotNull(nameof(state)).EnsureOfType<P_LoopStartState>().Value;
			//
			var ctx = startState.Context;
			IUnhandledExceptionObserver unhandledExceptionObserver;
			IRunControl runControl;
			CancellationToken ct;
			Func<CancellationToken, IVh<TCtxState>> stateFactory;
			ManualResetEventSlim awakeLoopEvent;
			Queue<P_InvokeItemBase> queue;
			PrimitiveSpinLock queueSpinLock;
			try {
				unhandledExceptionObserver = ctx.P_UnhandledExceptionObserver;
				runControl = ctx.RunControl;
				ct = ctx.P_Ct;
				stateFactory = ctx.P_StateFactory;
				awakeLoopEvent = ctx.P_AwakeLoopEvent;
				queue = ctx.P_InvokeQueue;
				queueSpinLock = ctx.P_InvokeQueueSpinLock;
			}
			catch (ObjectDisposedException) {
				return;
			}
			var bypassUnhandledExceptionObserver = false;
			try {
				Exception caughtException1 = default;
				try {
					ctx.P_IsLoopAlive = true;
					startState.LoopStartedEvent.Set();
					//
					IVh<TCtxState> ctxState = default;
					Exception caughtException2 = default;
					try {
						// Создание объекта состояния контекста.
						//
						try {
							ctxState = stateFactory(arg: ct);
						}
						catch (Exception exception) {
							ctxState = new Vh<TCtxState>(exception: exception);
						}
						// Цикл обработки очереди.
						//
						for (; ; ) {
							if (runControl.HasStopRequested || ct.IsCancellationRequested)
								break;
							//
							P_InvokeItemBase queueItem = default;
							try {
								queueItem = queueSpinLock.Invoke(() => queue.Count > 0 ? queue.Dequeue() : null);
								if (!(queueItem is null)) {
									if (queueItem.IsInvokeCompleted)
										continue;
									else
										try {
											itrlck.Set(ref ctx._currentInvoke, queueItem);
											queueItem.Invoke(ctx: ctx, ctxState: ctxState);
										}
										catch (Exception exception) {
											if (bypassUnhandledExceptionObserver)
												throw;
											else {
												bool isExceptionObserved;
												try {
													isExceptionObserved = unhandledExceptionObserver.ObserveException(exception: exception, component: queueItem);
												}
												catch (Exception secondException) {
													bypassUnhandledExceptionObserver = true;
													throw new AggregateException(exception, secondException);
												}
												if (!isExceptionObserved) {
													bypassUnhandledExceptionObserver = true;
													throw;
												}
											}
										}
										finally {
											itrlck.SetNull(ref ctx._currentInvoke, queueItem);
										}
									continue;
								}
							}
							catch (Exception exception) {
								try { queueItem.TrySetInvokeResultAsCanceled(); }
								catch (Exception secondException) {
									throw new AggregateException(exception, secondException);
								}
								throw;
							}
							// Ожидание.
							//
							if (awakeLoopEvent.IsSet)
								awakeLoopEvent.Reset();
							else
								awakeLoopEvent.Wait(millisecondsTimeout: 937);
						}
					}
					catch (Exception exception) {
						caughtException2 = exception;
						throw;
					}
					finally {
						ctxState?.Dispose(exception: caughtException2);
					}
				}
				catch (Exception exception) {
					caughtException1 = exception;
					throw;
				}
				finally {
					ctx.P_IsLoopAlive = false;
					var rethrowExceptionList = new List<Exception>(collection: caughtException1.Sequence().SkipNull());
					while (true) {
						var queueItem = queueSpinLock.Invoke(() => queue.Count > 0 ? queue.Dequeue() : null);
						if (queueItem is null)
							break;
						else
							try { queueItem.TrySetInvokeResultAsCanceled(); }
							catch (Exception exception) {
								rethrowExceptionList.Add(exception);
							}
					}
					if (rethrowExceptionList.Count > (caughtException1 is null ? 0 : 1))
						throw new AggregateException(innerExceptions: rethrowExceptionList);
				}
			}
			catch (Exception exception) {
				if (bypassUnhandledExceptionObserver)
					throw;
				else {
					bool isExceptionObserved;
					try {
						isExceptionObserved = unhandledExceptionObserver.ObserveException(exception: exception, component: ctx);
					}
					catch (Exception secondException) {
						throw new AggregateException(exception, secondException);
					}
					if (!isExceptionObserved)
						throw;
				}
			}
		}

		// TODO: Put strings into the resources.
		//
		static EonException P_CreateRequiredPropertyMissingException(StatefulSynchronizationContext<TCtxState> ctx, string propName = default)
			=> new EonException(message: $"Required object (or property) is missing due to this component has not yet started or already stopped.{(string.IsNullOrEmpty(propName) ? string.Empty : $"{Environment.NewLine}\tProperty:{propName.FmtStr().GNLI2()}")}{Environment.NewLine}\tComponent:{ctx.FmtStr().GNLI2()}");

		#endregion

		bool _hasShutdownRequested;

		bool _hasShutdownFinished;

		TaskCompletionSource<Nil> _shutdownOp;

		EventHandler _eventHandler_ShutdownFinished;

		readonly XFullCorrelationId _correlationId;

		Func<CancellationToken, IVh<TCtxState>> _contextStateFactory;

		CancellationToken _stopToken;

		IUnhandledExceptionObserver _unhandledExceptionObserver;

		RunControl<StatefulSynchronizationContext<TCtxState>> _runControl;

		CancellationTokenSource _cts;

		ManualResetEventSlim _awakeLoopEvent;

		Queue<P_InvokeItemBase> _invokeQueue;

		PrimitiveSpinLock _invokeQueueSpinLock;

		Task _loopTask;

		int _isLoopAlive;

		P_InvokeItemBase _currentInvoke;

		readonly bool _autoStart;

		public StatefulSynchronizationContext(Func<CancellationToken, IVh<TCtxState>> stateFactory, CancellationToken stopToken = default, XFullCorrelationId correlationId = default, bool autoStart = default, IUnhandledExceptionObserver unhandledExceptionObserver = default) {
			stateFactory.EnsureNotNull(nameof(stateFactory));
			//
			if (correlationId is null)
				_correlationId = XCorrelationId.New();
			else
				_correlationId = correlationId + XCorrelationId.New();
			_contextStateFactory = stateFactory;
			_stopToken = stopToken;
			_autoStart = autoStart;
			_unhandledExceptionObserver = unhandledExceptionObserver ?? UnhandledExceptionObserverOption.Require();
			_runControl =
				new RunControl<StatefulSynchronizationContext<TCtxState>>(
					options: RunControlOptions.SingleStart | RunControlOptions.AutoStopOnTokenSignal,
					component: this,
					attemptState: null,
					beforeStart: null,
					start: P_DoStartAsync,
					stop: P_DoStopAsync,
					stopToken: stopToken);
		}

		Func<CancellationToken, IVh<TCtxState>> P_StateFactory
			=> ReadDA(ref _contextStateFactory);

		IUnhandledExceptionObserver P_UnhandledExceptionObserver
			=> ReadDA(ref _unhandledExceptionObserver);

		public XFullCorrelationId CorrelationId
			=> _correlationId;

		public bool AutoStart {
			get {
				EnsureNotDisposeState();
				return _autoStart;
			}
		}

		public IRunControl<IStatefulSynchronizationContext<TCtxState>> RunControl
			=> ReadDA(ref _runControl);

		public bool HasShutdownRequested
			=> vlt.Read(ref _hasShutdownRequested);

		public bool HasShutdownFinished
			=> vlt.Read(ref _hasShutdownFinished);

		CancellationToken P_Ct
			=> (ReadDA(ref _cts) ?? throw P_CreateRequiredPropertyMissingException(ctx: this, propName: nameof(_cts))).Token;

		ManualResetEventSlim P_AwakeLoopEvent
			=> ReadDA(ref _awakeLoopEvent) ?? throw P_CreateRequiredPropertyMissingException(ctx: this, propName: nameof(_awakeLoopEvent));

		Queue<P_InvokeItemBase> P_InvokeQueue
			=> ReadDA(ref _invokeQueue) ?? throw P_CreateRequiredPropertyMissingException(ctx: this, propName: nameof(_invokeQueue));

		PrimitiveSpinLock P_InvokeQueueSpinLock
			=> ReadDA(ref _invokeQueueSpinLock) ?? throw P_CreateRequiredPropertyMissingException(ctx: this, propName: nameof(_invokeQueueSpinLock));

		bool P_IsLoopAlive {
			get => Interlocked.CompareExchange(ref _isLoopAlive, 0, 0) == 1;
			set => Interlocked.Exchange(ref _isLoopAlive, value ? 1 : 0);
		}

		public event EventHandler ShutdownFinished {
			add { AddEventHandler(ref _eventHandler_ShutdownFinished, value); }
			remove { RemoveEventHandler(ref _eventHandler_ShutdownFinished, value); }
		}

		// TODO: Put strings into the resources.
		//
		Task P_DoStartAsync(IRunControlAttemptState state) {
			// TODO_HIGH: Этот таймаут должен браться из контекста запуска (когда будет реализован) как время оставшееся для выполнения операции запуска.
			//
			const int loopStartMillisecondsTimeout = 10000;
			//
			try {
				state.EnsureNotNull(nameof(state));
				//
				CancellationTokenSource cts = default;
				ManualResetEventSlim awakeLoopEvent = default;
				ManualResetEventSlim loopStartedEvent = default;
				try {
					CancellationTokenUtilities.LinkedOrNew(ct1: ReadDA(ref _stopToken), cts: out cts);
					WriteDA(ref _cts, cts);
					WriteDA(ref _awakeLoopEvent, awakeLoopEvent = new ManualResetEventSlim(initialState: false));
					WriteDA(ref _invokeQueue, new Queue<P_InvokeItemBase>());
					WriteDA(ref _invokeQueueSpinLock, new PrimitiveSpinLock());
					var loopTask =
						new Task(
							state: new P_LoopStartState(context: this, loopStartedEvent: loopStartedEvent = new ManualResetEventSlim(initialState: false)),
							action: P_Loop,
							creationOptions: TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);
					WriteDA(ref _loopTask, loopTask);
					loopTask.Start(scheduler: TaskScheduler.Default);
					if (!loopStartedEvent.Wait(millisecondsTimeout: loopStartMillisecondsTimeout)) {
						loopStartedEvent.Dispose();
						if (loopTask.IsCompleted)
							throw new EonException(message: "The execute loop didn't start at the allotted time. Start timed out." + (loopTask.IsCanceled ? $"{Environment.NewLine}The task servicing the loop was canceled." : string.Empty), innerException: loopTask.IsFaulted ? loopTask.Exception : null);
						else
							throw new TimeoutException(message: $"The execute loop didn't start at the allotted time. Start timed out.{Environment.NewLine}Status of the task servicing the loop:{loopTask.Status.FmtStr().GNLI()}");
					}
				}
				catch {
					awakeLoopEvent?.Dispose();
					cts?.Dispose();
					Interlocked.Exchange(ref _invokeQueueSpinLock, null);
					Interlocked.Exchange(ref _invokeQueue, null);
					Interlocked.Exchange(ref _loopTask, null);
					throw;
				}
				finally {
					loopStartedEvent?.Dispose();
				}
				//
				return TaskUtilities.FromVoidResult();
			}
			catch (Exception exception) {
				return TaskUtilities.FromError(error: exception);
			}
		}

		async Task P_DoStopAsync(IRunControlAttemptState state) {
			var cts = ReadDA(ref _cts);
			var awakeLoopEvent = ReadDA(ref _awakeLoopEvent);
			var loopTask = ReadDA(ref _loopTask);
			var caughtExceptions = new List<Exception>();
			//
			try {
				cts.Cancel(throwOnFirstException: false);
			}
			catch (Exception exception) { caughtExceptions.Add(exception); }
			try {
				awakeLoopEvent.Set();
			}
			catch (Exception exception) { caughtExceptions.Add(exception); }
			try {
				await loopTask.ConfigureAwait(false);
			}
			catch (Exception exception) { caughtExceptions.Add(exception); }
			try {
				Interlocked.Exchange(ref _awakeLoopEvent, null)?.Dispose();
				Interlocked.Exchange(ref _cts, null)?.Dispose();
			}
			catch (Exception exception) { caughtExceptions.Add(exception); }
			//
			Interlocked.Exchange(ref _invokeQueueSpinLock, null);
			Interlocked.Exchange(ref _invokeQueue, null);
			Interlocked.Exchange(ref _loopTask, null);
			//
			if (caughtExceptions.Count > 0)
				throw new AggregateException(innerExceptions: caughtExceptions);
		}

		public void ExecuteOneWay<TState>(StatefulSynchronizationContextExecuteOneWay<TCtxState, TState> action, TState state, CancellationToken ct = default) {
			action.EnsureNotNull(nameof(action));
			//
			IRunControl runControl;
			if (ct.IsCancellationRequested)
				throw new OperationCanceledException(token: ct);
			else if ((runControl = RunControl).HasStopRequested)
				throw new OperationCanceledException();
			else if (!runControl.IsStarted) {
				if (_autoStart)
					runControl.StartAsync(ctx: default).WaitAsync(ct: ct).WaitWithTimeout();
				else
					using (var locCtx = ContextUtilities.Create(ct: ct))
						runControl.WaitStartCompletionAsync(ctx: locCtx).WaitWithTimeout();
			}
			P_ExecuteAsync(@delegate: doInvoke, state: state, ct: ct, oneWay: true);
			//
			Nil doInvoke(IStatefulSynchronizationContext<TCtxState> locCtx, IVh<TCtxState> locCtxState, TState locState, CancellationToken locCt) {
				action(ctx: locCtx, ctxState: locCtxState, state: locState, ct: ct);
				return Nil.Value;
			};
		}

		public async Task<TResult> ExecuteAsync<TState, TResult>(StatefulSynchronizationContextExecute<TCtxState, TState, TResult> @delegate, TState state, CancellationToken ct = default)
			=> await P_ExecuteAsync(@delegate: @delegate, state: state, ct: ct, oneWay: false).ConfigureAwait(false);

		// TODO: Put strings into the resources.
		//
		async Task<TResult> P_ExecuteAsync<TState, TResult>(StatefulSynchronizationContextExecute<TCtxState, TState, TResult> @delegate, TState state, CancellationToken ct, bool oneWay) {
			var unobserverExceptionHandler = default(IUnhandledExceptionObserver);
			var invokeItem = default(P_InvokeItem<TState, TResult>);
			try {
				unobserverExceptionHandler = P_UnhandledExceptionObserver;
				IRunControl runControl;
				if (ct.IsCancellationRequested)
					throw new OperationCanceledException(token: ct);
				else if ((runControl = RunControl).HasStopRequested)
					throw new OperationCanceledException();
				else if (!runControl.IsStarted) {
					if (_autoStart)
						await runControl.StartAsync(ctx: default).WaitAsync(ct: ct).ConfigureAwait(false);
					else
						using (var locCtx = ContextUtilities.Create(ct: ct))
							await runControl.WaitStartCompletionAsync(ctx: locCtx).ConfigureAwait(false);
				}
				if (!P_IsLoopAlive)
					throw new EonException(message: $"The execute loop is not active.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
				else {
					Task<TResult> invokeItemAwaitable;
					try {
						var contextCt = P_Ct;
						var queue = P_InvokeQueue;
						var queueSpinLock = P_InvokeQueueSpinLock;
						var awakeLoopEvent = P_AwakeLoopEvent;
						//
						invokeItem = new P_InvokeItem<TState, TResult>(ctx: this, @delegate: @delegate, userState: state, contextCt: contextCt, userCt: ct);
						var enqueueResult =
							queueSpinLock
							.Invoke(
								() => {
									if (runControl.HasStopRequested)
										return null;
									else {
										EnsureNotDisposeState(considerDisposeRequest: true);
										queue.Enqueue(item: invokeItem);
										return invokeItem;
									}
								});
						if (enqueueResult is null) {
							invokeItem.TrySetInvokeResultAsCanceled();
							throw new OperationCanceledException(token: contextCt);
						}
						else if (!P_IsLoopAlive)
							throw new EonException(message: $"The execute loop is not active.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
						else {
							awakeLoopEvent.Set();
							invokeItemAwaitable = invokeItem.UserInvokeAwaitable;
						}
					}
					catch {
						invokeItem?.TrySetInvokeResultAsCanceled();
						throw;
					}
					return await invokeItemAwaitable.ConfigureAwait(false);
				}
			}
			catch (Exception exception) when (oneWay && !exception.IsOperationCancellation()) {
				if (!(unobserverExceptionHandler?.ObserveException(exception: exception, component: (object)invokeItem ?? this) ?? false))
					throw;
				else
					return typeof(TResult) == Nil.Type ? (TResult)(object)Nil.Value : default;
			}
		}

		public void TryCancelInvokes() {
			var spinLock = TryReadDA(ref _invokeQueueSpinLock);
			var queue = TryReadDA(ref _invokeQueue, considerDisposeRequest: true);
			if (!(queue is null)) {
				spinLock.Invoke(() => queue.TryDequeueAll()).Observe(locItem => locItem.TrySetInvokeResultAsCanceled());
			}
			TryReadDA(ref _currentInvoke, considerDisposeRequest: true)?.TryCancel();
		}

		public Task ShutdownAsync() {
			if (HasShutdownFinished)
				return Task.CompletedTask;
			else {
				TaskCompletionSource<Nil> existingOp = default;
				TaskCompletionSource<Nil> newOp = default;
				try {
					existingOp = itrlck.Get(ref _shutdownOp) ?? WriteDA(location: ref _shutdownOp, value: newOp = new TaskCompletionSource<Nil>(creationOptions: TaskCreationOptions.None), comparand: null) ?? newOp;
					//
					if (ReferenceEquals(newOp, existingOp)) {
						vlt.Write(location: ref _hasShutdownRequested, value: true);
						TaskUtilities.RunOnDefaultScheduler(factory: doShutdownAsync).ContinueWithTryApplyResultTo(taskProxy: newOp);
						newOp
							.Task
							.ContinueWith(
								continuationAction:
									locTask => {
										if (!locTask.IsCanceled)
											vlt.Write(location: ref _hasShutdownFinished, value: true);
										itrlck.SetNullBool(location: ref _shutdownOp, comparand: newOp);
									},
								continuationOptions: TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.NotOnFaulted);
					}
					return existingOp.Task;
				}
				catch (Exception exception) {
					if (newOp?.TrySetException(exception) == true)
						return newOp.Task;
					else
						return TaskUtilities.FromError(exception);
				}
				finally {
					if (!ReferenceEquals(existingOp, newOp))
						newOp?.TrySetCanceled();
				}
			}
			//
			async Task doShutdownAsync() {
				await ReadDA(ref _runControl).StopAsync(finiteStop: true).ConfigureAwait(false);
				var shutdownFinishedEventHandler = ReadDA(ref _eventHandler_ShutdownFinished);
				Dispose();
				// TODO_HIGH: Возможно, имеет смысл вызвать событие асинхронно.
				//
				shutdownFinishedEventHandler?.Invoke(sender: this, e: EventArgs.Empty);
			}
		}

		public override string ToString()
			=> $"{nameof(StatefulSynchronizationContext<TCtxState>)}, {_correlationId.Value}";

		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose) {
				TryReadDA(ref _runControl)?.StopAsync().WaitWithTimeout();
			}
			//
			base.FireBeforeDispose(explicitDispose);
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_invokeQueueSpinLock?.EnterAndExitLock();
				_invokeQueue?.Clear();
				_runControl?.Dispose();
				_cts?.Dispose();
				_awakeLoopEvent?.Dispose();
			}
			_invokeQueueSpinLock = null;
			_invokeQueue = null;
			_cts = null;
			_awakeLoopEvent = null;
			_contextStateFactory = null;
			_loopTask = null;
			_runControl = null;
			_unhandledExceptionObserver = null;
			_stopToken = default;
			//
			_eventHandler_ShutdownFinished = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}