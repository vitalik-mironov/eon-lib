using System.Runtime.CompilerServices;

namespace Eon {

	public static partial class ArgumentUtilities {

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static ArgumentUtilitiesHandle<string> EnsureNotNull(this string value, string name) {
			if (value is null)
				throw Internal_CreateNullValueException(isPropertyValue: false, name: name, exceptionFactory: null);
			else
				return new ArgumentUtilitiesHandle<string>(value: value, name: name);
		}

	}

}