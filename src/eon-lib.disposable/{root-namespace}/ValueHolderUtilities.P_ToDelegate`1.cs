using System;

using Eon.Collections;
using Eon.Reflection;

namespace Eon {

	public static partial class ValueHolderUtilities {

		#region Nested types

		// TODO_HIGH: От этого можно избавиться, если перенести работу с конкретным типом значения именно в ImmutableValueStore.
		// Изначально проблема в том, что ImmutableValueStore "работает" именно с типом, определяемым generic-параметром TValue, а должен с типом, который имеет переданное в конструктор ImmutableValueStore значение.
		//
		static class P_ToDelegate<TValue> {

			static readonly Type __TypeOfTValue;

			static readonly bool __IsTValueValueType;

			static readonly Type __VhConcreteType;

			static readonly Type __VhFactoriesKey;

			static P_ToDelegate() {
				__TypeOfTValue = typeof(TValue);
				__IsTValueValueType = __TypeOfTValue.IsValueType();
				__VhConcreteType = typeof(Vh<>).MakeGenericType(__TypeOfTValue);
				__VhFactoriesKey = typeof(Func<,,>).MakeGenericType(__TypeOfTValue, typeof(bool), __VhConcreteType);
			}

			public static IVh<TValue> ToValueHolder(TValue value, bool ownsValue)
				=> ToValueHolder(value: value, ownsValue: ownsValue, nopDispose: false);

			public static IVh<TValue> ToValueHolder(TValue value, bool ownsValue, bool nopDispose) {
				Type valueConcreteType, valueStoreConcreteType, cacheKey;
				if (__IsTValueValueType || value == null) {
					valueConcreteType = __TypeOfTValue;
					valueStoreConcreteType = __VhConcreteType;
					cacheKey = __VhFactoriesKey;
				}
				else {
					valueConcreteType = value.GetType();
					valueStoreConcreteType = typeof(Vh<>).MakeGenericType(valueConcreteType);
					cacheKey = typeof(Func<,,>).MakeGenericType(__TypeOfTValue, typeof(bool), valueStoreConcreteType);
				}
				var cachedDelegate =
					__VhFactories
					.GetOrAdd(
						spinLock: __VhFactoriesSpinLock,
						key: cacheKey,
						factory: (locKey) => ActivationUtilities.RequireConstructor<TValue, bool, bool, IVh<TValue>>(concreteType: valueStoreConcreteType, arg1ConcreteType: valueConcreteType));
				return cachedDelegate.Cast<Delegate, Func<TValue, bool, bool, IVh<TValue>>>()(arg1: value, arg2: ownsValue, arg3: nopDispose);
			}

		}

		#endregion

	}

}