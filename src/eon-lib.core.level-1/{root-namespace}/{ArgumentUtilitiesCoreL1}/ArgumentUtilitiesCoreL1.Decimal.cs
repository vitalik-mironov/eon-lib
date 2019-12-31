using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<decimal> EnsureHasMaxScaleOf(this ArgumentUtilitiesHandle<decimal> hnd, int scale) {
			scale.EnsureNotLessThanZero(nameof(scale));
			//
			if (hnd.Value == Math.Round(d: hnd.Value, decimals: scale))
				return hnd;
			else {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}Максимальная точность значения должна быть не более, чем '{scale.ToString("d")}' знака дробной части.";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentException(paramName: hnd.Name, message: exceptionMessage);
			}
		}

		public static ArgumentUtilitiesHandle<decimal?> EnsureHasMaxScaleOf(this ArgumentUtilitiesHandle<decimal?> hnd, int scale) {
			scale.EnsureNotLessThanZero(nameof(scale));
			//
			if (hnd.Value.HasValue && hnd.Value.Value != Math.Round(d: hnd.Value.Value, decimals: scale)) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}Максимальная точность значения должна быть не более, чем '{scale.ToString("d")}' знака дробной части.";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<decimal> EnsureNotLessThan(this ArgumentUtilitiesHandle<decimal> hnd, decimal operand) {
			if (hnd.Value < operand) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThan", operand.ToString("g"))}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<decimal?> EnsureNotLessThan(this ArgumentUtilitiesHandle<decimal?> hnd, decimal operand) {
			if (hnd.Value.HasValue && hnd.Value.Value < operand) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThan", operand.ToString("g"))}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<decimal> EnsureNotGreaterThan(this ArgumentUtilitiesHandle<decimal> hnd, decimal operand) {
			if (hnd.Value > operand) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", operand.ToString("g"))}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<decimal?> EnsureNotGreaterThan(this ArgumentUtilitiesHandle<decimal?> hnd, decimal operand) {
			if (hnd.Value.HasValue && hnd.Value.Value > operand) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", operand.ToString("g"))}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<decimal> EnsureBetween(this ArgumentUtilitiesHandle<decimal> hnd, decimal bound1Inclusive, decimal bound2Inclusive) {
			if (NumberUtilities.Between(number: hnd.Value, a: bound1Inclusive, b: bound2Inclusive))
				return hnd;
			else {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "OutOfRange/WithRange", (bound1Inclusive > bound2Inclusive ? bound2Inclusive : bound1Inclusive).ToString("g"), (bound2Inclusive > bound1Inclusive ? bound2Inclusive : bound1Inclusive).ToString("g"))}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
		}

		public static ArgumentUtilitiesHandle<decimal?> EnsureBetween(this ArgumentUtilitiesHandle<decimal?> hnd, decimal bound1Inclusive, decimal bound2Inclusive) {
			if (!hnd.Value.HasValue || NumberUtilities.Between(number: hnd.Value.Value, a: bound1Inclusive, b: bound2Inclusive))
				return hnd;
			else {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "OutOfRange/WithRange", (bound1Inclusive > bound2Inclusive ? bound2Inclusive : bound1Inclusive).ToString("g"), (bound2Inclusive > bound1Inclusive ? bound2Inclusive : bound1Inclusive).ToString("g"))}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
		}

	}

}