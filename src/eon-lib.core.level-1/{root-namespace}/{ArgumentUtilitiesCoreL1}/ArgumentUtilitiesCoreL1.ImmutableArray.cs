using System;
using System.Collections.Immutable;
using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<ImmutableArray<T>> EnsureNotEmptyOrDefault<T>(this ArgumentUtilitiesHandle<ImmutableArray<T>> hnd) {
			if (hnd.Value.IsDefault) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}Указанное значение не содержит экземпляр массива.";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentException(paramName: hnd.Name, message: exceptionMessage);
			} else if (hnd.Value.IsEmpty) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(Array), "CanNotEmpty")}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentException(paramName: hnd.Name, message: exceptionMessage);
			} else
				return hnd;
		}

	}

}