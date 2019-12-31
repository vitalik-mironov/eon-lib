using System;

namespace Eon {

	public static partial class ArgumentUtilitiesUri {

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<UriBasedIdentifier> EnsureNotUndefined(this ArgumentUtilitiesHandle<UriBasedIdentifier> hnd) {
			if (hnd.Value == UriBasedIdentifier.Undefined) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Указано недопустимое значение свойства '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}В качестве значения не может использоваться неопределенный идентификатор — '{UriBasedIdentifier.Undefined}'.";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			} else
				return hnd;
		}

	}

}