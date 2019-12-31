using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Eon.Linq;
using Eon.Threading.Tasks;

namespace Eon.Threading {

	using SignalAwaitable = AsyncTaskCompletionSource<bool>;

	public sealed class PrimitiveAsyncAutoResetEvent
		:Disposable {

		#region Static members

		static readonly SignalAwaitable __Signaled = new SignalAwaitable(result: true);

		#endregion

		Queue<SignalAwaitable> _queue;

		PrimitiveSpinLock _queueSpinLock;

		public PrimitiveAsyncAutoResetEvent() {
			_queueSpinLock = new PrimitiveSpinLock();
			_queue = new Queue<SignalAwaitable>();
		}

		public void Set() {
			var queue = ReadDA(ref _queue);
			var queueSpinLock = ReadDA(ref _queueSpinLock, considerDisposeRequest: true);
			queueSpinLock
				.Invoke(
					action:
						() => {
							EnsureNotDisposeState(considerDisposeRequest: true);
							//
							if (queue.Count == 0)
								queue.Enqueue(__Signaled);
							else if (queue.Count > 1 || !ReferenceEquals(queue.Peek(), __Signaled)) {
								var locSignaled = false;
								for (; queue.Count > 0;) {
									if (queue.Dequeue().TrySetResult(result: true)) {
										locSignaled = true;
										break;
									}
									Thread.Sleep(millisecondsTimeout: 1); // Выполнение любого готового к выполнению потока в системе (ОС).
								}
								if (!locSignaled)
									queue.Enqueue(__Signaled);
							}
						});
		}

		public Task<bool> WaitAsync(TimeoutDuration timeout)
			=> WaitAsync(ct: CancellationToken.None, timeout: timeout, treatCancellationAsTimeoutExpiry: false);

		public Task<bool> WaitAsync(CancellationToken ct)
			=> WaitAsync(ct: ct, timeout: TimeoutDuration.Infinite, treatCancellationAsTimeoutExpiry: false);

		public Task<bool> WaitAsync(CancellationToken ct, TimeoutDuration timeout)
			=> WaitAsync(ct: ct, timeout: timeout, treatCancellationAsTimeoutExpiry: false);

		public Task<bool> WaitAsync(CancellationToken ct, TimeoutDuration timeout, bool treatCancellationAsTimeoutExpiry) {
			SignalAwaitable signalAwaitable = null;
			try {
				timeout.EnsureNotNull(nameof(timeout));
				//
				if (ct.IsCancellationRequested)
					return treatCancellationAsTimeoutExpiry ? TaskUtilities.FromFalse() : TaskUtilities.FromCanceled<bool>(ct: ct);
				else {
					var timeoutCts = default(CancellationTokenSource);
					var timeoutCtRegistration = default(CancellationTokenRegistration?);
					try {
						signalAwaitable = new SignalAwaitable();
						//
						if (ct.CanBeCanceled) {
							var ctRegistration = default(CancellationTokenRegistration);
							try {
								signalAwaitable
									.Task
									.ContinueWith(
										continuationAction: (locTask) => { ctRegistration.Dispose(); },
										continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
								ctRegistration =
									ct
									.Register(
										callback:
											() => {
												if (treatCancellationAsTimeoutExpiry)
													signalAwaitable.TrySetResult(result: false);
												else
													signalAwaitable.TrySetCanceled(ct: ct);
											});
							}
							catch {
								ctRegistration.Dispose();
								throw;
							}
						}
						//
						if (!timeout.IsInfinite) {
							timeoutCts = new CancellationTokenSource(millisecondsDelay: timeout.Milliseconds);
							timeoutCtRegistration =
								timeoutCts
								.Token
								.Register(callback: () => signalAwaitable.TrySetResult(result: false));
						}
						//
						var queue = ReadDA(ref _queue);
						var queueSpinLock = ReadDA(ref _queueSpinLock, considerDisposeRequest: true);
						queueSpinLock
							.Invoke(
								action:
									() => {
										if (ct.IsCancellationRequested) {
											if (treatCancellationAsTimeoutExpiry)
												signalAwaitable.TrySetResult(result: false);
											else
												signalAwaitable.TrySetCanceled(ct: ct);
										}
										else {
											EnsureNotDisposeState(considerDisposeRequest: true);
											//
											if (queue.Count == 1 && ReferenceEquals(queue.Peek(), __Signaled)) {
												if (signalAwaitable.TrySetResult(result: true))
													queue.Dequeue();
											}
											else if (timeout.Milliseconds == 0)
												signalAwaitable.TrySetResult(result: false);
											else
												queue.Enqueue(signalAwaitable);
										}
									});
						return signalAwaitable.Task;
					}
					catch {
						timeoutCtRegistration?.Dispose();
						timeoutCts?.Dispose();
						throw;
					}
				}
			}
			catch (Exception exception) {
				if (signalAwaitable?.TrySetException(exception) == true)
					return signalAwaitable.Task;
				else
					return TaskUtilities.FromError<bool>(error: exception);
			}
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_queueSpinLock?.EnterAndExitLock();
				var queue = _queue;
				if (queue != null && queue.Count > 0) {
					var exception = DisposableUtilities.NewObjectDisposedException(disposable: this, disposeRequestedException: false);
					queue.ForEach(action: locItem => locItem.TrySetException(exception: exception));
					queue.Clear();
				}
			}
			_queue = null;
			_queueSpinLock = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}