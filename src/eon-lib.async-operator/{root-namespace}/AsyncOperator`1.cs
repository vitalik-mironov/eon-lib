using System;
using System.Threading;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Diagnostics;
using Eon.Runtime.Options;
using Eon.Threading;
using Eon.Threading.Tasks;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon {

	/// <summary>
	/// Provides support for lazy asynchronous operation.
	/// </summary>
	/// <typeparam name="TResult">Type of operation result.</typeparam>
	public sealed class AsyncOperator<TResult>
		:Disposable, IAsyncOperator<TResult> {

		#region Nested types

		sealed class P_SessionObject
			:Disposable {

			const int __ExecutionStateFlag_None = 0;

			const int __ExecutionStateFlag_AboutExecution = 1;

			const int __ExecutionStateFlag_Completed = 2;

			const int __ExecutionStateFlag_Prevent = 4;

			#region Static members

			// TODO: Put strings into the resources.
			//
			static void P_ProvideResult(P_SessionObject session, TaskCompletionSource<TResult> resultConsumer, IContext ctx = default) {
				session.EnsureNotNull(nameof(session));
				resultConsumer.EnsureNotNull(nameof(resultConsumer));
				//
				var unhandledExceptionObserver = session.P_UnhandledExceptionObserver;
				var ct = ctx.Ct();
				var ctRegistration = default(CancellationTokenRegistration?);
				resultConsumer.Task.ContinueWith(continuationAction: locTask => itrlck.Or(ref session._executionStateFlag, __ExecutionStateFlag_Completed), continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
				resultConsumer.Task.ContinueWith(continuationAction: locTask => ctRegistration?.Dispose(), continuationOptions: TaskContinuationOptions.PreferFairness);
				//
				var previousExecutionStateFlag = itrlck.Or(location: ref session._executionStateFlag, value: __ExecutionStateFlag_AboutExecution);
				if (previousExecutionStateFlag == __ExecutionStateFlag_None) {
					// Флаг отмены до начала выполнения не был установлен.
					//
					if (ct.IsCancellationRequested) {
						if (session.P_TrySetResultAsCanceled(ct))
							resultConsumer.TrySetCanceled();
					}
					else {
						if (ct.CanBeCanceled)
							ctRegistration = ct.Register(callback: () => { if (session.P_TrySetResultAsCanceled(ct)) resultConsumer.TrySetCanceled(); });
						//
						var factoryTask =
							Task
							.Factory
							.StartNew(
								function:
									() => {
										// Собственно, здесь выполняется делегат операции, указанный одним из конструкторов AsyncOperator`1.
										//
										var locFactoryTask = session.P_Factory(arg: ctx);
										if (locFactoryTask is null)
											throw
												new EonException(message: $"Factory method has returned invalid result '{FormatStringUtilitiesCoreL0.GetNullValueText()}'.{Environment.NewLine}\tMethod:{session.P_Factory.FmtStr().GNLI2()}");
										return locFactoryTask;
									},
								cancellationToken: ct,
								creationOptions: TaskCreationOptions.PreferFairness,
								scheduler: TaskScheduler.Default)
							.Unwrap();
						var factoryTaskResultHandlingTask =
							factoryTask
							.ContinueWith(
								continuationAction:
									locTask => {
										if (locTask.IsCanceled) {
											if (session.P_TrySetResultAsCanceled(ct: CancellationToken.None))
												resultConsumer.TrySetCanceled(cancellationToken: CancellationToken.None);
										}
										else if (locTask.IsFaulted) {
											var locTaskException = locTask.Exception.Flatten();
											try {
												if (session.P_TrySetResultAsFaulted(locTaskException)) {
													resultConsumer.TrySetException(locTaskException);
													return;
												}
											}
											catch (Exception locException) {
												throw new AggregateException(locTaskException, locException);
											}
											throw new AggregateException(locTaskException);
										}
										else if (session.P_TrySetOrDisposeResult(locTask.Result))
											resultConsumer.TrySetResult(locTask.Result);
									},
								cancellationToken: CancellationToken.None,
								continuationOptions: TaskContinuationOptions.PreferFairness,
								scheduler: TaskScheduler.Default);
						if (!(unhandledExceptionObserver is null || ReferenceEquals(objA: unhandledExceptionObserver, objB: NopUnhandledExceptionObserver.Instance)))
							factoryTaskResultHandlingTask
								.ContinueWith(
									continuationAction:
										locTask => {
											var locTaskException = locTask.Exception.Flatten();
											try {
												if (unhandledExceptionObserver.ObserveException(exception: locTaskException, component: session, message: $"An exception was thrown by async operator ({nameof(AsyncOperator<TResult>)}) session.", exceptionCtx: ctx))
													return;
											}
											catch (Exception locException) {
												throw new AggregateException(locTaskException, locException);
											}
											throw new AggregateException(locTaskException);
										},
									continuationOptions: TaskContinuationOptions.OnlyOnFaulted,
									cancellationToken: default,
									scheduler: TaskScheduler.Default);
					}
				}
				else if (session.P_TrySetResultAsCanceled(ct: CancellationToken.None))
					// Был ранее установлен флаг отмены выполнения.
					//
					resultConsumer.TrySetCanceled();
			}

			#endregion

			readonly bool _ownsResult;

			Func<IContext, Task<TResult>> _factory;

			IVh<TResult> _resultHolder;

			int _executionStateFlag;

			TaskCompletionSource<TResult> _executionTaskProxy;

			IUnhandledExceptionObserver _unhandledExceptionObserver;

			internal P_SessionObject(Func<IContext, Task<TResult>> factory, bool ownsResult, IUnhandledExceptionObserver unhandledExceptionObserver) {
				factory.EnsureNotNull(nameof(factory));
				//
				_factory = factory;
				_ownsResult = ownsResult;
				_unhandledExceptionObserver = unhandledExceptionObserver;
			}

			internal P_SessionObject(IVh<TResult> result) {
				result.EnsureNotNull(nameof(result));
				//
				_resultHolder = result;
				_executionStateFlag = __ExecutionStateFlag_AboutExecution | __ExecutionStateFlag_Completed;
				_unhandledExceptionObserver = null;
			}

			IUnhandledExceptionObserver P_UnhandledExceptionObserver
				=> ReadDA(ref _unhandledExceptionObserver);

			Func<IContext, Task<TResult>> P_Factory
				=> ReadDA(ref _factory);

			int P_ExecutionStateFlags
				=> Interlocked.CompareExchange(ref _executionStateFlag, __ExecutionStateFlag_None, __ExecutionStateFlag_None);

			public bool IsCompleted
				=> (P_ExecutionStateFlags & __ExecutionStateFlag_Completed) == __ExecutionStateFlag_Completed;

			public bool HasPrevention
				=> (P_ExecutionStateFlags & __ExecutionStateFlag_Prevent) == __ExecutionStateFlag_Prevent;

			// TODO: Put strings into the resources.
			//
			public TResult Result {
				get {
					if (IsCompleted)
						return ReadDA(ref _resultHolder).Value;
					else
						throw new EonException("Операция еще не завершена.");
				}
			}

			void P_TrySetResultHolder(IVh<TResult> result, out bool resultOwnership) {
				var exchange = result;
				try {
					if (!IsDisposeRequested)
						try {
							exchange = WriteDA(location: ref _resultHolder, value: result, comparand: null);
						}
						catch (ObjectDisposedException) { }
				}
				finally {
					resultOwnership = exchange is null || ReferenceEquals(itrlck.Get(location: ref _resultHolder), result);
				}
			}

			bool P_TrySetOrDisposeResult(TResult result) {
				if (IsDisposeRequested) {
					if (_ownsResult)
						(result as IDisposable)?.Dispose();
					return false;
				}
				else {
					var resultOwnershipTaken = false;
					var resultHolder = default(IVh<TResult>);
					try {
						resultHolder = result.ToValueHolder(ownsValue: _ownsResult);
						P_TrySetResultHolder(result: resultHolder, resultOwnership: out resultOwnershipTaken);
						return resultOwnershipTaken;
					}
					finally {
						if (!resultOwnershipTaken)
							resultHolder?.Dispose();
					}
				}
			}

			bool P_TrySetResultAsFaulted(Exception fault) {
				fault.EnsureNotNull(nameof(fault));
				//
				if (IsDisposeRequested)
					return false;
				else {
					var resultOwnershipTaken = false;
					var resultHolder = default(Vh<TResult>);
					try {
						resultHolder = new Vh<TResult>(exception: fault);
						P_TrySetResultHolder(result: resultHolder, resultOwnership: out resultOwnershipTaken);
						return resultOwnershipTaken;
					}
					finally {
						if (!resultOwnershipTaken)
							resultHolder?.Dispose();
					}
				}
			}

			// TODO: Put strings into the resources.
			//
			bool P_TrySetResultAsCanceled(CancellationToken ct) {
				if (IsDisposeRequested)
					return false;
				else {
					var resultOwnershipTaken = false;
					var resultStore = default(Vh<TResult>);
					try {
						resultStore = new Vh<TResult>(exception: new OperationCanceledException(message: "Операция была отменена.", token: ct));
						P_TrySetResultHolder(result: resultStore, resultOwnership: out resultOwnershipTaken);
						return resultOwnershipTaken;
					}
					finally {
						if (!resultOwnershipTaken)
							resultStore?.Dispose();
					}
				}
			}

			public Task<TResult> ExecuteAsync(IContext ctx = default) {
				try {
					if (IsCompleted)
						return ReadDA(ref _resultHolder, considerDisposeRequest: true).Awaitable().Unwrap();
					else {
						var ct = ctx.Ct();
						var thisCallTask = default(TaskCompletionSource<TResult>);
						try {
							thisCallTask = new TaskCompletionSource<TResult>(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);
							if (ct.CanBeCanceled) {
								var ctRegistration = ct.Register(callback: () => thisCallTask.TrySetCanceled());
								thisCallTask.Task.ContinueWith(continuationAction: locTask => ctRegistration.Dispose(), continuationOptions: TaskContinuationOptions.PreferFairness);
							}
							var existingExecutionTask = ReadDA(ref _executionTaskProxy, considerDisposeRequest: true);
							if (existingExecutionTask is null) {
								var newExecutionTask = default(TaskCompletionSource<TResult>);
								try {
									newExecutionTask = new TaskCompletionSource<TResult>(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);
									//
									existingExecutionTask = UpdDAIfNull(location: ref _executionTaskProxy, value: newExecutionTask);
									if (ReferenceEquals(existingExecutionTask, newExecutionTask))
										P_ProvideResult(session: this, ctx: ctx, resultConsumer: existingExecutionTask);
								}
								finally {
									if (!ReferenceEquals(existingExecutionTask, newExecutionTask))
										newExecutionTask?.TrySetCanceled();
								}
							}
							existingExecutionTask.Task.ContinueWithTryApplyResultTo(taskProxy: thisCallTask, options: TaskContinuationOptions.PreferFairness);
							return thisCallTask.Task;
						}
						catch (Exception exception) {
							if (thisCallTask?.TrySetException(exception) == true)
								return thisCallTask.Task;
							else
								return Task.FromException<TResult>(exception);
						}
					}
				}
				catch (Exception exception) {
					return Task.FromException<TResult>(exception: exception);
				}
			}

			public bool TrySetPrevention() {
				EnsureNotDisposeState(considerDisposeRequest: true);
				// Здесь именно устанавливается флаг отмены до начала выполнения, если никокой другой флаг, отражающий ход выполнения (начала, завершения), еще не установлен.
				//
				return Interlocked.CompareExchange(ref _executionStateFlag, __ExecutionStateFlag_Prevent, __ExecutionStateFlag_None) == __ExecutionStateFlag_None;
			}

			protected override void FireBeforeDispose(bool explicitDispose) {
				if (explicitDispose)
					Interlocked.CompareExchange(ref _executionStateFlag, __ExecutionStateFlag_Prevent, __ExecutionStateFlag_None);
				//
				base.FireBeforeDispose(explicitDispose);
			}

			protected override void Dispose(bool explicitDispose) {
				if (explicitDispose) {
					_resultHolder?.Dispose();
					_executionTaskProxy?.TrySetCanceled();
				}
				_executionTaskProxy = null;
				_resultHolder = null;
				_factory = null;
				//
				base.Dispose(explicitDispose);
			}

		}

		#endregion

		#region Static members

		static readonly Type __ResultTypeConstraint = typeof(TResult);

		static Func<IContext, Task<TResult>> P_ResultFactoryConvert(Func<Task<TResult>> factory) {
			factory.EnsureNotNull(nameof(factory));
			//
			return locFactory;
			//
			async Task<TResult> locFactory(IContext locCtx)
				=> await factory().ConfigureAwait(false);
		}

		static Func<IContext, TResult> P_ResultFactoryConvert(Func<TResult> factory) {
			factory.EnsureNotNull(nameof(factory));
			//
			return locCtx => factory();
		}

		#endregion

		readonly bool _ownsResult;

		readonly bool _isReexecutable;

		Func<IContext, TResult> _auxiliaryFactory;

		Func<IContext, Task<TResult>> _executableFactory;

		P_SessionObject _session;

		IUnhandledExceptionObserver _unhandledExceptionObserver;

		public AsyncOperator(Func<Task<TResult>> asyncFactory, IUnhandledExceptionObserver unhandledExceptionObserver = default)
			: this(asyncFactory: P_ResultFactoryConvert(asyncFactory), ownsResult: false, isReexecutable: false, unhandledExceptionObserver: unhandledExceptionObserver) { }

		public AsyncOperator(Func<Task<TResult>> asyncFactory, bool ownsResult, IUnhandledExceptionObserver unhandledExceptionObserver = default)
			: this(asyncFactory: P_ResultFactoryConvert(asyncFactory), ownsResult: ownsResult, isReexecutable: false, unhandledExceptionObserver: unhandledExceptionObserver) { }

		public AsyncOperator(Func<Task<TResult>> asyncFactory, bool ownsResult, bool isReexecutable, IUnhandledExceptionObserver unhandledExceptionObserver = default)
			: this(asyncFactory: P_ResultFactoryConvert(asyncFactory), ownsResult: ownsResult, isReexecutable: isReexecutable, unhandledExceptionObserver: unhandledExceptionObserver) { }

		public AsyncOperator(Func<IContext, Task<TResult>> asyncFactory, IUnhandledExceptionObserver unhandledExceptionObserver = default)
			: this(asyncFactory: asyncFactory, ownsResult: false, isReexecutable: false, unhandledExceptionObserver: unhandledExceptionObserver) { }

		public AsyncOperator(Func<IContext, Task<TResult>> asyncFactory, bool ownsResult, IUnhandledExceptionObserver unhandledExceptionObserver = default)
			: this(asyncFactory: asyncFactory, ownsResult: ownsResult, isReexecutable: false, unhandledExceptionObserver: unhandledExceptionObserver) { }

		public AsyncOperator(Func<TResult> factory, IUnhandledExceptionObserver unhandledExceptionObserver = default)
			: this(factory: P_ResultFactoryConvert(factory), ownsResult: false, isReexecutable: false, unhandledExceptionObserver: unhandledExceptionObserver) { }

		public AsyncOperator(Func<TResult> factory, bool ownsResult, IUnhandledExceptionObserver unhandledExceptionObserver = default)
			: this(factory: P_ResultFactoryConvert(factory), ownsResult: ownsResult, isReexecutable: false, unhandledExceptionObserver: unhandledExceptionObserver) { }

		public AsyncOperator(Func<TResult> factory, bool ownsResult, bool isReexecutable, IUnhandledExceptionObserver unhandledExceptionObserver = default)
			: this(factory: P_ResultFactoryConvert(factory), ownsResult: ownsResult, isReexecutable: isReexecutable, unhandledExceptionObserver: unhandledExceptionObserver) { }

		public AsyncOperator(Func<IContext, TResult> factory, IUnhandledExceptionObserver unhandledExceptionObserver = default)
			: this(factory: factory, ownsResult: false, isReexecutable: false, unhandledExceptionObserver: unhandledExceptionObserver) { }

		public AsyncOperator(Func<IContext, TResult> factory, bool ownsResult, IUnhandledExceptionObserver unhandledExceptionObserver = default)
			: this(factory: factory, ownsResult: ownsResult, isReexecutable: false, unhandledExceptionObserver: unhandledExceptionObserver) { }

		public AsyncOperator(TResult result)
			: this(result: result, ownsResult: false) { }

		public AsyncOperator(Func<IContext, Task<TResult>> asyncFactory, bool ownsResult, bool isReexecutable, IUnhandledExceptionObserver unhandledExceptionObserver = default) {
			asyncFactory.EnsureNotNull(nameof(asyncFactory));
			//
			_ownsResult = ownsResult;
			_isReexecutable = isReexecutable;
			_executableFactory = asyncFactory;
			_unhandledExceptionObserver = unhandledExceptionObserver ?? UnhandledExceptionObserverOption.Require();
			//
			_session = new P_SessionObject(factory: asyncFactory, ownsResult: ownsResult, unhandledExceptionObserver: _unhandledExceptionObserver);
		}

		public AsyncOperator(Func<IContext, TResult> factory, bool ownsResult, bool isReexecutable, IUnhandledExceptionObserver unhandledExceptionObserver = default) {
			factory.EnsureNotNull(nameof(factory));
			//
			_ownsResult = ownsResult;
			_isReexecutable = isReexecutable;
			_auxiliaryFactory = factory;
			_executableFactory =
				locCtx =>
					Task
					.Factory
					.StartNew(
						function: locTaskState => ReadDA(ref _auxiliaryFactory, considerDisposeRequest: true)((IContext)locTaskState),
						state: locCtx,
						cancellationToken: locCtx.Ct(),
						creationOptions: TaskCreationOptions.PreferFairness,
						scheduler: TaskScheduler.Default);
			_unhandledExceptionObserver = unhandledExceptionObserver ?? UnhandledExceptionObserverOption.Require();
			//
			_session = new P_SessionObject(factory: _executableFactory, ownsResult: ownsResult, unhandledExceptionObserver: _unhandledExceptionObserver);
		}

		public AsyncOperator(TResult result, bool ownsResult) {
			_ownsResult = ownsResult;
			_isReexecutable = false;
			_unhandledExceptionObserver = null;
			//
			_session = new P_SessionObject(result: result.ToValueHolder(ownsValue: ownsResult));
		}

		Func<IContext, Task<TResult>> P_ExecutableFactory
			=> ReadDA(ref _executableFactory);

		public bool OwnsResult {
			get {
				EnsureNotDisposeState();
				return _ownsResult;
			}
		}

		public bool IsCompleted
			=> ReadDA(ref _session).IsCompleted;

		public bool IsReexecutable
			=> _isReexecutable;

		public TResult Result
			=> ReadDA(ref _session).Result;

		public Type ResultTypeConstraint
			=> __ResultTypeConstraint;

		public bool HasPrevention
			=> ReadDA(ref _session).HasPrevention;

		public async Task<TResult> ExecuteAsync(CancellationToken ct) {
			if (ct.CanBeCanceled) {
				ct.ThrowExceptionIfCancellationRequested();
				using (var ctx = ContextUtilities.Create(ct: ct))
					return await ReadDA(location: ref _session, considerDisposeRequest: true).ExecuteAsync(ctx: ctx).ConfigureAwait(false);
			}
			else
				return await ReadDA(location: ref _session, considerDisposeRequest: true).ExecuteAsync(ctx: default).ConfigureAwait(false);
		}

		public async Task<TResult> ExecuteAsync(IContext ctx = default)
			=> await ReadDA(location: ref _session, considerDisposeRequest: true).ExecuteAsync(ctx: ctx).ConfigureAwait(false);

		ITaskWrap<TResult> IAsyncOperator<TResult>.ExecuteAsync(IContext ctx)
			=> TaskUtilities.Wrap(factory: async () => await ExecuteAsync(ctx: ctx).ConfigureAwait(false));

		ITaskWrap<TResult> IAsyncOperator<TResult>.ExecuteAsync(CancellationToken ct)
			=> TaskUtilities.Wrap(factory: async () => await ExecuteAsync(ct: ct).ConfigureAwait(false));

		// TODO: Put strings into the resources.
		//
		public Task<TResult> ReexecuteAsync(IContext ctx = default) {
			try {
				if (IsReexecutable) {
					var executableFactory = P_ExecutableFactory;
					var ownsResult = OwnsResult;
					var unhandledExceptionObserver = ReadDA(location: ref _unhandledExceptionObserver, considerDisposeRequest: true);
					var session = default(P_SessionObject);
					var newSession = default(P_SessionObject);
					var caughtException = default(Exception);
					try {
						P_SessionObject existingSession;
						//
						for (; ; ) {
							existingSession = ReadDA(ref _session, considerDisposeRequest: true);
							if (existingSession.IsCompleted && !existingSession.HasPrevention) {
								newSession = newSession ?? new P_SessionObject(factory: executableFactory, ownsResult: ownsResult, unhandledExceptionObserver: unhandledExceptionObserver);
								var previousSession = WriteDA(ref _session, newSession, comparand: existingSession);
								if (ReferenceEquals(objA: existingSession, objB: previousSession)) {
									session = newSession;
									// Асинхронно выгрузить "старую" сессию.
									//
									Task.Factory.StartNew(action: previousSession.Dispose, cancellationToken: CancellationToken.None, creationOptions: TaskCreationOptions.PreferFairness, scheduler: TaskScheduler.Default);
									break;
								}
							}
							else {
								session = existingSession;
								break;
							}
						}
						return session.ExecuteAsync(ctx: ctx);
					}
					catch (Exception exception) {
						caughtException = exception;
						throw;
					}
					finally {
						if (!ReferenceEquals(session, newSession))
							newSession?.Dispose(caughtException);
					}
				}
				else
					throw new EonException(message: $"Repeated execution not allowed for this operation.{Environment.NewLine}\tOperation:{this.FmtStr().GNLI2()}");
			}
			catch (Exception exception) {
				return Task.FromException<TResult>(exception);
			}
		}

		public async Task<TResult> ReexecuteAsync(CancellationToken ct) {
			if (ct.CanBeCanceled) {
				ct.ThrowExceptionIfCancellationRequested();
				using (var ctx = ContextUtilities.Create(ct: ct))
					return await ReexecuteAsync(ctx: ctx).ConfigureAwait(false);
			}
			else
				return await ReexecuteAsync(ctx: default).ConfigureAwait(false);
		}

		ITaskWrap<TResult> IAsyncOperator<TResult>.ReexecuteAsync(IContext ctx)
			=> TaskUtilities.Wrap(factory: async () => await ReexecuteAsync(ctx: ctx).ConfigureAwait(false));

		ITaskWrap<TResult> IAsyncOperator<TResult>.ReexecuteAsync(CancellationToken ct)
			=> TaskUtilities.Wrap(factory: async () => await ReexecuteAsync(ct: ct).ConfigureAwait(false));

		public bool TrySetPrevention()
			=> ReadDA(ref _session, considerDisposeRequest: true).TrySetPrevention();

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose)
				_session?.Dispose();
			_session = null;
			_executableFactory = null;
			_auxiliaryFactory = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}