using System;
using System.Reflection;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		public static ArgumentUtilitiesHandle<TypeInfo> EnsureInterface(this ArgumentUtilitiesHandle<TypeInfo> hnd) {
			if (hnd.Value == null || hnd.Value.IsInterface)
				return hnd;
			throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(Type), "NotInterface", hnd.Value));
		}

		public static ArgumentUtilitiesHandle<TypeInfo> EnsureClass(this ArgumentUtilitiesHandle<TypeInfo> argument) {
			if (argument.Value == null || argument.Value.IsClass)
				return argument;
			throw new ArgumentOutOfRangeException(argument.Name, FormatXResource(typeof(Type), "NotClass", argument.Value));
		}

	}

}