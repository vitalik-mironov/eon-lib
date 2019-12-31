using System;

namespace Eon {

	public static partial class FormatStringUtilities {

		/// <summary>
		/// Значение: '16384'.
		/// </summary>
		public static readonly int StringGFormatMaxLength = 16384;

		/// <summary>
		/// Значение: '16384'.
		/// </summary>
		public static readonly int StringGLongFormatMaxLength = 16384;

		public static string Truncated(this FormatStringUtilitiesHandle<string> hnd)
			=> Truncated(hnd: hnd, maxLength: StringGFormatMaxLength);

		public static string Truncated(this FormatStringUtilitiesHandle<string> hnd, int maxLength)
			=> hnd.Object is null ? GetNullValueText() : (hnd.Object == string.Empty ? string.Empty : StringUtilities.Truncate(original: hnd.Object, maxTruncatedLength: maxLength, ellipsis: TruncateEllipsis, mode: StringUtilities.TruncateMode.Middle));

		public static string Pii(this FormatStringUtilitiesHandle<string> hnd)
			=> Pii(hnd: hnd, formatProvider: null);

		// TODO: Put strings into the resources.
		//
		public static string Pii(this FormatStringUtilitiesHandle<string> hnd, IFormatProvider formatProvider) {
			var value = hnd.Object;
			if (value is null)
				return GetNullValueText(formatProvider: formatProvider);
			else if (value == string.Empty)
				return "<пустая строка>";
			else
				return $"<представлено, длина {hnd.Object.Length.ToString(format: "d", provider: formatProvider)}>";
		}

		public static string G(this FormatStringUtilitiesHandle<string> hnd)
			=> G(hnd: hnd, formatProvider: null);

		// TODO: Put strings into the resources.
		//
		public static string G(this FormatStringUtilitiesHandle<string> hnd, IFormatProvider formatProvider) {
			var value = hnd.Object;
			if (value is null)
				return GetNullValueText(formatProvider: formatProvider);
			else if (value == string.Empty)
				return "<пустая строка>";
			else if (char.IsWhiteSpace(value, 0) || char.IsWhiteSpace(value, value.Length - 1))
				return $"'{StringUtilities.Truncate(original: value, maxTruncatedLength: StringGFormatMaxLength - 2, mode: StringUtilities.TruncateMode.Middle)}'";
			else
				return StringUtilities.Truncate(original: value, maxTruncatedLength: StringGFormatMaxLength, mode: StringUtilities.TruncateMode.Middle);
		}

		public static string GNLI(this FormatStringUtilitiesHandle<string> hnd)
			=> GNLI(hnd, formatProvider: null);

		public static string GNLI(this FormatStringUtilitiesHandle<string> hnd, IFormatProvider formatProvider)
			=> Environment.NewLine + G(hnd: hnd, formatProvider: formatProvider).IndentLines();

		public static string GNLI2(this FormatStringUtilitiesHandle<string> hnd)
			=> GNLI2(hnd, formatProvider: null);

		public static string GNLI2(this FormatStringUtilitiesHandle<string> hnd, IFormatProvider formatProvider)
			=> Environment.NewLine + G(hnd: hnd, formatProvider: formatProvider).IndentLines2();

		public static string GNLI3(this FormatStringUtilitiesHandle<string> hnd)
			=> GNLI3(hnd, formatProvider: null);

		public static string GNLI3(this FormatStringUtilitiesHandle<string> hnd, IFormatProvider formatProvider)
			=> Environment.NewLine + G(hnd: hnd, formatProvider: formatProvider).IndentLines3();

		public static string GI(this FormatStringUtilitiesHandle<string> hnd)
			=> GI(hnd, formatProvider: null);

		public static string GI(this FormatStringUtilitiesHandle<string> hnd, IFormatProvider formatProvider)
			=>
			G(hnd, formatProvider: formatProvider)
			.IndentLines();

		public static string GI2(this FormatStringUtilitiesHandle<string> hnd)
			=> GI2(hnd, formatProvider: null);

		public static string GI2(this FormatStringUtilitiesHandle<string> hnd, IFormatProvider formatProvider)
			=>
			G(hnd, formatProvider: formatProvider)
			.IndentLines2();

		public static string PiiNLI(this FormatStringUtilitiesHandle<string> hnd)
			=> Environment.NewLine + Pii(hnd: hnd, formatProvider: null).IndentLines();

		public static string PiiNLI(this FormatStringUtilitiesHandle<string> hnd, IFormatProvider formatProvider)
			=> Environment.NewLine + Pii(hnd: hnd, formatProvider: formatProvider).IndentLines();

		public static string PiiNLI2(this FormatStringUtilitiesHandle<string> hnd)
			=> Environment.NewLine + Pii(hnd: hnd, formatProvider: null).IndentLines2();

		public static string PiiNLI2(this FormatStringUtilitiesHandle<string> hnd, IFormatProvider formatProvider)
			=> Environment.NewLine + Pii(hnd: hnd, formatProvider: formatProvider).IndentLines2();

		public static string PiiNLI3(this FormatStringUtilitiesHandle<string> hnd)
			=> Environment.NewLine + Pii(hnd: hnd, formatProvider: null).IndentLines3();

		public static string PiiNLI3(this FormatStringUtilitiesHandle<string> hnd, IFormatProvider formatProvider)
			=> Environment.NewLine + Pii(hnd: hnd, formatProvider: formatProvider).IndentLines3();

	}

}