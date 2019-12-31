using System;
using System.Reflection;
using Eon.Reflection;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		// TODO: Put exception messages into the resources.
		//
		public static ArgumentUtilitiesHandle<PropertyInfo> EnsureNonStatic(this ArgumentUtilitiesHandle<PropertyInfo> hnd, out MethodInfo getAccessor, out MethodInfo setAccessor) {
			if (hnd.Value != null) {
				if (!PropertyInfoUtilities.IsNonStatic(hnd, out getAccessor, out setAccessor))
					throw
						new ArgumentException(
							paramName: hnd.Name,
							message: $"Указанное свойство является статическим.{Environment.NewLine}\tСвойство:{hnd.Value.FmtStr().GNLI2()}");
			}
			else {
				getAccessor = null;
				setAccessor = null;
			}
			return hnd;
		}

		public static ArgumentUtilitiesHandle<PropertyInfo> EnsureNonStatic(this ArgumentUtilitiesHandle<PropertyInfo> hnd)
			=> EnsureNonStatic(hnd, out _, out _);

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<PropertyInfo> EnsureNonStaticRead(this ArgumentUtilitiesHandle<PropertyInfo> hnd, out MethodInfo getAccessor) {
			if (!(hnd.Value is null)) {
				if (!PropertyInfoUtilities.IsNonStaticRead(hnd: hnd, getAccessor: out getAccessor))
					throw
						new ArgumentException(
							message: $"Указанное свойство является недоступным для чтения.{Environment.NewLine}\tСвойство:{hnd.Value.FmtStr().GNLI2()}",
							paramName: hnd.Name);
			}
			else
				getAccessor = null;
			return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<PropertyInfo> EnsureRead(this ArgumentUtilitiesHandle<PropertyInfo> hnd) {
			if (hnd.Value != null && !hnd.Value.IsRead())
				throw
					new ArgumentException(
						message: $"Указанное свойство является недоступным для чтения.{Environment.NewLine}\tСвойство:{hnd.Value.FmtStr().GNLI2()}",
						paramName: hnd.Name);
			else
				return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<PropertyInfo> EnsureWrite(this ArgumentUtilitiesHandle<PropertyInfo> hnd) {
			if (hnd.Value != null && !hnd.Value.IsWrite())
				throw
					new ArgumentException(
						message: $"Указанное свойство является недоступным для записи.{Environment.NewLine}\tСвойство:{hnd.Value.FmtStr().GNLI2()}",
						paramName: hnd.Name);
			else
				return hnd;
		}

	}

}