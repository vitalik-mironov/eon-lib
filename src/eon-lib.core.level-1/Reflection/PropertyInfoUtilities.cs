using System.Reflection;

namespace Eon.Reflection {

	public static class PropertyInfoUtilities {

		public static bool IsNonStatic(this PropertyInfo propertyInfo)
			=> IsNonStatic(hnd: propertyInfo.Arg(nameof(propertyInfo)), getAccessor: out _, setAccessor: out _);

		public static bool IsNonStatic(this PropertyInfo propertyInfo, out MethodInfo getAccessor, out MethodInfo setAccessor)
			=> IsNonStatic(hnd: propertyInfo.Arg(nameof(propertyInfo)), getAccessor: out getAccessor, setAccessor: out setAccessor);

		public static bool IsNonStatic(this ArgumentUtilitiesHandle<PropertyInfo> hnd, out MethodInfo getAccessor, out MethodInfo setAccessor) {
			hnd.EnsureNotNull();
			//
			var locGetAccessor = hnd.Value.GetGetMethod(true);
			var locSetAccessor = hnd.Value.GetSetMethod(true);
			var isStatic = locGetAccessor?.IsStatic ?? locSetAccessor.IsStatic;
			if (isStatic) {
				getAccessor = null;
				setAccessor = null;
				return false;
			}
			else {
				getAccessor = locGetAccessor;
				setAccessor = locSetAccessor;
				return true;
			}
		}

		public static bool IsNonStaticRead(this PropertyInfo propertyInfo)
			=> IsNonStaticRead(propertyInfo.Arg(nameof(propertyInfo)), out _);

		public static bool IsNonStaticRead(this ArgumentUtilitiesHandle<PropertyInfo> hnd)
			=> IsNonStaticRead(hnd, out _);

		public static bool IsNonStaticRead(this ArgumentUtilitiesHandle<PropertyInfo> hnd, out MethodInfo getAccessor) {
			MethodInfo getter, setter;
			hnd.EnsureNotNull().EnsureNonStatic(out getter, out setter);
			//
			if (getter is null) {
				getAccessor = null;
				return false;
			}
			else {
				getAccessor = getter;
				return true;
			}
		}

		public static bool IsRead(this PropertyInfo propertyInfo)
			=> IsRead(hnd: propertyInfo.Arg(nameof(propertyInfo)), getAccessor: out _);

		public static bool IsRead(this PropertyInfo propertyInfo, out MethodInfo getAccessor)
			=> IsRead(hnd: propertyInfo.Arg(nameof(propertyInfo)), getAccessor: out getAccessor);

		public static bool IsRead(this ArgumentUtilitiesHandle<PropertyInfo> hnd, out MethodInfo getAccessor) {
			hnd.EnsureNotNull();
			//
			var locGetAccessor = hnd.Value.GetGetMethod(true);
			if (locGetAccessor is null) {
				getAccessor = null;
				return false;
			}
			else {
				getAccessor = locGetAccessor;
				return true;
			}
		}

		public static bool IsWrite(this PropertyInfo propertyInfo)
			=> IsWrite(hnd: propertyInfo.Arg(nameof(propertyInfo)), setAccessor: out _);

		public static bool IsWrite(this PropertyInfo propertyInfo, out MethodInfo setAccessor)
			=> IsWrite(hnd: propertyInfo.Arg(nameof(propertyInfo)), setAccessor: out setAccessor);

		public static bool IsWrite(this ArgumentUtilitiesHandle<PropertyInfo> hnd, out MethodInfo setAccessor) {
			hnd.EnsureNotNull();
			//
			var locSetAccessor = hnd.Value.GetSetMethod(true);
			if (locSetAccessor is null) {
				setAccessor = null;
				return false;
			}
			else {
				setAccessor = locSetAccessor;
				return true;
			}
		}

		public static bool HasIndexParameters(this PropertyInfo propertyInfo)
			=> HasIndexParameters(propertyInfo.Arg(nameof(propertyInfo)));

		public static bool HasIndexParameters(this ArgumentUtilitiesHandle<PropertyInfo> hnd)
			=> hnd.EnsureNotNull().Value.GetIndexParameters().Length > 0;

	}

}