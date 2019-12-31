using System;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<T> EnsureNotDisabled<T>(this ArgumentUtilitiesHandle<T> hnd)
			where T : IAbilityOption {
			if (hnd.Value?.IsDisabled == true) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'." : "Invalid value specified.")}{Environment.NewLine}Functional use of this object is disabled (see '{hnd.Name}.{nameof(hnd.Value.IsDisabled)}').{Environment.NewLine}\tObject:{hnd.Value.FmtStr().GNLI2()}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

	}

}