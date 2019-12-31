using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		public static TimeSpan EnsureNotGreaterThan(this TimeSpan value, TimeSpan operand, string name) {
			if (value > operand)
				throw new ArgumentOutOfRangeException(string.IsNullOrEmpty(name) ? nameof(value) : name, FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", operand.ToString("c")));
			return value;
		}

		public static ArgumentUtilitiesHandle<TimeSpan> EnsureNotGreaterThan(this ArgumentUtilitiesHandle<TimeSpan> hnd, TimeSpan operand) {
			EnsureNotGreaterThan(hnd.Value, operand, hnd.Name);
			return hnd;
		}

		public static ArgumentUtilitiesHandle<TimeSpan> EnsureAbsNotGreaterThan(this ArgumentUtilitiesHandle<TimeSpan> hnd, TimeSpan operand) {
			if (hnd.Value.Abs() > operand) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}Значение по модулю не может быть больше, чем '{operand:c}'.";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<TimeSpan> EnsureNotLessThan(this ArgumentUtilitiesHandle<TimeSpan> hnd, TimeSpan operand) {
			if (hnd.Value < operand) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThan", operand.ToString("c"))}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<TimeSpan> EnsureGreaterThanZero(this TimeSpan value, string name)
			=> EnsureGreaterThanZero(new ArgumentUtilitiesHandle<TimeSpan>(value, name));

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<TimeSpan> EnsureGreaterThanZero(this ArgumentUtilitiesHandle<TimeSpan> hnd) {
			if (hnd.Value <= TimeSpan.Zero) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "MustGreaterThan", TimeSpan.Zero.ToString("c"))}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<TimeSpan> EnsureGreaterThan(this ArgumentUtilitiesHandle<TimeSpan> hnd, TimeSpan operand) {
			if (hnd.Value <= operand)
				throw
					new ArgumentOutOfRangeException(
						paramName: hnd.Name,
						message: $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "MustGreaterThan", operand.ToString("c"))}");
			else
				return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<TimeSpan> EnsureTimeOfDay(this ArgumentUtilitiesHandle<TimeSpan> hnd) {
			if (TimeSpanUtilities.IsTimeOfDay(hnd.Value))
				return hnd;
			else
				throw
					new ArgumentOutOfRangeException(
						message: $"Указанное значение '{hnd.Value.ToString("c")}' не представляет время суток.",
						paramName: hnd.Name);
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<TimeSpan?> EnsureTimeOfDay(this ArgumentUtilitiesHandle<TimeSpan?> argument) {
			if (argument.Value.HasValue && !TimeSpanUtilities.IsTimeOfDay(argument.Value.Value))
				throw
					new ArgumentOutOfRangeException(
						message: $"Указанное значение '{argument.Value.Value.ToString("c")}' не представляет время суток.",
						paramName: argument.Name);
			else
				return argument;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<TimeSpan> EnsureHasMaxScaleOfSeconds(this ArgumentUtilitiesHandle<TimeSpan> argument) {
			if (TimeSpanUtilities.HasMaxScaleOfSeconds(ts: argument.Value))
				return argument;
			else
				throw
					new ArgumentOutOfRangeException(
						message: $"Указанное значение '{argument.Value.ToString("c")}' имеет недопустимый масштаб. Ожидаемый максимальный масштаб — 1 секунда.",
						paramName: argument.Name);
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<TimeSpan?> EnsureHasMaxScaleOfSeconds(this ArgumentUtilitiesHandle<TimeSpan?> argument) {
			if (argument.Value.HasValue && !TimeSpanUtilities.HasMaxScaleOfSeconds(argument.Value.Value))
				throw
					new ArgumentOutOfRangeException(
						message: $"Указанное значение '{argument.Value.Value.ToString("c")}' имеет недопустимый масштаб. Ожидаемый максимальный масштаб — 1 секунда.",
						paramName: argument.Name);
			else
				return argument;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<TimeSpan> EnsureHasMaxScaleOfMilliSeconds(this ArgumentUtilitiesHandle<TimeSpan> hnd) {
			if (TimeSpanUtilities.HasMaxScaleOfMilliSeconds(ts: hnd.Value))
				return hnd;
			else {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}Указанное значение '{hnd.Value.ToString("c")}' имеет недопустимый масштаб. Ожидаемый максимальный масштаб — 1 миллисекунда.";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<TimeSpan?> EnsureHasMaxScaleOfMilliSeconds(this ArgumentUtilitiesHandle<TimeSpan?> hnd) {
			if (hnd.Value.HasValue && !TimeSpanUtilities.HasMaxScaleOfMilliSeconds(ts: hnd.Value.Value)) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}Указанное значение '{hnd.Value.Value.ToString("c")}' имеет недопустимый масштаб. Ожидаемый максимальный масштаб — 1 миллисекунда.";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

	}

}