using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Eon.Collections;
using Eon.Context;
using Eon.Diagnostics.Logging;
using Eon.Linq;
using Eon.Threading;
using Eon.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Eon.Pooling {

	public class Pool<T>
		:Disposable, IPool<T>
		where T : class {

		#region Static & constant members

		static Func<IContext, Task<T>> P_MakeItemFactoryDelegate(Func<T> itemFactory) {
			itemFactory.EnsureNotNull(nameof(itemFactory));
			//
			return func;
			//
			async Task<T> func(IContext locCtx) {
				locCtx.ThrowIfCancellationRequested();
				//
				await Task.CompletedTask;
				//
				return itemFactory();
			}
		}

		static Func<IContext, Task<T>> P_MakeItemFactoryDelegate(Func<Task<T>> itemFactory) {
			itemFactory.EnsureNotNull(nameof(itemFactory));
			//
			return func;
			//
			async Task<T> func(IContext locCtx) {
				locCtx.ThrowIfCancellationRequested();
				//
				return await itemFactory().ConfigureAwait(false);
			}
		}

		#endregion

		Func<IContext, Task<T>> _itemFactory;

		readonly bool _ownsFactoriedItem;

		ILogger _logger;

		ImmutableStack<ImmutableStackEntry<Leasable<T>>> _poolItems;

		public Pool(
			Func<Task<T>> itemFactory,
			ArgumentPlaceholder<bool> ownsItem = default,
			ArgumentPlaceholder<TimeoutDuration> itemPreferredSlidingTtl = default,
			ArgumentPlaceholder<int> maxSize = default,
			string displayName = default,
			ArgumentPlaceholder<ILogger> logger = default)
			: this(
					itemFactory: P_MakeItemFactoryDelegate(itemFactory: itemFactory),
					ownsItem: ownsItem.Substitute(value: Pool.DefaultOfOwnsItem),
					itemPreferredSlidingTtl: itemPreferredSlidingTtl.Substitute(value: Pool.DefaultOfItemPreferredSlidingTtl),
					maxSize: maxSize.Substitute(value: Pool.DefaultOfMaxSize),
					displayName: displayName,
					logger: logger) { }

		public Pool(
			Func<IContext, Task<T>> itemFactory,
			ArgumentPlaceholder<bool> ownsItem = default,
			ArgumentPlaceholder<TimeoutDuration> itemPreferredSlidingTtl = default,
			ArgumentPlaceholder<int> maxSize = default,
			string displayName = default,
			ArgumentPlaceholder<ILogger> logger = default)
			: this(
					itemFactory: itemFactory, 
					ownsItem: ownsItem.Substitute(value: Pool.DefaultOfOwnsItem), 
					itemPreferredSlidingTtl: itemPreferredSlidingTtl.Substitute(value: Pool.DefaultOfItemPreferredSlidingTtl), 
					maxSize: maxSize.Substitute(value: Pool.DefaultOfMaxSize), 
					displayName: displayName, 
					logger: logger) { }

		public Pool(
			Func<T> itemFactory,
			ArgumentPlaceholder<bool> ownsItem = default,
			ArgumentPlaceholder<TimeoutDuration> itemPreferredSlidingTtl = default,
			ArgumentPlaceholder<int> maxSize = default,
			string displayName = default,
			ArgumentPlaceholder<ILogger> logger = default)
			: this(
					itemFactory: P_MakeItemFactoryDelegate(itemFactory: itemFactory), 
					ownsItem: ownsItem.Substitute(value: Pool.DefaultOfOwnsItem), 
					itemPreferredSlidingTtl: itemPreferredSlidingTtl.Substitute(value: Pool.DefaultOfItemPreferredSlidingTtl),
					maxSize: maxSize.Substitute(value: Pool.DefaultOfMaxSize), 
					displayName: displayName, 
					logger: logger) { }

		public Pool(
			Func<IContext, Task<T>> itemFactory, 
			bool ownsItem, 
			TimeoutDuration itemPreferredSlidingTtl, 
			int maxSize, 
			string displayName = default, 
			ArgumentPlaceholder<ILogger> logger = default) {
			//
			itemFactory.EnsureNotNull(nameof(itemFactory));
			itemPreferredSlidingTtl.EnsureNotNull(nameof(itemPreferredSlidingTtl));
			maxSize.Arg(nameof(maxSize)).EnsureNotLessThan(operand: 1);
			displayName.Arg(nameof(displayName)).EnsureNotEmpty().EnsureHasMaxLength(maxLength: Pool.DisplayNameMaxLength);
			//
			_itemFactory = itemFactory;
			_ownsFactoriedItem = ownsItem;
			ItemPreferredSlidingTtl = itemPreferredSlidingTtl;
			MaxSize = maxSize;
			P_MaxSizeMinusOne = maxSize - 1;
			DisplayName = displayName;
			_poolItems = ImmutableStack<ImmutableStackEntry<Leasable<T>>>.Empty;
			_logger = logger.Substitute(value: default);
			//
			Pool.Register(pool: this);
		}

		public string DisplayName { get; }

		public TimeoutDuration ItemPreferredSlidingTtl { get; }

		public int MaxSize { get; }

		int P_MaxSizeMinusOne { get; }

		Leasable<T> P_NewPoolItem(IContext ctx = default)
			=> new Leasable<T>(ownsAcquired: _ownsFactoriedItem, preferredSlidingTtl: ItemPreferredSlidingTtl, acquire: ReadDA(ref _itemFactory, considerDisposeRequest: true), logger: ReadDA(location: ref _logger, considerDisposeRequest: true).ArgPlaceholder());

		async Task<IVh<T>> P_NewItemAsync(IContext ctx = default) {
			var factory = ReadDA(location: ref _itemFactory, considerDisposeRequest: true);
			var newItem = await factory(arg: ctx).ConfigureAwait(false);
			return newItem.ToValueHolder(ownsValue: _ownsFactoriedItem);
		}

		public virtual async Task<IUsing<T>> TakeAsync(IContext ctx = default) {
			var logger = ReadDA(location: ref _logger, considerDisposeRequest: true);
			var correlationId = ctx?.FullCorrelationId;
			var thisPoolString = ToString();
			var newPoolItem = default(Leasable<T>);
			var notDisposeNewItemLeasableOnFail = false;
			try {
				for (; ; ) {
					ctx.ThrowIfCancellationRequested();
					//
					var poolItems = ReadDA(location: ref _poolItems, considerDisposeRequest: true);
					// Обход пула в попытке найти "свободный" компонент.
					//
					foreach (var poolItem in poolItems) {
						// TODO_HIGH: Будет оптимальнее, если "свободный" компонент получать не обходом всех элементов пула, а из к.л. переменной класса, хранящей один или более "свободных" компонентов.
						//
						var leaseResult = await poolItem.Value.UseIfAsync(maxUseCountCondition: 1, ctx: ctx).ConfigureAwait(false);
						if (leaseResult.IsInitialized) {
							try {
								logger
									?.LogDebug(
										eventId: PoolEventIds.TakeExisting,
										message: $"Component taken from pool.{Environment.NewLine}\tComponent:{Environment.NewLine}\t\t{{component}}{Environment.NewLine}\tPool:{Environment.NewLine}\t\t{{pool}}{Environment.NewLine}\tPool item index:{Environment.NewLine}\t\t{{pool_item_index}}{Environment.NewLine}\tPool item:{Environment.NewLine}\t\t{{pool_item}}{Environment.NewLine}\tPool size limit:{Environment.NewLine}\t\t{{pool_size_limit}}{Environment.NewLine}\tCorrelation ID:{Environment.NewLine}\t\t{{correlation_id}}",
										args: new object[ ] { leaseResult.Value?.ToString(), thisPoolString, poolItem.Position.ToString(), poolItem.Value.ToString(), MaxSize.ToString(), correlationId?.Value });
								return new UsingClass<T>(@using: ref leaseResult);
							}
							catch {
								leaseResult.Dispose();
								throw;
							}
						}
					}
					// Создание компонента и помещение его в пул (если позволяют условия).
					//
					int currentSizeMinusOne;
					if (poolItems.IsEmpty)
						currentSizeMinusOne = -1;
					else
						currentSizeMinusOne = poolItems.Peek().Position;
					if (currentSizeMinusOne < P_MaxSizeMinusOne) {
						// В пуле есть "свободные места".
						//
						newPoolItem = newPoolItem ?? P_NewPoolItem(ctx: ctx);
						WriteDA(
							location: ref _poolItems,
							value: poolItems.Push(value: new ImmutableStackEntry<Leasable<T>>(value: newPoolItem, position: currentSizeMinusOne + 1)),
							comparand: poolItems,
							result: out notDisposeNewItemLeasableOnFail);
						if (notDisposeNewItemLeasableOnFail) {
							// Созданный компонент помещён в пул.
							// Попытка взять этот компонент в использование.
							//
							var leaseResult = await newPoolItem.UseIfAsync(maxUseCountCondition: 1, ctx: ctx).ConfigureAwait(false);
							if (!leaseResult.IsInitialized) {
								// Только что созданный компонент из пула уже "кто-то" взял в использование.
								//
								newPoolItem = null;
								notDisposeNewItemLeasableOnFail = false;
							}
							else {
								try {
									logger
										?.LogDebug(
											eventId: PoolEventIds.TakeNew,
											message: $"New component created, placed in pool and taken from it.{Environment.NewLine}\tComponent:{Environment.NewLine}\t\t{{component}}{Environment.NewLine}\tPool:{Environment.NewLine}\t\t{{pool}}{Environment.NewLine}\tPool item index:{Environment.NewLine}\t\t{{pool_item_index}}{Environment.NewLine}\tPool item:{Environment.NewLine}\t\t{{pool_item}}{Environment.NewLine}\tPool size limit:{Environment.NewLine}\t\t{{pool_size_limit}}{Environment.NewLine}\tCorrelation ID:{Environment.NewLine}\t\t{{correlation_id}}",
											args: new object[ ] { leaseResult.Value?.ToString(), thisPoolString, (currentSizeMinusOne + 1).ToString(), newPoolItem.ToString(), MaxSize.ToString(), correlationId?.Value });
									return new UsingClass<T>(@using: ref leaseResult);
								}
								catch {
									leaseResult.Dispose();
									throw;
								}
							}
						}
					}
					else {
						// Пул полностью заполнен. Создать компонент, не помещать его в пул.
						// TODO_HIGH: Возможность ожидания освобождения компонента из пула вместо создания нового.
						// TODO_HIGH: Когда создается новый экземпляр компонента, то при завершении работы с ним (Using`1.Dispose()) класть его в пул.
						//
						IVh<T> newItem = default;
						try {
							newItem = await P_NewItemAsync(ctx: ctx).ConfigureAwait(false);
							logger
								?.LogDebug(
									eventId: PoolEventIds.TakeExhausted,
									message: $"Pool exhausted, no free components. Component taken as new, bypassing the pool.{Environment.NewLine}\tComponent:{Environment.NewLine}\t\t{{component}}{Environment.NewLine}\tPool:{Environment.NewLine}\t\t{{pool}}{Environment.NewLine}\tPool size limit:{Environment.NewLine}\t\t{{pool_size_limit}}{Environment.NewLine}\tCorrelation ID:{Environment.NewLine}\t\t{{correlation_id}}",
									args: new object[ ] { newItem.Value?.ToString(), thisPoolString, MaxSize.ToString(), correlationId?.Value });
							return
								new UsingClass<T>(
									value: newItem.Value,
									dispose:
										(locValue, locExplicitDispose) => {
											if (locExplicitDispose)
												newItem.Dispose();
										});
						}
						catch (Exception exception) {
							newItem?.Dispose(exception: exception);
							throw;
						}
					}
				}
			}
			catch (Exception exception) {
				if (!notDisposeNewItemLeasableOnFail)
					newPoolItem?.Dispose(exception: exception);
				throw;
			}
		}

		ITaskWrap<IUsing<T>> IPool<T>.TakeAsync(IContext ctx) {
			try { return TakeAsync(ctx: ctx).Wrap(); }
			catch (Exception exception) { return TaskUtilities.Wrap<IUsing<T>>(exception: exception); }
		}

		public virtual (int size, int free)? GetBaseStats(bool disposeTolerant = default) {
			var items = disposeTolerant ? TryReadDA(location: ref _poolItems) : ReadDA(location: ref _poolItems);
			if (items is null) {
				if (!disposeTolerant)
					EnsureNotDisposeState();
				return default;
			}
			else {
				var size = 0;
				var free = 0;
				foreach (var item in items) {
					size++;
					if (item.Value.CollectionTimepoint > 0L)
						free++;
				}
				return (size, free);
			}
		}

		public override string ToString() {
			var displayName = DisplayName;
			if (string.IsNullOrEmpty(displayName))
				return $"{GetType().Name}, hc0x{RuntimeHelpers.GetHashCode(o: this).ToString("x8")}";
			else
				return $"{displayName}, hc0x{RuntimeHelpers.GetHashCode(o: this).ToString("x8")}";
		}

		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose)
				_poolItems?.Observe(locItem => locItem.Value?.SetDisposeRequested());
			//
			base.FireBeforeDispose(explicitDispose);
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				var logger = _logger;
				if (!(logger is null)) {
					var componentString = ToString();
					logger
						.LogDebug(
							eventId: GenericEventIds.ExplicitDispose,
							message: $"Explicit dispose call.{Environment.NewLine}\tComponent:{Environment.NewLine}\t\t{{component}}",
							args: new object[ ] { componentString });
				}
				_poolItems?.Observe(locItem => locItem.Value?.Dispose());
			}
			_itemFactory = null;
			_poolItems = null;
			_logger = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}