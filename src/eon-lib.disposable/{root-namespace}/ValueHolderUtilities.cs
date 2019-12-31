using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

using Eon.Threading;

namespace Eon {

	/// <summary>
	/// Value holder utilities.
	/// </summary>
	public static partial class ValueHolderUtilities {

		internal static readonly ExceptionDispatchInfo HasExceptionSentinel = ExceptionDispatchInfo.Capture(source: new Exception());

		static readonly Dictionary<Type, Delegate> __VhFactories;

		static readonly PrimitiveSpinLock __VhFactoriesSpinLock;

		static ValueHolderUtilities() {
			__VhFactoriesSpinLock = new PrimitiveSpinLock();
			__VhFactories = new Dictionary<Type, Delegate>();
		}

		public static IVh<TValue> ToValueHolder<TValue>(this TValue value)
			=> ToValueHolder(value: value, ownsValue: false);

		public static IVh<TValue> ToValueHolder<TValue>(this TValue value, bool ownsValue)
			=> P_ToDelegate<TValue>.ToValueHolder(value: value, ownsValue: ownsValue);

		public static IVh<TValue> ToValueHolder<TValue>(this TValue value, bool ownsValue, bool nopDispose)
			=> P_ToDelegate<TValue>.ToValueHolder(value: value, ownsValue: ownsValue, nopDispose: nopDispose);

		public static IVh<TValue> ToValueHolder<TValue>(this TValue value, IEnumerable<IDisposable> disposables)
			=> new P_HolderWithDisposeRegistry<TValue>(value, disposables);

		public static IVh<TValue> ToValueHolder<TValue>(this TValue value, IDisposable disposable1, IDisposable disposable2, IDisposable disposable3)
			=> ToValueHolder(value, disposables: new[ ] { disposable1, disposable2, disposable3 });

		public static IVh<TValue> ToValueHolder<TValue>(this TValue value, IDisposable disposable1, IDisposable disposable2)
			=> ToValueHolder(value, disposable1, disposable2, disposable3: null);

		public static IVh<TValue> ToValueHolder<TValue>(this TValue value, IDisposable disposable)
			=> ToValueHolder(value, disposable, disposable2: null, disposable3: null);

		public static TValue GetOrDefault<TValue>(this IVh<TValue> vs)
			=> vs is null ? default : vs.ValueDisposeTolerant;

		public static IVh<T> MoveValue<T>(this IVh<T> source) {
			if (source is null)
				throw new ArgumentNullException(paramName: nameof(source));
			//
			IVh<T> result = default;
			try {
				result = source.Value.ToValueHolder(ownsValue: source.OwnsValue);
				source.RemoveValue();
				if (source.OwnsValue)
					source.EnsureNotDisposeState(considerDisposeRequest: true);
				return result;
			}
			catch (Exception exception) {
				result?.Dispose(exception);
				throw;
			}
		}

	}

}