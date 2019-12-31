using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;

using Eon.Text;

using static Eon.Resources.XResource.XResourceUtilities;

using FmtStrUtils = Eon.FormatStringUtilities;

namespace Eon {

	/// <summary>
	/// Утилиты приведения объектов к текстовому представления.
	/// </summary>
	public static partial class FormatStringUtilities {

		public const string TruncateEllipsis = "[…]";

		public const string ShortFormatSpecifier = "v";

		public const string LongFormatSpecifier = "V";

		public static FormatStringUtilitiesHandle<T> FmtStr<T>(this T obj)
			=> new FormatStringUtilitiesHandle<T>(obj: obj);

		public static string NamedPartsDHMS(this FormatStringUtilitiesHandle<TimeSpan> hnd)
			=> P_NamedPartsDHMS(hnd.Object);

		public static string GetNullValueText()
			=> FormatStringUtilitiesCoreL0.GetNullValueText();

#pragma warning disable IDE0060 // Remove unused parameter
		public static string GetNullValueText(IFormatProvider formatProvider)
#pragma warning restore IDE0060 // Remove unused parameter
			=> FormatStringUtilitiesCoreL0.GetNullValueText();

		// TODO: Put strings into the resources.
		//
		static string P_NamedPartsDHMS(TimeSpan timeSpan)
			=> $"{timeSpan.Days.ToString("d")} дн., {timeSpan.Hours.ToString("d")} ч., {timeSpan.Minutes.ToString("d")} мин., {timeSpan.Seconds.ToString("d")} сек.";

		public static string NamedPartsDHMS(this FormatStringUtilitiesHandle<TimeSpan?> handle)
			=> handle.Object.HasValue ? P_NamedPartsDHMS(handle.Object.Value) : GetNullValueText();

		public static string YesNo(this FormatStringUtilitiesHandle<bool> obj)
			=> FormatXResource(typeof(FmtStrUtils), obj.Object ? "TrueValue" : "FalseValue");

		public static string YesNo(this FormatStringUtilitiesHandle<bool?> obj)
			=> obj.Object == null ? FmtStrUtils.GetNullValueText() : (FormatXResource(typeof(FmtStrUtils), obj.Object.Value ? "TrueValue" : "FalseValue"));

		public static string G<T>(this FormatStringUtilitiesHandle<T> hnd, IFormatProvider formatProvider) {
			IFormattable formattableObject;
			if (hnd.Object == null)
				return GetNullValueText(formatProvider: formatProvider);
			else if ((formattableObject = hnd.Object as IFormattable) is null)
				return hnd.Object.ToString();
			else
				return formattableObject.ToString(format: null, formatProvider: formatProvider);
		}

		public static string PrefixVInvariant(this FormatStringUtilitiesHandle<Version> hnd)
			=> hnd.Object is null ? GetNullValueText(formatProvider: CultureInfo.InvariantCulture) : "v" + G(hnd: hnd, formatProvider: CultureInfo.InvariantCulture);

		public static string GInvariant(this FormatStringUtilitiesHandle<Version> hnd)
			=> G(hnd: hnd, formatProvider: CultureInfo.InvariantCulture);

		public static string G(this FormatStringUtilitiesHandle<Version> hnd)
			=> G(hnd: hnd, formatProvider: null);

		public static string G(this FormatStringUtilitiesHandle<Version> hnd, IFormatProvider formatProvider) {
			if (hnd.Object == null)
				return GetNullValueText(formatProvider: formatProvider);
			else {
				var version = hnd.Object;
				if (version.Revision > 0)
					return version.ToString(fieldCount: 4);
				else if (version.Build > 0)
					return version.ToString(fieldCount: 3);
				else if (version.Minor > -1)
					return version.ToString(fieldCount: 2);
				else
					return version.ToString(fieldCount: 1);
			}
		}

		public static string GNLI<T>(this FormatStringUtilitiesHandle<T> hnd, IFormatProvider formatProvider)
			=> Environment.NewLine + G(hnd: hnd, formatProvider: formatProvider).IndentLines();

		public static string GNLI<T>(this FormatStringUtilitiesHandle<T> hnd)
			=> Environment.NewLine + G(hnd: hnd, formatProvider: null).IndentLines();

		public static string GI<T>(this FormatStringUtilitiesHandle<T> hnd)
			=> hnd.G().IndentLines();

		public static string GNLI2<T>(this FormatStringUtilitiesHandle<T> hnd)
			=> Environment.NewLine + hnd.G().IndentLines2();

		public static string GNLI3<T>(this FormatStringUtilitiesHandle<T> hnd)
			=> Environment.NewLine + hnd.G().IndentLines3();

		public static string GNLI2(this FormatStringUtilitiesHandle<Version> hnd)
			=> Environment.NewLine + hnd.G().IndentLines2();

		public static string GNLI(this FormatStringUtilitiesHandle<Version> hnd)
			=> Environment.NewLine + hnd.G().IndentLines();

		public static string GI2<T>(this FormatStringUtilitiesHandle<T> hnd)
			=> hnd.G().IndentLines2();

		public static string G<T>(this FormatStringUtilitiesHandle<T> hnd)
			=> hnd.Object == null ? GetNullValueText() : hnd.Object.ToString();

		public static string G(this FormatStringUtilitiesHandle<SocketError> hnd)
			=> $"{hnd.Object.ToString("d")} ({hnd.Object.ToString("g")})";

		public static string G(this FormatStringUtilitiesHandle<TypeInfo> hnd)
			=> P_GeneralForMemberInfo(hnd.Object);

		public static string GForMemberInfo<T>(this FormatStringUtilitiesHandle<T> hnd)
			where T : MemberInfo
			=> P_GeneralForMemberInfo(hnd.Object);

		static string P_GeneralForMemberInfo(MemberInfo memberInfo, string overrideMemberInfoText = default, IFormatProvider formatProvider = default) {
			if (memberInfo is null)
				return GetNullValueText(formatProvider: formatProvider);
			else {
				Type declaringType;
				if (memberInfo is TypeInfo || (declaringType = memberInfo.DeclaringType) == null)
					return overrideMemberInfoText ?? memberInfo.ToString();
				else
					return $"{declaringType}: {(overrideMemberInfoText ?? memberInfo.ToString())}";
			}
		}

		public static string Decimal(this FormatStringUtilitiesHandle<int> hnd, IFormatProvider formatProvider)
			=> hnd.Object.ToString("d", formatProvider);

		public static string Decimal(this FormatStringUtilitiesHandle<int> hnd)
			=> hnd.Object.ToString(format: "d", provider: null);

		public static string Decimal(this FormatStringUtilitiesHandle<int?> hnd, IFormatProvider formatProvider)
			=> hnd.Object.HasValue ? hnd.Object.Value.ToString("d", formatProvider) : GetNullValueText(formatProvider: formatProvider);

		public static string Decimal(this FormatStringUtilitiesHandle<int?> hnd)
			=> hnd.Object.HasValue ? hnd.Object.Value.ToString(format: "d", provider: null) : GetNullValueText();

		public static string Decimal(this FormatStringUtilitiesHandle<long> hnd, IFormatProvider formatProvider)
			=> hnd.Object.ToString("d", provider: formatProvider);

		public static string Decimal(this FormatStringUtilitiesHandle<long> hnd)
			=> hnd.Object.ToString("d", provider: null);

		public static string Decimal(this FormatStringUtilitiesHandle<long?> hnd, IFormatProvider formatProvider)
			=> hnd.Object.HasValue ? hnd.Object.Value.ToString("d", provider: formatProvider) : GetNullValueText(formatProvider: formatProvider);

		public static string Decimal(this FormatStringUtilitiesHandle<long?> hnd)
			=> hnd.Object.HasValue ? hnd.Object.Value.ToString("d", provider: null) : GetNullValueText();

		public static string DecimalInvariant(this FormatStringUtilitiesHandle<long> hnd)
			=> hnd.Object.ToString("d", CultureInfo.InvariantCulture);

		public static string DecimalInvariant(this FormatStringUtilitiesHandle<long?> hnd)
			=> hnd.Object.HasValue ? hnd.Object.Value.ToString("d", CultureInfo.InvariantCulture) : GetNullValueText(formatProvider: CultureInfo.InvariantCulture);

		public static string N(this FormatStringUtilitiesHandle<Guid> hnd)
			=> hnd.Object.ToString("n");

		public static string D(this FormatStringUtilitiesHandle<Guid> hnd)
			=> hnd.Object.ToString("d");

		public static string Constant(this FormatStringUtilitiesHandle<TimeSpan> hnd)
			=> hnd.Object.ToString("c");

		// TODO: Put strings into the resources.
		//
		public static string FormatCommandLineArgs() {
			string[ ] args;
			var notSupported = false;
			try { args = Environment.GetCommandLineArgs(); }
			catch (NotSupportedException) { args = null; notSupported = true; }
			if (notSupported)
				return "<не поддерживается>";
			else if (args.IsNullOrEmpty())
				return "<отсутсвуют>";
			else {
				using (var acquiredBuffer = StringBuilderUtilities.AcquireBuffer()) {
					var sb = acquiredBuffer.StringBuilder;
					sb.Append(args[ 0 ]);
					for (var i = 1; i < args.Length; i++) {
						sb.Append(Environment.NewLine);
						sb.Append('\t');
						sb.Append(args[ i ]);
					}
					return sb.ToString();
				}
			}
		}

	}

}