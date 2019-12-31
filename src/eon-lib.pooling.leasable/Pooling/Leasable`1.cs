using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Threading;
using Eon.Threading.Tasks;

using Microsoft.Extensions.Logging;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Pooling {

	public sealed class Leasable<T>
		:Leasable
		where T : class {

		#region Nested types

		sealed class P_Acquisition {

			#region Static members

			static long __IdCounter = 0L;

			#endregion

			readonly XFullCorrelationId _id;

			Func<IContext, Task<IVh<T>>> _acquire;

			Func<IVh<T>, Task> _release;

			Task<IVh<T>> _acquireTask;

			DateTimeOffset _acquireDateTime;

			Task _releaseTask;

			readonly ILogger _logger;

			// TODO: Put strings into the resources.
			//
			internal P_Acquisition(Func<IContext, Task<IVh<T>>> acquire, Func<IVh<T>, Task> release, XFullCorrelationId lifecycleId, ILogger logger) {
				acquire.EnsureNotNull(nameof(acquire));
				release.EnsureNotNull(nameof(release));
				//
				_id = lifecycleId + (XCorrelationId)$"lease-{Interlocked.Increment(ref __IdCounter).ToString("d", CultureInfo.InvariantCulture)}";
				_acquire = acquire;
				_release = release;
				_logger = logger;
			}

			// TODO: Put strings into the resources.
			//
			public Task<IVh<T>> AcquireAsync(IContext context = default) {
				try {
					var existingTask = itrlck.Get(ref _acquireTask);
					var ct = context?.Ct() ?? CancellationToken.None;
					if (existingTask is null) {
						var newTaskProxy = default(TaskCompletionSource<IVh<T>>);
						try {
							newTaskProxy = new TaskCompletionSource<IVh<T>>(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);
							if (itrlck.UpdateIfNullBool(ref _acquireTask, newTaskProxy.Task, out existingTask)) {
								if (itrlck.Get(ref _releaseTask) is null)
									doAcquireAsync(locContext: context).ContinueWithTryApplyResultTo(taskProxy: newTaskProxy);
								else
									newTaskProxy.TrySetException(exception: new EonException(message: $"Invalid acquisition attempt.  Release operation has been attempted already.{Environment.NewLine}\tLease ID:{_id.FmtStr().GNLI2()}"));
								return existingTask;
							}
							else
								return existingTask.WaitResultAsync(ct: ct);
						}
						catch (Exception exception) {
							if (newTaskProxy?.TrySetException(exception) == true)
								return newTaskProxy.Task;
							else
								throw;
						}
						finally {
							if (!ReferenceEquals(newTaskProxy.Task, existingTask))
								newTaskProxy.TrySetCanceled();
						}
					}
					else
						return existingTask.WaitResultAsync(ct: ct);
				}
				catch (Exception exception) {
					return TaskUtilities.FromError<IVh<T>>(exception);
				}
				//
				async Task<IVh<T>> doAcquireAsync(IContext locContext) {
					var locAcquire = itrlck.Get(ref _acquire);
					var locRelease = itrlck.Get(ref _release);
					if (itrlck.Get(ref _releaseTask) is null) {
						try {
							IVh<T> locAcquired = default;
							try {
								locAcquired = await locAcquire(arg: locContext).ConfigureAwait(false);
								if (locAcquired is null)
									throw new EonException(message: $"Instance acquisition method has returned invalid value '{locAcquired.FmtStr().G()}'.{Environment.NewLine}\tLease ID:{_id.FmtStr().GNLI2()}");
								_acquireDateTime = DateTimeOffset.Now;
								_logger
									?.LogDebug(
											eventId: LeasableEventIds.LeaseStart,
											message: $"Lease started.{Environment.NewLine}\tLease ID:{Environment.NewLine}\t\t{{lease_id}}{Environment.NewLine}\tCorrelation ID:{Environment.NewLine}\t\t{{correlation_id}}",
											args: new object[ ] { _id.ToString(), locContext?.FullCorrelationId.ToString() });

								return locAcquired;
							}
							catch (Exception locException) {
								if (!(locAcquired is null)) {
									try {
										await locRelease(arg: locAcquired).ConfigureAwait(false);
									}
									catch (Exception locSecondException) {
										throw new AggregateException(locException, locSecondException);
									}
								}
								throw;
							}
						}
						catch (Exception locException) {
							try {
								_logger
									?.LogWarning(
											eventId: LeasableEventIds.LeaseStart,
											exception: locException,
											message: $"Lease start error.{Environment.NewLine}\tLease ID:{Environment.NewLine}\t\t{{lease_id}}{Environment.NewLine}\tCorrelation ID:{Environment.NewLine}\t\t{{correlation_id}}",
											args: new object[ ] { _id.ToString(), locContext?.FullCorrelationId.ToString() });
							}
							catch (Exception locSecondException) {
								throw new AggregateException(locException, locSecondException);
							}
							throw;
						}
					}
					else
						throw new EonException(message: $"Invalid lease start attempt. Lease end has been attempted already.{Environment.NewLine}\tLease ID:{_id.FmtStr().GNLI2()}");
				}
			}

			public Task ReleaseAsync() {
				try {
					var existingTask = itrlck.Get(ref _releaseTask);
					if (existingTask is null) {
						var newTaskProxy = default(TaskCompletionSource<Nil>);
						try {
							newTaskProxy = new TaskCompletionSource<Nil>(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);
							if (itrlck.UpdateIfNullBool(ref _releaseTask, newTaskProxy.Task, out existingTask))
								doReleaseAsync().ContinueWithTryApplyResultTo(taskProxy: newTaskProxy);
						}
						catch (Exception exception) {
							if (newTaskProxy?.TrySetException(exception) == true)
								return newTaskProxy.Task;
							else
								throw;
						}
						finally {
							if (!ReferenceEquals(newTaskProxy.Task, existingTask))
								newTaskProxy.TrySetCanceled();
						}
					}
					return existingTask;
				}
				catch (Exception exception) {
					return TaskUtilities.FromError(exception);
				}
				//
				async Task doReleaseAsync() {
					Func<IContext, Task<IVh<T>>> locAcquire = default;
					Task<IVh<T>> locAcquireTask = default;
					Func<IVh<T>, Task> locRelease = default;
					try {
						locAcquire = itrlck.Get(ref _acquire);
						locRelease = itrlck.Get(ref _release);
						locAcquireTask = itrlck.Get(ref _acquireTask);
						if (!(locAcquireTask is null)) {
							await locAcquireTask.WaitCompletionAsync().ConfigureAwait(false);
							if (locAcquireTask.IsSucceeded()) {
								var locAcquired = locAcquireTask.Result;
								var locNowDateTime = DateTimeOffset.Now;
								Exception locCaughtException = default;
								try {
									await locRelease(arg: locAcquired).ConfigureAwait(false);
								}
								catch (Exception locException) {
									locCaughtException = locException;
									throw;
								}
								finally {
									if (locCaughtException is null) {
										_logger
											?.LogDebug(
													eventId: LeasableEventIds.LeaseEnd,
													message: $"Lease end.{Environment.NewLine}\tLease ID:{Environment.NewLine}\t\t{{lease_id}}{Environment.NewLine}\tLease duration:{Environment.NewLine}\t\t{{lease_duration}}",
													args: new object[ ] { _id.ToString(), locNowDateTime.Subtract(_acquireDateTime).ToString("c") });
									}
									else
										try {
											_logger
												?.LogWarning(
														eventId: LeasableEventIds.LeaseEnd,
														exception: locCaughtException,
														message: $"Lease end error.{Environment.NewLine}\tLease ID:{Environment.NewLine}\t\t{{lease_id}}{Environment.NewLine}\tLease duration:{Environment.NewLine}\t\t{{lease_duration}}",
														args: new object[ ] { _id.ToString(), locNowDateTime.Subtract(_acquireDateTime).ToString("c") });
										}
										catch (Exception locException) {
											throw new AggregateException(locCaughtException, locException);
										}
								}
							}
						}
					}
					finally {
						itrlck.SetNullBool(ref _acquireTask, locAcquireTask);
						itrlck.SetNullBool(ref _acquire, locAcquire);
						itrlck.SetNullBool(ref _release, locRelease);
					}
				}
			}

		}

		sealed class P_Lifecycle
			:Disposable {

			#region Static members

			static long __IdCounter = 0L;

			public static Task DisposeAsync(P_Lifecycle lifecycle) {
				try {
					if (lifecycle is null)
						throw new ArgumentNullException(paramName: nameof(lifecycle));
					//
					if (lifecycle.Disposing || lifecycle.IsDisposed)
						return TaskUtilities.FromVoidResult();
					else
						return P_BeforeDisposeAsync(lifecycle: lifecycle, continuation: lifecycle.Dispose);
				}
				catch (Exception exception) {
					return TaskUtilities.FromError(error: exception);
				}
			}

			static Task P_BeforeDisposeAsync(P_Lifecycle lifecycle, Action continuation) {
				try {
					if (lifecycle is null)
						throw new ArgumentNullException(paramName: nameof(lifecycle));
					//
					if (lifecycle.Disposing || lifecycle.IsDisposed)
						return TaskUtilities.FromVoidResult();
					else
						return doDisposeAsync();
				}
				catch (Exception exception) {
					return TaskUtilities.FromError(error: exception);
				}
				//
				async Task doDisposeAsync() {
					lifecycle.SetDisposeRequested();
					var acquisition = lifecycle._acquisition;
					if (!(acquisition is null))
						await acquisition.ReleaseAsync().ConfigureAwait(false);
					continuation?.Invoke();
				}
			}

			#endregion

			readonly XFullCorrelationId _id;

			Func<IContext, Task<IVh<T>>> _acquire;

			Func<IVh<T>, Task> _release;

			P_Acquisition _acquisition;

			readonly ILogger _logger;

			internal P_Lifecycle(Func<IContext, Task<IVh<T>>> acquire, Func<IVh<T>, Task> release, XFullCorrelationId transientInstanceId, ILogger logger) {
				acquire.EnsureNotNull(nameof(acquire));
				release.EnsureNotNull(nameof(release));
				//
				_id = transientInstanceId + (XCorrelationId)$"life-cycle-{Interlocked.Increment(ref __IdCounter).ToString("d", CultureInfo.InvariantCulture)}";
				_acquire = acquire;
				_release = release;
				_logger = logger;
			}

			public async Task<IVh<T>> RequireInstanceAsync(IContext context = default) {
				var currentAcq = ReadDA(ref _acquisition, considerDisposeRequest: true);
				if (currentAcq is null) {
					P_Acquisition newAcq = default;
					try {
						newAcq = new P_Acquisition(acquire: ReadDA(ref _acquire), release: ReadDA(ref _release, considerDisposeRequest: true), lifecycleId: _id, logger: _logger);
						if (!UpdDAIfNullBool(location: ref _acquisition, value: newAcq, current: out currentAcq))
							await newAcq.ReleaseAsync().ConfigureAwait(false);
					}
					catch {
						itrlck.SetNull(location: ref _acquisition, comparand: newAcq);
						throw;
					}
				}
				return await currentAcq.AcquireAsync(context: context).ConfigureAwait(false);
			}

			protected override void FireBeforeDispose(bool explicitDispose) {
				if (explicitDispose)
					P_BeforeDisposeAsync(lifecycle: this, continuation: null).WaitWithTimeout();
				//
				base.FireBeforeDispose(explicitDispose);
			}

			protected override void Dispose(bool explicitDispose) {
				_acquisition = null;
				_acquire = null;
				_release = null;
				//
				base.Dispose(explicitDispose);
			}

		}

		#endregion

		#region Static members

		static readonly string __TypeGenericDefinitionName;

		static Leasable() {
			__TypeGenericDefinitionName = typeof(Leasable<>).Name;
		}

		static Task P_DefaultRelease(IVh<T> instance) {
			try {
				instance?.Dispose();
				return TaskUtilities.FromVoidResult();
			}
			catch (Exception exception) {
				return TaskUtilities.FromError(exception);
			}
		}

		static Func<IContext, Task<IVh<T>>> P_GetAcquireDelegate(Func<IVh<T>> acquire) {
			acquire.EnsureNotNull(nameof(acquire));
			//
			return (locContext) => TaskUtilities.FromResult(getter: acquire);
		}

		static Func<IContext, Task<IVh<T>>> P_GetAcquireDelegate(Func<IContext, Task<T>> acquire, bool ownsAcquired) {
			acquire.EnsureNotNull(nameof(acquire));
			//
			return
				async (locContext) => {
					var locAcquired = await acquire(arg: locContext).ConfigureAwait(false);
					return locAcquired.ToValueHolder(ownsValue: ownsAcquired);
				};
		}

		static Func<IVh<T>, Task> P_GetReleaseDelegate(Action<IVh<T>> release) {
			release.EnsureNotNull(nameof(release));
			//
			return
				(locInstance) => {
					try {
						release(locInstance);
						return TaskUtilities.FromVoidResult();
					}
					catch (Exception exception) {
						return TaskUtilities.FromError(error: exception);
					}
				};
		}

		#endregion

		Func<IContext, Task<IVh<T>>> _acquire;

		Func<IVh<T>, Task> _release;

		P_Lifecycle _lifecycle;

		TaskCompletionSource<Nil> _collectTask;

		public Leasable(Func<IVh<T>> acquire, TimeoutDuration preferredSlidingTtl, ArgumentPlaceholder<ILogger> logger = default)
			: this(acquire: P_GetAcquireDelegate(acquire: acquire), release: P_DefaultRelease, preferredSlidingTtl: preferredSlidingTtl, logger: logger) { }

		public Leasable(Func<IVh<T>> acquire, Action<IVh<T>> release, TimeoutDuration preferredSlidingTtl, ArgumentPlaceholder<ILogger> logger = default)
			: this(acquire: P_GetAcquireDelegate(acquire: acquire), release: P_GetReleaseDelegate(release: release), preferredSlidingTtl: preferredSlidingTtl, logger: logger) { }

		public Leasable(Func<IContext, Task<IVh<T>>> acquire, Func<IVh<T>, Task> release, TimeoutDuration preferredSlidingTtl, ArgumentPlaceholder<ILogger> logger = default)
			: base(preferredSlidingTtl: preferredSlidingTtl, logger: logger) {
			//
			acquire.EnsureNotNull(nameof(acquire));
			release.EnsureNotNull(nameof(release));
			//
			_acquire = acquire;
			_release = release;
		}

		public Leasable(Func<IContext, Task<IVh<T>>> acquire, TimeoutDuration preferredSlidingTtl, ArgumentPlaceholder<ILogger> logger = default)
			: this(acquire: acquire, release: P_DefaultRelease, preferredSlidingTtl: preferredSlidingTtl, logger: logger) { }

		public Leasable(Func<IContext, Task<T>> acquire, bool ownsAcquired, TimeoutDuration preferredSlidingTtl, ArgumentPlaceholder<ILogger> logger = default)
			: this(acquire: P_GetAcquireDelegate(acquire: acquire, ownsAcquired: ownsAcquired), release: P_DefaultRelease, preferredSlidingTtl: preferredSlidingTtl, logger: logger) { }

		// TODO: Put strings into the resources.
		//
		public async Task<Using<T>> UseAsync(IContext ctx = default) {
			var @using = await UseIfAsync(maxUseCountCondition: long.MaxValue, ctx: ctx).ConfigureAwait(false);
			if (!@using.IsInitialized)
				throw new EonException(message: $"Instance acquired.{Environment.NewLine}\tInstance:{this.FmtStr().GNLI2()}");
			else
				return @using;
		}

		public async Task<Using<T>> UseIfAsync(long maxUseCountCondition, IContext ctx = default) {
			Using<long> innerUsing = default;
			try {
				UseIf(maxUseCountCondition: maxUseCountCondition, @using: out innerUsing);
				if (!innerUsing.IsInitialized)
					return default;
				else {
					var collectTask = ReadDA(ref _collectTask, considerDisposeRequest: true);
					if (!(collectTask is null))
						await collectTask.WaitCompletionAsync(ct: ctx?.Ct() ?? CancellationToken.None).ConfigureAwait(false);
					//
					return new Using<T>(value: await P_RequireInstanceAsync(context: ctx).ConfigureAwait(false), dispose: (locValue, locExplicitDispose) => innerUsing.Dispose());
				}
			}
			catch (Exception exception) {
				try { innerUsing.Dispose(); }
				catch (Exception secondException) { throw new AggregateException(exception, secondException); }
				throw;
			}
		}

		async Task<T> P_RequireInstanceAsync(IContext context = default) {
			var current = ReadDA(ref _lifecycle);
			if (current is null) {
				context.ThrowIfCancellationRequested();
				var created = default(P_Lifecycle);
				try {
					created = new P_Lifecycle(transientInstanceId: Id, acquire: ReadDA(ref _acquire), release: ReadDA(ref _release, considerDisposeRequest: true), logger: Logger);
					if (!UpdDAIfNullBool(ref _lifecycle, created, out current))
						await P_Lifecycle.DisposeAsync(created).ConfigureAwait(false);
				}
				catch (Exception exception) {
					itrlck.SetNullBool(location: ref _lifecycle, comparand: created);
					if (!(created is null))
						try { await P_Lifecycle.DisposeAsync(created).ConfigureAwait(false); }
						catch (Exception secondException) { throw new AggregateException(exception, secondException); }
					throw;
				}
			}
			return (await current.RequireInstanceAsync(context: context).ConfigureAwait(false)).Value;
		}

		private protected sealed override void TryCollect() {
			var currentLifecycle = itrlck.Get(ref _lifecycle);
			if (!(currentLifecycle is null || CollectionTimepoint < 1L || IsDisposeRequested)) {
				var currentAwaitable = itrlck.Get(ref _collectTask);
				if (currentAwaitable is null) {
					var createdAwaitable = default(TaskCompletionSource<Nil>);
					try {
						createdAwaitable = new TaskCompletionSource<Nil>(creationOptions: TaskCreationOptions.None);
						if (itrlck.UpdateIfNullBool(ref _collectTask, createdAwaitable, out currentAwaitable)) {
							currentAwaitable.Task.ContinueWith(locTask => itrlck.SetNullBool(ref _collectTask, createdAwaitable), TaskContinuationOptions.ExecuteSynchronously);
							currentLifecycle = itrlck.Get(ref _lifecycle);
							if (currentLifecycle is null || CollectionTimepoint < 1L || IsDisposeRequested) {
								currentAwaitable.TrySetResult(null);
								currentAwaitable = null;
							}
							else
								TaskUtilities
									.RunOnDefaultScheduler(factory: async () => await doTryCollectAsync(currentLifecycle).ConfigureAwait(false))
									.ContinueWithTryApplyResultTo(taskProxy: currentAwaitable);
						}
					}
					catch {
						itrlck.SetNullBool(ref _collectTask, createdAwaitable);
						createdAwaitable?.TrySetCanceled();
						throw;
					}
					finally {
						if (!ReferenceEquals(createdAwaitable, currentAwaitable))
							createdAwaitable?.TrySetCanceled();
					}
					if (!(currentAwaitable is null))
						currentAwaitable.Task.WaitWithTimeout();
				}
				else
					currentAwaitable.Task.WaitWithTimeout();
			}
			//
			async Task doTryCollectAsync(P_Lifecycle locLifecycle) {
				if (locLifecycle is null)
					throw new ArgumentNullException(paramName: nameof(locLifecycle));
				//
				if (ReferenceEquals(locLifecycle, itrlck.Get(ref _lifecycle)) && CollectionTimepoint > 0L && !IsDisposeRequested) {
					var exchangeResult = Interlocked.CompareExchange(ref _lifecycle, null, locLifecycle);
					if (ReferenceEquals(exchangeResult, locLifecycle))
						await P_Lifecycle.DisposeAsync(locLifecycle).ConfigureAwait(false);
				}
			}
		}

		public override string ToString()
			=> $"{__TypeGenericDefinitionName}: {typeof(T)}, {Id?.Value}";

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_lifecycle?.Dispose();
			}
			_acquire = null;
			_release = null;
			_lifecycle = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}