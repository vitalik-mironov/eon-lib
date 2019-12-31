using System;
using System.Collections.Generic;
using System.Globalization;

using Eon.Runtime.Options;
using Eon.Threading;

namespace Eon.Resources.XResource {

	public static class XResourceUtilities {

		static readonly PrimitiveSpinLock __TextResourceAccessorsSpinLock;

		static readonly Dictionary<Type, ByTypeTextXResourceAccessor> __TextResourceAccessors;

		/// <summary>
		/// Value: 'urn:eon:eon-xresource-source'.
		/// </summary>
		public static readonly string EonSourceResourceName;

		public static readonly IXResourceSourcePointer EonSource;

		static XResourceUtilities() {
			__TextResourceAccessorsSpinLock = new PrimitiveSpinLock();
			__TextResourceAccessors = new Dictionary<Type, ByTypeTextXResourceAccessor>();
			EonSourceResourceName = "urn:eon:eon-xresource-source";
			EonSource = new AssemblyManifestXResourceSourcePointerAttribute(type: typeof(XResourceUtilities), resourceName: EonSourceResourceName);
		}

		/// <summary>
		/// Gets the current XResource service (see <see cref="IXResourceService"/>) set by option <see cref="XResourceServiceOption"/> (see <see cref="RuntimeOptions"/>).
		/// </summary>
		public static IXResourceService Service
			=> XResourceServiceOption.Require();

		public static string FormatXResource(Type locator, string subpath, params object[ ] args)
			=> Service.Format(locator: locator, subpath: subpath, args: args, throwIfMissing: true);

		public static string FormatXResource(IFormatProvider formatProvider, Type locator, string subpath, params object[ ] args)
			=> Service.Format(locator: locator, culture: formatProvider as CultureInfo, subpath: subpath, args: args, throwIfMissing: true);

		// TODO: Exceptions & mesages.
		//
		public static string FormatXResourceEnum(IFormatProvider formatProvider, object enumValue) {
			var enumType = enumValue?.GetType();
			if (enumType is null)
				return FormatStringUtilitiesCoreL0.GetNullValueText();
			else if (!enumType.IsEnum)
				throw
					new ArgumentException(
						message: $"Specified type is not enum type.{Environment.NewLine}\tType:{Environment.NewLine}\t\t{enumType}",
						paramName: nameof(enumValue));
			else if (enumType.IsDefined(attributeType: typeof(FlagsAttribute), inherit: false))
				throw
					new ArgumentException(
						message: $"Specified type is flag enum type. Such type is not supported.{Environment.NewLine}\tType:{Environment.NewLine}\t\t{enumType}",
						paramName: nameof(enumValue));
			else
				return FormatXResource(formatProvider: formatProvider, locator: enumType, subpath: Enum.GetName(enumType: enumType, value: enumValue), args: new object[ ] { enumValue });
		}

		public static string FormatXResourceEnum(object enumValue)
			=> FormatXResourceEnum(formatProvider: null, enumValue: enumValue);

		public static ByTypeTextXResourceAccessor GetTextXResourceAccessor(Type type) {
			type.EnsureNotNull(nameof(type));
			//
			var existing =
				__TextResourceAccessorsSpinLock
				.Invoke(
					func:
						() => {
							__TextResourceAccessors.TryGetValue(key: type, value: out var locExisting);
							return locExisting;
						});
			if (existing is null) {
				existing = new ByTypeTextXResourceAccessor(type: type, provider: Service);
				existing =
					__TextResourceAccessorsSpinLock
					.Invoke(
						func:
							() => {
								try {
									__TextResourceAccessors.Add(key: type, value: existing);
									return existing;
								}
								catch (ArgumentException) {
									return __TextResourceAccessors[ type ];
								}
							});
			}
			return existing;
		}

	}

}