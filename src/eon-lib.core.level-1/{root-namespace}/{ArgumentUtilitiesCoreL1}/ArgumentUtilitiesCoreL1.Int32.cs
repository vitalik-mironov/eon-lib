using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		public static int EnsureGreaterThanOrEqual(this int value, int operand, string name) {
			if (value < operand)
				throw new ArgumentOutOfRangeException(string.IsNullOrEmpty(name) ? nameof(value) : name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThan", operand.ToString("d")));
			return value;
		}

		public static int EnsureNotLessThanOne(this int value, string argumentName) {
			if (value < 1)
				throw new ArgumentOutOfRangeException(string.IsNullOrEmpty(argumentName) ? "argumentValue" : argumentName, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanOne"));
			return value;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<int?> EnsureNotLessThanZero(this ArgumentUtilitiesHandle<int?> hnd) {
			if (hnd.Value < 0) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(locator: typeof(ArgumentOutOfRangeException), subpath: "CanNotLessThanZero")}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<int> EnsureNotLessThanZero(this ArgumentUtilitiesHandle<int> hnd) {
			if (hnd.Value < 0) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(locator: typeof(ArgumentOutOfRangeException), subpath: "CanNotLessThanZero")}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<int> EnsureNotLessThan(this ArgumentUtilitiesHandle<int> hnd, int operand) {
			if (hnd.Value < operand) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThan", operand.ToString("d"))}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<int> EnsureNotLessThan(this int value, string name, int operand) {
			if (value < operand)
				throw new ArgumentOutOfRangeException(string.IsNullOrEmpty(name) ? nameof(value) : name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThan", operand.ToString("d")));
			return new ArgumentUtilitiesHandle<int>(value, name);
		}

		public static ArgumentUtilitiesHandle<int> EnsureNotLessThanZero(this int value, string name) {
			if (value < 0)
				throw new ArgumentOutOfRangeException(string.IsNullOrEmpty(name) ? nameof(value) : name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanZero"));
			return new ArgumentUtilitiesHandle<int>(value, string.IsNullOrEmpty(name) ? nameof(value) : name);
		}


		public static ArgumentUtilitiesHandle<int?> EnsureNotLessThan(this ArgumentUtilitiesHandle<int?> hnd, int operand) {
			if (hnd.Value < operand) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThan", operand.ToString("d"))}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<int> EnsureNotGreaterThan(this ArgumentUtilitiesHandle<int> hnd, int operand) {
			if (hnd.Value > operand) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", operand.ToString("d"))}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<int> EnsureEq(this ArgumentUtilitiesHandle<int> hnd, int operand) {
			if (hnd.Value == operand)
				return hnd;
			else {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "OutOfRange/WithRange", operand.ToString("d"), operand.ToString("d"))}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
		}

		public static ArgumentUtilitiesHandle<int?> EnsureNotGreaterThan(this ArgumentUtilitiesHandle<int?> hnd, int operand, Func<ArgumentUtilitiesHandle<int?>, string> exceptionMessageFirstLineFactory = null) {
			if (hnd.Value > operand) {
				var exceptionMessageFirstLine = exceptionMessageFirstLineFactory?.Invoke(hnd);
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{($"{(string.IsNullOrEmpty(exceptionMessageFirstLine) ? string.Empty : $"{exceptionMessageFirstLine}{Environment.NewLine}")}{FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", operand.ToString("d"))}")}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<int?> EnsureBetween(this ArgumentUtilitiesHandle<int?> hnd, int bound1Inclusive, int bound2Inclusive) {
			if (hnd.Value.HasValue && !hnd.Value.Value.Between(bound1Inclusive: bound1Inclusive, bound2Inclusive: bound2Inclusive)) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "OutOfRange/WithRange", (bound1Inclusive > bound2Inclusive ? bound2Inclusive : bound1Inclusive).FmtStr().Decimal(), (bound2Inclusive > bound1Inclusive ? bound2Inclusive : bound1Inclusive).FmtStr().Decimal())}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<int> EnsureGreaterThan(this ArgumentUtilitiesHandle<int> hnd, int operand) {
			if (hnd.Value > operand)
				return hnd;
			else {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "MustGreaterThan", operand.ToString("d"))}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
		}

		public static ArgumentUtilitiesHandle<int?> EnsureGreaterThan(this ArgumentUtilitiesHandle<int?> hnd, int operand) {
			if (!hnd.Value.HasValue || hnd.Value.Value > operand)
				return hnd;
			else {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "MustGreaterThan", operand.ToString("d"))}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
		}

	}

}