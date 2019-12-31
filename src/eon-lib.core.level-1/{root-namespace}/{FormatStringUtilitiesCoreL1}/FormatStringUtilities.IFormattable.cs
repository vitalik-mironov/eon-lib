using System;

namespace Eon {

	public static partial class FormatStringUtilities {

		/// <summary>
		/// Значение: 'G'.
		/// </summary>
		public static readonly string GLongFormatSpecifier = "G";

		/// <summary>
		/// Значение: 'G';
		/// </summary>
		public static readonly char GLongFormatSpecifierChar = 'G';

		/// <summary>
		/// Значение: 'g'.
		/// </summary>
		public static readonly string GShortFormatSpecifier = "g";

		/// <summary>
		/// Значение: 'g';
		/// </summary>
		public static readonly char GShortFormatSpecifierChar = 'g';

		public static string GLong<T>(this FormatStringUtilitiesHandle<T> hnd, IFormatProvider formatProvider)
			where T : IFormattable
			=> hnd.Object == null ? GetNullValueText(formatProvider: formatProvider) : hnd.Object.ToString(format: GLongFormatSpecifier, formatProvider: formatProvider);

		public static string GLong<T>(this FormatStringUtilitiesHandle<T> hnd)
			where T : IFormattable
			=> GLong(hnd: hnd, formatProvider: null);

		public static string GLongNlI<T>(this FormatStringUtilitiesHandle<T> hnd, IFormatProvider formatProvider)
			where T : IFormattable
			=> Environment.NewLine + GLong(hnd: hnd, formatProvider: formatProvider).IndentLines();

		public static string GLongNlI<T>(this FormatStringUtilitiesHandle<T> hnd)
			where T : IFormattable
			=> GLongNlI(hnd: hnd, formatProvider: null);

		public static string GLongNlI2<T>(this FormatStringUtilitiesHandle<T> hnd, IFormatProvider formatProvider)
			where T : IFormattable
			=> Environment.NewLine + GLong(hnd: hnd, formatProvider: formatProvider).IndentLines2();

		public static string GLongNlI2<T>(this FormatStringUtilitiesHandle<T> hnd)
			where T : IFormattable
			=> GLongNlI2(hnd: hnd, formatProvider: null);

		public static string G<T>(this FormatStringUtilitiesHandle<T?> hnd, IFormatProvider formatProvider)
			where T : struct, IFormattable
			=> hnd.Object.HasValue ? hnd.Object.Value.ToString(format: null, formatProvider: formatProvider) : GetNullValueText(formatProvider: formatProvider);

		public static string G<T>(this FormatStringUtilitiesHandle<T?> hnd)
			where T : struct, IFormattable
			=> hnd.Object.HasValue ? hnd.Object.Value.ToString(format: null, formatProvider: null) : GetNullValueText(formatProvider: null);

		public static string GI<T>(this FormatStringUtilitiesHandle<T> hnd, IFormatProvider formatProvider = null)
			where T : IFormattable
			=> (hnd.Object == null ? GetNullValueText(formatProvider: formatProvider) : hnd.Object.ToString(format: null, formatProvider: formatProvider)).IndentLines();

	}

}