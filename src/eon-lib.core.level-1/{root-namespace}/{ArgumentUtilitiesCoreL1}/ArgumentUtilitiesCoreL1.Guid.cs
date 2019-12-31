using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		public static ArgumentUtilitiesHandle<Guid> EnsureNotEmpty(this ArgumentUtilitiesHandle<Guid> hnd) {
			if (hnd.Value == Guid.Empty) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Guid), "SuchNotAllowed/WithValue", hnd.Value.ToString())}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<Guid?> EnsureNotEmpty(this ArgumentUtilitiesHandle<Guid?> hnd) {
			if (hnd.Value.HasValue && hnd.Value.Value == Guid.Empty) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Guid), "SuchNotAllowed/WithValue", hnd.Value.Value.ToString())}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

	}

}