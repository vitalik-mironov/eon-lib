using System;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<T> EnsureReadOnly<T>(this ArgumentUtilitiesHandle<T> hnd)
			where T : IIsReadOnlyProperty {
			if (hnd.Value == null)
				return hnd;
			else if (!hnd.Value.IsReadOnly) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'." : "Invalid value specified.")}{Environment.NewLine}This object is muttable (see '{hnd.Name}.{nameof(hnd.Value.IsReadOnly)}'). Immutable object expected.{Environment.NewLine}\tObject:{hnd.Value.FmtStr().GNLI2()}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<T> EnsureNotReadOnly<T>(this ArgumentUtilitiesHandle<T> hnd)
			where T : IIsReadOnlyProperty {
			if (hnd.Value == null)
				return hnd;
			else if (hnd.Value.IsReadOnly) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'." : "Invalid value specified.")}{Environment.NewLine}This object is immutable (see '{hnd.Name}.{nameof(hnd.Value.IsReadOnly)}'). Muttable object expected.{Environment.NewLine}\tObject:{hnd.Value.FmtStr().GNLI2()}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

	}

}