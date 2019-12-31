using System;
using System.Collections.Generic;
using System.Threading;

using Eon.Threading;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Collections.Specialized {

	public sealed class RingRegistry<TKey, TValue>
		where TValue : class {

		#region Static & constant members

		/// <summary>
		/// <para>Value: '512'.</para>
		/// </summary>
		const int __DefaultCleanupThreshold = 512;

		#endregion

		readonly PrimitiveSpinLock<(TKey key, TValue value), (bool result, TValue value)> _registrySpinLock;

		readonly Dictionary<TKey, (long version, WeakReference<TValue> value)> _registry;

		long _versionCounter;

		public RingRegistry(int maxCapacity, IEqualityComparer<TKey> keyComparer = default, int cleanupThreshold = default) {
			maxCapacity.Arg(nameof(maxCapacity)).EnsureNotLessThanZero();
			cleanupThreshold.Arg(nameof(cleanupThreshold)).EnsureNotLessThanZero();
			//
			_registrySpinLock = maxCapacity > 0 ? new PrimitiveSpinLock<(TKey key, TValue value), (bool result, TValue value)>() : null;
			_registry = maxCapacity > 0 ? new Dictionary<TKey, (long version, WeakReference<TValue> value)>(comparer: keyComparer = keyComparer ?? EqualityComparer<TKey>.Default) : null;
			_versionCounter = long.MinValue;
			CleanupThreshold = cleanupThreshold == default ? __DefaultCleanupThreshold : cleanupThreshold;
			MaxCapacity = maxCapacity;
			KeyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
		}

		public int MaxCapacity { get; }

		public IEqualityComparer<TKey> KeyComparer { get; }

		public long Version
			=> itrlck.Get(location: ref _versionCounter);

		public int CleanupThreshold { get; }

		public bool Put(TKey key, TValue value) {
			if (key == null)
				throw new ArgumentNullException(paramName: nameof(key));
			//
			return MaxCapacity > 0 ? _registrySpinLock.Invoke(func: spinLockRegion, state: (key, value)).result : false;
			//
			(bool result, TValue value) spinLockRegion((TKey key, TValue value) locArgs) {
				var locNewVersion = itrlck.Increment(location: ref _versionCounter, maxInclusive: long.MaxValue, locationName: $"{nameof(RingRegistry<TKey, TValue>)}.{nameof(_versionCounter)}");
				//
				if (_registry.Count % CleanupThreshold == 0) {
					var locKeysToRemove = new List<TKey>();
					var locKeyComparer = KeyComparer;
					var locIsKeyArgRemoved = false;
					foreach (var locItem in _registry) {
						if (!locItem.Value.value.TryGetTarget(target: out _)) {
							locKeysToRemove.Add(item: locItem.Key);
							locIsKeyArgRemoved = locIsKeyArgRemoved || locKeyComparer.Equals(x: locArgs.key, y: locItem.Key);
						}
					}
					for (var i = 0; i < locKeysToRemove.Count; i++)
						_registry.Remove(key: locKeysToRemove[ i ]);
					if (locIsKeyArgRemoved) {
						_registry.Add(key: locArgs.key, value: (version: locNewVersion, value: new WeakReference<TValue>(target: locArgs.value, trackResurrection: false)));
						return (true, locArgs.value);
					}
				}
				//
				if (_registry.ContainsKey(key: locArgs.key))
					_registry[ locArgs.key ] = (version: locNewVersion, value: new WeakReference<TValue>(target: locArgs.value, trackResurrection: false));
				else if (_registry.Count < MaxCapacity)
					_registry.Add(key: locArgs.key, value: (version: locNewVersion, value: new WeakReference<TValue>(target: locArgs.value, trackResurrection: false)));
				else {
					var locVersion = long.MaxValue;
					var locKeyToRemove = default(TKey);
					foreach (var locItem in _registry) {
						if (!locItem.Value.value.TryGetTarget(target: out _)) {
							locKeyToRemove = locItem.Key;
							break;
						}
						else if (locItem.Value.version <= locVersion) {
							locVersion = locItem.Value.version;
							locKeyToRemove = locItem.Key;
						}
					}
					_registry.Remove(key: locKeyToRemove);
					_registry.Add(key: locArgs.key, value: (version: locNewVersion, value: new WeakReference<TValue>(target: locArgs.value, trackResurrection: false)));
				}
				return (true, locArgs.value);
			}
		}

		public bool Get(TKey key, out TValue value) {
			if (key == null)
				throw new ArgumentNullException(paramName: nameof(key));
			//
			if (MaxCapacity > 0) {
				var buffer = _registrySpinLock.Invoke(func: spinLockRegion, state: (key, default));
				value = buffer.value;
				return buffer.result;
			}
			else {
				value = default;
				return false;
			}
			//
			(bool result, TValue value) spinLockRegion((TKey key, TValue value) locArgs) {
				if (_registry.TryGetValue(key: locArgs.key, value: out var locValueWeakRefference)) {
					if (locValueWeakRefference.value.TryGetTarget(target: out var locValue))
						return (true, locValue);
					else
						_registry.Remove(key: locArgs.key);
				}
				return (false, default);
			}
		}

		public void Reset() {
			if (MaxCapacity > 0)
				_registrySpinLock.Invoke(func: spinLockRegion, state: default);
			//
			(bool result, TValue value) spinLockRegion((TKey key, TValue value) locArgs) {
				_registry.Clear();
				Interlocked.Exchange(location1: ref _versionCounter, value: long.MinValue);
				return default;
			}
		}

	}

}