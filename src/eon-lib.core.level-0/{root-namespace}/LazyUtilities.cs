using System;

namespace Eon {

	public static class LazyUtilities {

		public static void DisposeValue<T>(this Lazy<T> lazy)
			where T : IDisposable {
			if (lazy?.IsValueCreated == true)
				lazy.Value?.Dispose();
		}

		public static void DisposeValue<T>(this Lazy<T[ ]> lazy)
			where T : IDisposable {
			if (!(lazy is null || !lazy.IsValueCreated)) {
				var value = lazy.Value;
				if (!(value is null)) {
					var lowerBound = value.GetLowerBound(0);
					var upperBoundPlusOne = value.GetUpperBound(0) + 1;
					for (var i = lowerBound; i < upperBoundPlusOne; i++) {
						value[ i ]?.Dispose();
						value[ i ] = default;
					}
				}
			}
		}

	}

}