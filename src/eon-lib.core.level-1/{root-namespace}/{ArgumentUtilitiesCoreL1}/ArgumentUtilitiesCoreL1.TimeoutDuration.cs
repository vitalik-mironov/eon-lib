using System;

using Eon.Threading;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<TimeoutDuration> EnsureNotInfinite(this ArgumentUtilitiesHandle<TimeoutDuration> hnd) {
			if (hnd.Value?.IsInfinite == true) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}Указанное значение '{hnd.Value.Milliseconds:d}' ({hnd.Value.TimeSpan.ToString("c")}) представляет бесконечный таймаут. Бесконечный таймаут недопустим.";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(
						paramName: hnd.Name,
						message: exceptionMessage);
			}
			else
				return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<TimeoutDuration> EnsureNotLessThan(this ArgumentUtilitiesHandle<TimeoutDuration> hnd, TimeSpan operand) {
			if (operand > TimeSpan.Zero && hnd.Value?.IsInfinite == false && hnd.Value.TimeSpan < operand) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThan", operand.ToString("c"))}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

	}

}