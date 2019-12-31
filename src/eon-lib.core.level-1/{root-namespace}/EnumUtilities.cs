using System;
using System.Collections.Generic;

using Eon.Collections;
using Eon.Reflection;
using Eon.Threading;

namespace Eon {

	public static class EnumUtilities {

		#region Nested types

		abstract class P_EnumTypeApi {

			protected P_EnumTypeApi() { }

			public abstract object Parse(string value, bool ignoreCase = false);

		}

		sealed class P_EnumTypeApi<TEnum>
			:P_EnumTypeApi
			where TEnum : struct {

			internal P_EnumTypeApi() { }

			public override object Parse(string value, bool ignoreCase = false)
				=> Parse<TEnum>(value: value.EnsureNotNull(nameof(value)).Value, ignoreCase: ignoreCase).Value;
		}

		#endregion

		static readonly Dictionary<Type, P_EnumTypeApi> __EnumTypeApiCache;

		static readonly PrimitiveSpinLock __EnumTypeApiCacheSpinLock;

		static EnumUtilities() {
			__EnumTypeApiCache = new Dictionary<Type, P_EnumTypeApi>();
			__EnumTypeApiCacheSpinLock = new PrimitiveSpinLock();
		}

		static P_EnumTypeApi P_GetEnumTypeApi(Type type)
			=>
			__EnumTypeApiCache
			.GetOrAdd(
				spinLock: __EnumTypeApiCacheSpinLock,
				key: type.EnsureNotNull(nameof(type)).EnsureEnum().Value,
				factory: (locKey) => ActivationUtilities.RequireConstructor<P_EnumTypeApi>(concreteType: typeof(P_EnumTypeApi<>).MakeGenericType(locKey))());

		public static TEnum? Parse<TEnum>(string value, bool ignoreCase = false)
			where TEnum : struct
			=> value.Arg(nameof(value)).ParseEnumNull<TEnum>(ignoreCase: ignoreCase);

		public static object Parse(string value, Type type, bool ignoreCase = default)
			=> P_GetEnumTypeApi(type: type).Parse(value: value, ignoreCase: ignoreCase);

	}

}