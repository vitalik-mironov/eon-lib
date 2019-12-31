using System;
using System.Collections.Immutable;

using Eon.Threading;

namespace Eon.Collections {

	public static class DictionaryUtilitiesDisposable {

		[Obsolete(message: "Use '" + nameof(DictionaryUtilitiesDisposable) + "." + nameof(GetOrAddDA) + "' instead.", error: true)]
		public static TValue GetOrAdd<TKey, TValue>(this Disposable owner, ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TValue> factory, string locationName = default, Action<TKey, TValue> unclaimedValue = default)
			=> GetOrAddDA(owner: owner, location: ref location, key: key, factory: factory, locationName: locationName, unclaimedValue: unclaimedValue);

		public static TValue GetOrAddDA<TKey, TValue>(this Disposable owner, ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TValue> factory, string locationName = default, Action<TKey, TValue> unclaimedValue = default) {
			owner.EnsureNotNull(nameof(owner));
			key.EnsureNotNull(nameof(key));
			factory.EnsureNotNull(nameof(factory));
			//
			var factoriedValue = default(TValue);
			var valueFactoryCalled = false;
			var existingValue = default(TValue);
			var updateResult = default(UpdateResult<ImmutableDictionary<TKey, TValue>>);
			try {
				owner
					.UpdDA(
						location: ref location,
						transform:
							locCurrent => {
								if (locCurrent is null)
									throw ExceptionUtilities.NewNullReferenceException(varName: locationName ?? nameof(location), component: owner);
								else if (locCurrent.TryGetValue(key: key, value: out var locValue)) {
									existingValue = locValue;
									return locCurrent;
								}
								else {
									factoriedValue = valueFactoryCalled ? factoriedValue : factory(arg: key);
									valueFactoryCalled = true;
									return locCurrent.Add(key: key, value: factoriedValue);
								}
							},
						result: out updateResult);
				if (updateResult.IsUpdated)
					return factoriedValue;
				else
					return existingValue;
			}
			finally {
				if (updateResult.IsValid && !updateResult.IsUpdated && valueFactoryCalled)
					unclaimedValue?.Invoke(key, factoriedValue);
			}
		}

	}

}