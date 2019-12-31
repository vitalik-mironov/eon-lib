using System;
using System.Runtime.CompilerServices;
using System.Threading;

using Eon.Threading;

using itrlck = Eon.Threading.InterlockedUtilities;
using sys_itrlck = System.Threading.Interlocked;

namespace Eon.Runtime {

	public sealed class GarbageCollectionTracking<TInstance>
		where TInstance : class {

		#region Nested types

		sealed class P_DestroyHnd {

			Action _notifyDelegate;

			internal P_DestroyHnd() { }

			public void SetNotifyDelegate(Action action) {
				if (action is null)
					throw new ArgumentNullException(paramName: nameof(action));
				//
				if (!(sys_itrlck.CompareExchange(location1: ref _notifyDelegate, value: action, comparand: null) is null))
					throw new EonException();
			}

			~P_DestroyHnd() {
				sys_itrlck.Exchange(ref _notifyDelegate, null)?.Invoke();
			}

		}

		#endregion

		readonly ConditionalWeakTable<TInstance, P_DestroyHnd> _referencesRepo;

		readonly PrimitiveSpinLock _referencesRepoSpinLock;

		long _registeredCounter;

		EventHandler _event_AllRegisteredCollected;

		public GarbageCollectionTracking() {
			_referencesRepo = new ConditionalWeakTable<TInstance, P_DestroyHnd>();
			_referencesRepoSpinLock = new PrimitiveSpinLock();
			_registeredCounter = 0L;
		}

		public void Register(TInstance instance) {
			instance.EnsureNotNull(nameof(instance));
			//
			_referencesRepoSpinLock
				.Invoke(
					() => {
						if (!_referencesRepo.TryGetValue(instance, out var destroyHandle)) {
							destroyHandle = new P_DestroyHnd();
							var noError = false;
							try {
								_referencesRepo.Add(instance, destroyHandle);
								noError = true;
							}
							finally {
								if (noError) {
									destroyHandle.SetNotifyDelegate(action: P_DestroyHandleNotify);
									sys_itrlck.Increment(location: ref _registeredCounter);
								}
							}
						}
					});
		}

		void P_DestroyHandleNotify() {
			var decrementedCounter = sys_itrlck.Decrement(location: ref _registeredCounter);
			if (decrementedCounter == 0) {
				var eventDelegate = itrlck.Get(ref _event_AllRegisteredCollected);
				if (!(eventDelegate is null || Environment.HasShutdownStarted))
					ThreadPool
						.QueueUserWorkItem(
							callBack:
								locState => {
									var locArgs = (Tuple<GarbageCollectionTracking<TInstance>, EventHandler>)locState;
									if (!Environment.HasShutdownStarted)
										locArgs.Item2(sender: locArgs.Item1, e: EventArgs.Empty);
								},
							state: new Tuple<GarbageCollectionTracking<TInstance>, EventHandler>(this, eventDelegate));
			}
		}

		public event EventHandler AllRegisteredCollected {
			add {
				EventHandler original;
				for (; ; ) {
					original = itrlck.Get(ref _event_AllRegisteredCollected);
					if (ReferenceEquals(original, sys_itrlck.CompareExchange(location1: ref _event_AllRegisteredCollected, value: original + value, comparand: original)))
						break;
				}
			}
			remove {
				EventHandler original;
				for (; ; ) {
					original = itrlck.Get(ref _event_AllRegisteredCollected);
					if (ReferenceEquals(original, sys_itrlck.CompareExchange(ref _event_AllRegisteredCollected, original - value, original)))
						break;
				}
			}
		}

		public long RegisteredCount
			=> sys_itrlck.Add(location1: ref _registeredCounter, value: 0);

	}

}