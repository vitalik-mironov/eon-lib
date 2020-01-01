#region Compilation conditional symbols

#if DEBUG

#define DO_NOT_SCHEDULE_COLLECTION
#undef DO_NOT_SCHEDULE_COLLECTION

#endif

#endregion
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Threading;

using Microsoft.Extensions.Logging;

using itrlck = Eon.Threading.InterlockedUtilities;
using vlt = Eon.Threading.VolatileUtilities;

namespace Eon.Pooling {

#pragma warning disable IDE0042 // Deconstruct variable declaration

	public abstract class Leasable
		:Disposable {

		#region Static & constant members

		/// <summary>
		/// Значение: '00:00:15.013'.
		/// </summary>
		public static readonly TimeoutDuration DefaultMaxCollectionInterval = TimeoutDuration.FromMilliseconds(milliseconds: 15013);

		/// <summary>
		/// Значение: '00:00:05.003'.
		/// </summary>
		public static readonly TimeoutDuration DefaultMinCollectionInterval = TimeoutDuration.FromMilliseconds(milliseconds: 5003);

		/// <summary>
		/// Значение: '00:00:05.003'.
		/// </summary>
		public static readonly TimeoutDuration MinTtl = TimeoutDuration.FromMilliseconds(5003);

		static long __IdCounter = 0L;

		static readonly string __ItemCollectionUnobservedErrorText = $"В ходе вызова обработчиков сборки неиспользуемых компонентов возникли ошибки.{Environment.NewLine}Источник вызова данного исключения:{($"{typeof(Leasable).FullName}.{nameof(P_DoRepoCollection)}").FmtStr().GNLI()}";

		static readonly List<WeakReference<Leasable>> __CollectionRepo;

		static readonly PrimitiveSpinLock __CollectionRepoSpinLock;

		static Leasable() {
			__CollectionRepo = new List<WeakReference<Leasable>>();
			__CollectionRepoSpinLock = new PrimitiveSpinLock();
		}

		static void P_RegisterForCollection(Leasable item) {
			if (item is null)
				throw new ArgumentNullException(paramName: nameof(item));
			//
			if (!item.PreferredSlidingTtl.IsInfinite) {
				var reference = new WeakReference<Leasable>(target: item, trackResurrection: false);
				var repoSize = __CollectionRepoSpinLock.Invoke(() => { __CollectionRepo.Add(reference); return __CollectionRepo.Count; });
				if (repoSize == 1)
					P_ScheduleNextRepoCollection(afterMillisecondsDelay: DefaultMaxCollectionInterval.Milliseconds);
			}
		}

		static int P_RepoItemComparison(WeakReference<Leasable> x, WeakReference<Leasable> y) {
			if (ReferenceEquals(x, y))
				return 0;
			else if (x is null)
				return 1;
			else if (y is null)
				return -1;
			else {
				var xCode = getComparisonCode(x);
				var yCode = getComparisonCode(y);
				return xCode < yCode ? -1 : (xCode == yCode ? 0 : 1);
			}
			//
			long getComparisonCode(WeakReference<Leasable> obj) {
				Leasable transient;
				if (obj.TryGetTarget(out transient) && !transient.IsDisposeRequested)
					return transient.CollectionTimepoint;
				else
					return long.MaxValue;
			}
		}

		static void P_DoRepoCollection() {
			var state =
				__CollectionRepoSpinLock
				.Invoke(
					() => {
						__CollectionRepo.Sort(P_RepoItemComparison);
						var locRepoSize = __CollectionRepo.Count;
						var locTimepoint = long.MaxValue - DateTime.UtcNow.Ticks;
						var locCandidates = new List<Leasable>();
						var locNextCollectionTimepoint = 1L;
						for (var locIndex = locRepoSize - 1; locIndex > -1; locIndex--) {
							var locReference = __CollectionRepo[ locIndex ];
							Leasable locTransient;
							if (locReference is null || !locReference.TryGetTarget(out locTransient) || locTransient.IsDisposeRequested)
								__CollectionRepo.RemoveAt(locIndex);
							else {
								var locCollectionTimepoint = locTransient.CollectionTimepoint;
								if (locCollectionTimepoint == 0L)
									__CollectionRepo.RemoveAt(locIndex);
								else if (locCollectionTimepoint > 1) {
									if (locCollectionTimepoint < locTimepoint) {
										locNextCollectionTimepoint = locCollectionTimepoint;
										break;
									}
									else
										locCandidates.Add(locTransient);
								}
								else
									break;
							}
						}
						return (Candidates: locCandidates, NextCollectionTimepoint: locNextCollectionTimepoint, RepoSize: __CollectionRepo.Count);
					});
			var nextCollectionTimepoint = state.NextCollectionTimepoint;
			if (state.Candidates.Count > 0) {
				var caughtExceptions = new List<Exception>();
				for (var index = 0; index < state.Candidates.Count; index++) {
					var transient = state.Candidates[ index ];
					long collectionTimepoint;
					transient.P_TryCollect(caughtExceptions: caughtExceptions, outCollectionTimepoint: out collectionTimepoint);
					if (collectionTimepoint > nextCollectionTimepoint)
						nextCollectionTimepoint = collectionTimepoint;
				}
				if (caughtExceptions.Count > 0)
					Task.FromException(exception: new EonException(message: __ItemCollectionUnobservedErrorText, innerException: new AggregateException(innerExceptions: caughtExceptions)));
			}
			if (!Environment.HasShutdownStarted && state.RepoSize > 0) {
				var nextCollectionDelayMilliseconds = DefaultMaxCollectionInterval.Milliseconds;
				if (nextCollectionTimepoint > 1) {
					var utcNow = DateTime.UtcNow;
					var nextCollectionDateTime = new DateTime(ticks: long.MaxValue - nextCollectionTimepoint, DateTimeKind.Utc);
					if (utcNow < nextCollectionDateTime)
						nextCollectionDelayMilliseconds =
							Math
							.Max(
								val1: (int)Math.Min(nextCollectionDateTime.Subtract(utcNow).TotalMilliseconds, DefaultMaxCollectionInterval.Milliseconds),
								val2: DefaultMinCollectionInterval.Milliseconds);
				}
				P_ScheduleNextRepoCollection(afterMillisecondsDelay: nextCollectionDelayMilliseconds);
			}
		}

		static void P_ScheduleNextRepoCollection(int afterMillisecondsDelay) {
#if !DO_NOT_SCHEDULE_COLLECTION
			Task
			.Delay(millisecondsDelay: afterMillisecondsDelay)
			.ContinueWith(
				continuationAction: locDelayTask => P_DoRepoCollection(),
				cancellationToken: CancellationToken.None,
				continuationOptions: TaskContinuationOptions.PreferFairness,
				scheduler: TaskScheduler.Default);
#endif
		}

		#endregion

		TimeoutDuration _preferredSlidingTtl;

		long _collectionTimepoint;

		protected readonly ILogger Logger;

		private protected Leasable(TimeoutDuration preferredSlidingTtl, ArgumentPlaceholder<ILogger> logger = default) {
			preferredSlidingTtl.EnsureNotNull(nameof(preferredSlidingTtl));
			//
			Id = (XCorrelationId)$"{nameof(Leasable).ToLowerInvariant()}-{Interlocked.Increment(ref __IdCounter).ToString("d", CultureInfo.InvariantCulture)}";
			_preferredSlidingTtl = preferredSlidingTtl.IsInfinite ? preferredSlidingTtl : (preferredSlidingTtl.Milliseconds < MinTtl.Milliseconds ? MinTtl : preferredSlidingTtl);
			_collectionTimepoint = 1L;
			Logger = logger.HasExplicitValue ? logger.ExplicitValue : default;
			//
			if (!preferredSlidingTtl.IsInfinite)
				P_RegisterForCollection(item: this);
		}

		public XFullCorrelationId Id { get; private set; }

		public TimeoutDuration PreferredSlidingTtl
			=> ReadDA(ref _preferredSlidingTtl);

		/// <summary>
		/// Спец. временной маркер, определяющий момент времени, когда будет вызван обработчик сборки неиспользуемого компонента — <see cref="TryCollect"/>.
		/// <para>Чем меньше значение, тем позднее будет вызван обработчик сборки неиспользуемого компонента.</para>
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки.</para>
		/// <para>Значения:</para>
		/// <para>• [<see cref="long.MinValue"/>; <see langword="-1"/>]. Значение в этом диапазоне означает наличие как минимум одного захвата использования компонента (см. <see cref="UseIf(long, out Using{long})"/>).</para>
		/// <para>• <see langword="0"/> — устаналивается при поступлении запроса выгрузки (см. <see cref="IEonDisposable.IsDisposeRequested"/>).</para>
		/// <para>• [<see langword="1"/>; <see cref="long.MaxValue"/>]. Значение в этом диапазоне указывает момент времени, после наступления которого должен быть вызван обработчик сборки <see cref="TryCollect"/>. Чем больше значение в этом диапазоне, тем на более ранний момент оно указывает.</para>
		/// </summary>
		public long CollectionTimepoint
			=> vlt.Read(ref _collectionTimepoint);

		// TODO: Put strings into the resources.
		//
		private protected void UseIf(long maxUseCountCondition, out Using<long> @using) {
			maxUseCountCondition.Arg(nameof(maxUseCountCondition)).EnsureNotLessThanZero();
			//
			if (maxUseCountCondition == 0L) {
				EnsureNotDisposeState(considerDisposeRequest: true);
				@using = default;
			}
			else {
				long currentCollectionTimepoint;
				var maxUseCountAsNegative = -maxUseCountCondition;
				var isCollectionTimepointUpdated = false;
				var preferredTtl = ReadDA(ref _preferredSlidingTtl, considerDisposeRequest: true);
				try {
					itrlck.Update(ref _collectionTimepoint, transform: acquireUse, out currentCollectionTimepoint, out isCollectionTimepointUpdated);
					EnsureNotDisposeState(considerDisposeRequest: true);
					if (isCollectionTimepointUpdated)
						@using = new Using<long>(value: currentCollectionTimepoint, dispose: endUse);
					else
						@using = default;
				}
				catch {
					if (isCollectionTimepointUpdated)
						itrlck.Update(ref _collectionTimepoint, transform: releaseUse, out currentCollectionTimepoint, out isCollectionTimepointUpdated);
					throw;
				}
				//
				void endUse(long locValue, bool locExplicitDispose)
					=>
					itrlck
					.Update(location: ref _collectionTimepoint, transform: releaseUse, current: out var locCurrent, isUpdated: out var locIsUpdated);
				long acquireUse(long locValue) {
					if (locValue == 0L)
						// Поступил запрос выгрузки.
						//
						throw DisposableUtilities.NewObjectDisposedException(disposable: this, disposeRequestedException: true);
					else if (locValue < 0L) {
						// Есть уже как минимум один захват использования компонента.
						//
						if (locValue > maxUseCountAsNegative)
							checked { return locValue - 1L; }
						else
							return locValue;
					}
					else
						// Нет ни одного захвата использования компонента.
						// Тек. запрос — первая попытка захвата.
						//
						return -1L;
				}
				long releaseUse(long locValue) {
					if (locValue == 0L)
						// Поступил запрос выгрузки.
						//
						return locValue;
					else if (locValue == -1L) {
						// Освобождение последнего захвата использования компонента.
						//
						if (preferredTtl.IsInfinite)
							// Поскольку в качестве предпочтительного ttl указан бесконечный таймаут, то устанавливается спец. значение "1". Это значение запрещает производить сборку (уничтожение) компонента (см. P_DoRepoCollection).
							//
							return 1L;
						else
							// Устанавливается спец. значение, соответствующее моменту времени, после наступления которого при отсутствии захватов использования, компонент будет уничтожен.
							//
							checked { return long.MaxValue - DateTime.UtcNow.AddMilliseconds(preferredTtl.Milliseconds).Ticks; }
					}
					else if (locValue < -1L)
						// По меньшей мере, кроме особождаемого захвата использования, есть еще один.
						//
						checked { return locValue + 1L; }
					else
						// Неожидаемое состояние.
						//
						throw new EonException(message: $"Unexpected state of '{nameof(locValue)} ({nameof(_collectionTimepoint)})'.");
				}
			}
		}

		void P_TryCollect(List<Exception> caughtExceptions, out long outCollectionTimepoint) {
			if (caughtExceptions is null)
				throw new ArgumentNullException(paramName: nameof(caughtExceptions));
			//
			var currentTimepoint = CollectionTimepoint;
			if (currentTimepoint < 2L || IsDisposeRequested)
				outCollectionTimepoint = IsDisposeRequested ? 0L : currentTimepoint;
			else {
				try { TryCollect(); }
				catch (Exception exception) { caughtExceptions.Add(exception); }
				var exchangeResult = Interlocked.CompareExchange(ref _collectionTimepoint, 1L, currentTimepoint);
				if (exchangeResult == currentTimepoint)
					outCollectionTimepoint = 1L;
				else
					outCollectionTimepoint = exchangeResult;
			}
		}

		private protected abstract void TryCollect();

		protected sealed override void FireBeforeDispose(bool explicitDispose) {
			Interlocked.Exchange(ref _collectionTimepoint, 0L);
			//
			base.FireBeforeDispose(explicitDispose);
		}

		protected override void Dispose(bool explicitDispose) {
			_preferredSlidingTtl = null;
			//
			base.Dispose(explicitDispose);
		}

	}

#pragma warning restore IDE0042 // Deconstruct variable declaration

}