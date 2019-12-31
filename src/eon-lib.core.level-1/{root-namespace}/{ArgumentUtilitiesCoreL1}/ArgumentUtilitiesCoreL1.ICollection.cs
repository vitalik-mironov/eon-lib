using System;
using System.Collections.Generic;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<ICollection<T>> EnsureNotReadOnly<T>(this ArgumentUtilitiesHandle<ICollection<T>> hnd) {
			if (hnd.Value?.IsReadOnly == true) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}Specified collection is read-only.";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

	}

}