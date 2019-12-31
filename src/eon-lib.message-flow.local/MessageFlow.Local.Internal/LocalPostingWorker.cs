using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel;
using Eon.Context;
using Eon.Diagnostics;
using Eon.Threading;
using Eon.Threading.Tasks;

namespace Eon.MessageFlow.Local.Internal {

	internal sealed class LocalPostingWorker
		:Disposable, ILocalPostingWorker {

		#region Nested types

		sealed class P_LoopStartState {

			public readonly LocalPostingWorker Worker;

			public readonly ManualResetEventSlim LoopStartedEvent;

			internal P_LoopStartState(LocalPostingWorker worker, ManualResetEventSlim loopStartedEvent) {
				worker.EnsureNotNull(nameof(worker));
				loopStartedEvent.EnsureNotNull(nameof(loopStartedEvent));
				//
				Worker = worker;
				LoopStartedEvent = loopStartedEvent;
			}

		}

		sealed class P_LoopSingleItemPostingState {

			readonly LocalPostingWorker _worker;

			readonly ILocalSubscription _subscription;

			long _startTimestamp;

			long _completeTimestamp;

			internal P_LoopSingleItemPostingState(LocalPostingWorker worker, ILocalSubscription subscription) {
				worker.EnsureNotNull(nameof(worker));
				subscription.EnsureNotNull(nameof(subscription));
				//
				_worker = worker;
				_subscription = subscription;
				_startTimestamp = -1L;
				_completeTimestamp = -1L;
			}

			public void OnStarted() {
				_worker.P_OnLoopPostingStarted(operation: this);
				Interlocked.CompareExchange(ref _startTimestamp, StopwatchUtilities.GetTimestampAsTicks(), -1L);
			}

			public void OnCompleted() {
				Interlocked.CompareExchange(ref _completeTimestamp, StopwatchUtilities.GetTimestampAsTicks(), -1L);
				_worker.P_OnLoopPostingCompleted(operation: this);
			}

			public TimeSpan Duration {
				get {
					var startTimestamp = Interlocked.Read(ref _startTimestamp);
					if (startTimestamp == -1L)
						return TimeSpan.Zero;
					else {
						var completeTimestamp = Interlocked.Read(ref _completeTimestamp);
						if (completeTimestamp == -1L)
							return new TimeSpan(ticks: StopwatchUtilities.GetTimestampAsTicks() - startTimestamp);
						else
							return new TimeSpan(ticks: completeTimestamp - startTimestamp);
					}
				}
			}

			public ILocalSubscription Subscription
				=> _subscription;

		}

		#endregion

		#region Static members

#if TRG_NETFRAMEWORK
		static readonly string __ThisTypeFullName = typeof(LocalPostingWorker).FullName;
#endif

		static void P_Loop(P_LoopStartState startState) {
			startState.EnsureNotNull(nameof(startState));
			//
			var worker = startState.Worker;
			try {
				worker.P_IsLoopAlive = true;
				startState.LoopStartedEvent.Set();
				//
				var ct = worker.P_CancellationToken;
				var breakIdleEvent = worker.P_BreakLoopIdleEvent;
				var queue = worker.P_Queue;
				var queueSpinLock = worker.P_QueueSpinLock;
				var cancellationEvent = ct.WaitHandle;
				var events = new[ ] { cancellationEvent, breakIdleEvent };
				for (; !ct.IsCancellationRequested;) {
					var currentQueueEntry =
						queueSpinLock
							.Invoke(
								() => {
									if (queue.Count > 0)
										return queue.Peek();
									else
										return null;
								});
					if (currentQueueEntry == null) {
						// Очередь пуста. Переход в состояние простоя (ожидания сигнала либо отмены, либо изменения очереди).
						//
						WaitHandle signaledEvent;
						try {
							worker.P_IsLoopIdling = true;
							//
							signaledEvent = events.WaitAny(Timeout.Infinite);
						}
						catch (Exception exception) {
							if (exception is ThreadInterruptedException || (exception is ObjectDisposedException && ct.IsCancellationRequested))
								signaledEvent = cancellationEvent;
							else
								throw;
						}
						finally {
							worker.P_IsLoopIdling = false;
						}
						if (signaledEvent == cancellationEvent || ct.IsCancellationRequested)
							break;
						else
							continue;
					}
					else if (ct.IsCancellationRequested)
						break;
					else
						// Обработка элемента (доставка подписчикам).
						//
						for (; !ct.IsCancellationRequested;) {
							if (currentQueueEntry.TryGetNextPostingSubscription(out var postingToken, out var postingSubscription)) {
								if (postingSubscription.IsActive) {
									// Доставка подписчику.
									//
									var itemPostingState = new P_LoopSingleItemPostingState(worker: worker, subscription: postingSubscription);
									try {
										itemPostingState.OnStarted();
										P_DoSingleMessagePostingAsync(token: postingToken, subscription: postingSubscription, ct: ct).Wait();
										itemPostingState.OnCompleted();
									}
									catch {
										try { itemPostingState.OnCompleted(); }
										catch { }
										//
#if TRG_NETFRAMEWORK
										WindowsEventLogUtilities
											.WriteFaultToEventLog(
												fault: exception,
												failFastOnError: true,
												faultFormattingOptions: ExceptionInfoFormattingOptions.Full,
												message: $"Ошибка обработки доставки сообщения.{Environment.NewLine}\tБлок перехвата ошибки:{($"{__ThisTypeFullName}.{nameof(P_Loop)}").FmtStr().GNLI2()}");
#else
										// TODO_HIGH: Implement error logging (or something else).
										//
										// ...
#endif
									}
								}
							}
							else {
								// Элемент уже полностью обработан.
								//
								if (!currentQueueEntry.IsDisposeRequested)
									currentQueueEntry.Dispose();
								queueSpinLock
									.Invoke(
										() => {
											if (queue.Count > 0 && ReferenceEquals(currentQueueEntry, queue.Peek()))
												queue.Dequeue();
										});
								break;
							}
						}
				}
			}
			finally {
				worker.P_IsLoopAlive = false;
			}
		}

		static async Task P_DoSingleMessagePostingAsync(LocalPostingToken token, ILocalSubscription subscription, CancellationToken ct) {
			token.EnsureNotNull(nameof(token));
			subscription.EnsureNotNull(nameof(subscription));
			//
			if (subscription.IsActive) {
				ILocalSubscriber subscriber;
				try {
					subscriber = subscription.Subscriber;
				}
				catch (ObjectDisposedException) {
					if (subscription.IsActive)
						throw;
					else
						return;
				}
				//
				using (var context = ContextUtilities.Create(ct: ct))
					await subscriber.ProcessMessagePostAsync(subscription: subscription, message: token.Message, ctx: context).ConfigureAwait(false);
				//
				token.NotifySubscriptionPosted();
			}
		}

		#endregion

		int _isLoopAlive;

		int _isLoopIdling;

		P_LoopSingleItemPostingState _currentPostingOperation;

		AutoResetEvent _breakLoopIdleEvent;

		CancellationTokenSource _cancellationTokenSource;

		Task _loopTask;

		RunControl<LocalPostingWorker> _runControl;

		Queue<LocalPostingQueueEntry> _queue;

		PrimitiveSpinLock _queueSpinLock;

		internal LocalPostingWorker(
			Queue<LocalPostingQueueEntry> queue,
			PrimitiveSpinLock queueSpinLock,
			CancellationToken cancellationToken) {
			//
			queue.EnsureNotNull(nameof(queue));
			queueSpinLock.EnsureNotNull(nameof(queueSpinLock));
			cancellationToken.EnsureNotEmpty(nameof(cancellationToken));
			//
			_queue = queue;
			_queueSpinLock = queueSpinLock;
			_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			_breakLoopIdleEvent = new AutoResetEvent(initialState: false);
			_isLoopIdling = 0;
			_isLoopAlive = 0;
			_runControl = new RunControl<LocalPostingWorker>(options: RunControlOptions.SingleStart, component: this, start: P_DoStartAsync, stop: P_DoStopAsync);
		}

		CancellationToken P_CancellationToken
			=> ReadDA(ref _cancellationTokenSource).Token;

		AutoResetEvent P_BreakLoopIdleEvent
			=> ReadDA(ref _breakLoopIdleEvent);

		Queue<LocalPostingQueueEntry> P_Queue
			=> ReadDA(ref _queue);

		PrimitiveSpinLock P_QueueSpinLock
			=> ReadDA(ref _queueSpinLock);

		public RunControl<LocalPostingWorker> RunControl
			=> ReadDA(ref _runControl);

		// TODO: Put strings into the resources.
		//
		Task P_DoStartAsync(IRunControlAttemptState state) {
			// TODO_HIGH: Этот таймаут должен браться из контекста запуска (когда будет реализован) как время оставшееся для выполнения операции запуска.
			//
			const int loopStartMillisecondsTimeout = 10000;
			try {
				state.EnsureNotNull(nameof(state));
				//
				var ct = ReadDA(ref _cancellationTokenSource).Token;
				ManualResetEventSlim loopStartedEvent = default;
				try {
					loopStartedEvent = new ManualResetEventSlim(initialState: false);
					Task loopTask;
					Interlocked
						.Exchange(
							location1: ref _loopTask,
							value:
								loopTask =
								TaskUtilities
								.RunOnDefaultScheduler(
									action: P_Loop,
									state: new P_LoopStartState(worker: this, loopStartedEvent: loopStartedEvent),
									cancellationToken: ct,
									options: TaskCreationOptions.PreferFairness | TaskCreationOptions.LongRunning));
					if (!loopStartedEvent.Wait(millisecondsTimeout: loopStartMillisecondsTimeout)) {
						loopStartedEvent.Dispose();
						//
						if (loopTask.IsCompleted)
							throw
								new EonException(
									message:
										"Цикл обработки очереди рабочих элементов не запустился в отведённое для запуска время. Время ожидания запуска истекло."
										+ (loopTask.IsCanceled ? $"{Environment.NewLine}Задача цикла была отменена." : string.Empty),
									innerException: loopTask.IsFaulted ? loopTask.Exception : null);
						else
							throw
								new TimeoutException(
									message: $"Цикл обработки очереди рабочих элементов не запустился в отведённое для запуска время. Время ожидания запуска истекло.{Environment.NewLine}Статус задачи цикла:{loopTask.Status.FmtStr().GNLI()}");
					}
					//
					loopTask
						.ContinueWith(
							continuationAction: P_OnLoopTaskCompleted,
							cancellationToken: CancellationToken.None,
							continuationOptions: TaskContinuationOptions.PreferFairness,
							scheduler: TaskScheduler.Default);
				}
				finally {
					loopStartedEvent?.Dispose();
				}
				//
				return TaskUtilities.FromVoidResult();
			}
			catch (Exception firstException) {
				return TaskUtilities.FromError(error: firstException);
			}
		}

		Task P_DoStopAsync() {
			try {
				var cancellationTokenSource = ReadDA(ref _cancellationTokenSource);
				if (!cancellationTokenSource.IsCancellationRequested)
					cancellationTokenSource.Cancel();
				var loopTask = ReadDA(ref _loopTask);
				if (loopTask == null)
					return TaskUtilities.FromVoidResult();
				else
					return loopTask;
			}
			catch (Exception firstException) {
				return TaskUtilities.FromError(error: firstException);
			}
		}

		void P_OnLoopTaskCompleted(Task loopTask) {
			loopTask.EnsureNotNull(nameof(loopTask));
			// TODO_HIGH: Реализовать запуск новой задачи цикла (если таковое указано конфигурацией воркера).
			//
			// ...
		}

		bool P_IsLoopAlive {
			get { return Interlocked.CompareExchange(ref _isLoopAlive, 0, 0) == 1; }
			set { Interlocked.Exchange(ref _isLoopAlive, value ? 1 : 0); }
		}

		public bool IsLoopAlive => P_IsLoopAlive;

		bool P_IsLoopIdling {
			get => Interlocked.CompareExchange(ref _isLoopIdling, 0, 0) == 1;
			set => Interlocked.Exchange(ref _isLoopIdling, value ? 1 : 0);
		}

		public bool IsLoopIdling => P_IsLoopIdling;

		void P_OnLoopPostingStarted(P_LoopSingleItemPostingState operation) {
			operation.EnsureNotNull(nameof(operation));
			//
			if (!(WriteDA(location: ref _currentPostingOperation, value: operation, comparand: null) is null))
				throw new InvalidOperationException();
		}

		void P_OnLoopPostingCompleted(P_LoopSingleItemPostingState operation) {
			operation.EnsureNotNull(nameof(operation));
			//
			Interlocked.CompareExchange(ref _currentPostingOperation, null, comparand: operation);
		}

		public TimeSpan? CurrentlyPostingDuration
			=> Interlocked.CompareExchange(location1: ref _currentPostingOperation, value: null, comparand: null)?.Duration;

		public ILocalSubscription CurrentlyPostingSubscription
			=> Interlocked.CompareExchange(location1: ref _currentPostingOperation, value: null, comparand: null)?.Subscription;

		public bool BreakLoopIdle() {
			AutoResetEvent breakLoopIdleEvent;
			if (P_IsLoopIdling
				&& TryReadDA(ref _breakLoopIdleEvent, out breakLoopIdleEvent)) {
				try { breakLoopIdleEvent.Set(); }
				catch (ObjectDisposedException) { return false; }
				return true;
			}
			else
				return false;
		}

		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose)
				TryReadDA(ref _runControl)?.StopAsync().WaitWithTimeout();
			//
			base.FireBeforeDispose(explicitDispose);
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_runControl?.Dispose();
				_breakLoopIdleEvent?.Dispose();
				_cancellationTokenSource?.Dispose();
			}
			_runControl = null;
			_breakLoopIdleEvent = null;
			_cancellationTokenSource = null;
			_loopTask = null;
			_queue = null;
			_queueSpinLock = null;
			_currentPostingOperation = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}