using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<T[ ]> EnsureNoNullElements<T>(this ArgumentUtilitiesHandle<T[ ]> hnd) {
			if (!(hnd.Value is null)) {
				var array = hnd.Value;
				var lowerBound = array.GetLowerBound(0);
				var upperBound = array.GetUpperBound(0);
				for (var y = lowerBound; y <= upperBound; y++) {
					if (array[ y ] == null) {
						var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Array), "CanNotContainNull/NullAt", y.ToString("d"))}";
						throw
							hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
							?? new ArgumentException(paramName: hnd.Name, message: exceptionMessage);
					}
				}
			}
			return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<Array> EnsureOneDimensional(this ArgumentUtilitiesHandle<Array> hnd) {
			if (!(hnd.Value is null) && hnd.Value.Rank != 1) {
				var exceptionMessage =
					$"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}"
					+ $"{FormatXResource(locator: typeof(Array), subpath: "NotOneDimensional", args: new object[ ] { hnd.Value.Rank.ToString("d") })}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			return hnd;
		}

		public static void EnsureNotNullOrEmpty<T>(this T[ ] argument, string argumentName) {
			argument.EnsureNotNull(argumentName);
			if (argument.Length < 1)
				throw new ArgumentException(FormatXResource(typeof(Array), "CanNotEmpty"), string.IsNullOrEmpty(argumentName) ? "argument" : argumentName);
		}

		public static ArgumentUtilitiesHandle<T[ ]> EnsureHasLength<T>(this ArgumentUtilitiesHandle<T[ ]> hnd, int size) {
			if (size < 0)
				throw new ArgumentOutOfRangeException(nameof(size), FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanZero"));
			else if (hnd.Value == null || hnd.Value.Length == size)
				return hnd;
			else
				throw new ArgumentException(FormatXResource(typeof(Array), "ExpectedLength", size), hnd.Name);
		}

		public static ArgumentUtilitiesHandle<T[ ]> EnsureHasLength<T>(this T[ ] value, string name, int length) {
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length), FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanZero"));
			else if (value == null || value.Length == length)
				return new ArgumentUtilitiesHandle<T[ ]>(value, name);
			else
				throw new ArgumentException(FormatXResource(typeof(Array), "ExpectedLength", length.ToString("d")), string.IsNullOrEmpty(name) ? nameof(value) : name);
		}

		public static ArgumentUtilitiesHandle<T[ ]> EnsureHasMaxLength<T>(this ArgumentUtilitiesHandle<T[ ]> hnd, int maxLength) {
			if (maxLength < 0)
				throw new ArgumentOutOfRangeException(nameof(maxLength), FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanZero"));
			else if (hnd.Value == null || hnd.Value.Length <= maxLength)
				return hnd;
			else
				throw new ArgumentException(message: FormatXResource(typeof(Array), "TooBig/ExpectedMaxLength", maxLength.ToString("d")), paramName: hnd.Name);
		}

		public static ArgumentUtilitiesHandle<T[ ]> EnsureHasMinLength<T>(this ArgumentUtilitiesHandle<T[ ]> hnd, int minLength) {
			if (minLength < 0)
				throw new ArgumentOutOfRangeException(nameof(minLength), FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanZero"));
			else if (hnd.Value == null || hnd.Value.Length >= minLength)
				return hnd;
			else
				throw new ArgumentException(FormatXResource(typeof(Array), "TooSmall/ExpectedMinimalLength", minLength.ToString("d")), hnd.Name);
		}

		public static ArgumentUtilitiesHandle<T[ ]> EnsureHasMinLength<T>(this T[ ] value, string name, int minLength) {
			if (minLength < 0)
				throw new ArgumentOutOfRangeException(nameof(minLength), FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanZero"));
			else if (value == null || value.Length >= minLength)
				return new ArgumentUtilitiesHandle<T[ ]>(value, name);
			else
				throw new ArgumentException(FormatXResource(typeof(Array), "TooSmall/ExpectedMinimalLength", minLength.ToString("d")), string.IsNullOrEmpty(name) ? nameof(value) : name);
		}

		public static ArgumentUtilitiesHandle<TResult> As<TSource, TResult>(this ArgumentUtilitiesHandle<TSource[ ]> hnd)
			where TSource : class
			where TResult : class, TSource
			=>
			new ArgumentUtilitiesHandle<TResult>(value: hnd.Value as TResult, name: hnd.Name, isPropertyValue: hnd.IsProp, exceptionFactory: hnd.ExceptionFactory);

		public static ArgumentUtilitiesHandle<T[ ]> EnsureNotEmpty<T>(this ArgumentUtilitiesHandle<T[ ]> hnd) {
			if (hnd.Value is null || hnd.Value.Length > 0)
				return hnd;
			else {
				var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Array), "CanNotEmpty")}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentException(paramName: hnd.Name, message: exceptionMessage);
			}
		}

	}

}