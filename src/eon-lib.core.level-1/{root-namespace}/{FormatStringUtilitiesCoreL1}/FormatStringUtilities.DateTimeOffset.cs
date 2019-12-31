using System;
using System.Globalization;

namespace Eon {

	public static partial class FormatStringUtilities {

		public static string GShort(this FormatStringUtilitiesHandle<DateTimeOffset?> obj)
			=>
			GShort(
				obj: obj,
				formatProvider: null);

		public static string GShort(this FormatStringUtilitiesHandle<DateTimeOffset?> obj, IFormatProvider formatProvider)
			=>
			obj.Object.HasValue
			? $"{obj.Object.Value.ToString(format: "g", formatProvider: formatProvider)} ({obj.Object.Value.ToString(format: " K", formatProvider: formatProvider).Trim()})"
			: GetNullValueText(formatProvider: formatProvider);

		public static string GShort(this FormatStringUtilitiesHandle<DateTimeOffset> hnd)
			=> GShort(hnd: hnd, formatProvider: null);

		public static string GShort(this FormatStringUtilitiesHandle<DateTimeOffset> hnd, IFormatProvider formatProvider)
			=> $"{hnd.Object.ToString(format: "g", formatProvider: formatProvider)} ({hnd.Object.ToString(format: " K", formatProvider: formatProvider).Trim()})";

		public static string G(this FormatStringUtilitiesHandle<DateTimeOffset?> obj, IFormatProvider formatProvider = null)
			=> obj.Object.HasValue ? obj.Object.Value.ToString(format: null, formatProvider: formatProvider) : GetNullValueText(formatProvider: formatProvider);

		public static string RoundTrip(this FormatStringUtilitiesHandle<DateTimeOffset?> obj, IFormatProvider formatProvider)
			=> obj.Object.HasValue ? obj.Object.Value.ToString(format: "o", formatProvider: formatProvider) : FormatStringUtilities.GetNullValueText(formatProvider: formatProvider);

		public static string RoundTrip(this FormatStringUtilitiesHandle<DateTimeOffset?> obj)
			=> obj.Object.HasValue ? obj.Object.Value.ToString(format: "o", formatProvider: null) : FormatStringUtilities.GetNullValueText(formatProvider: null);

		public static string RoundTrip(this FormatStringUtilitiesHandle<DateTimeOffset> hnd)
			=> hnd.Object.ToString(format: "o", formatProvider: null);

		public static string RoundTrip(this FormatStringUtilitiesHandle<DateTimeOffset> hnd, IFormatProvider formatProvider)
			=> hnd.Object.ToString(format: "o", formatProvider: formatProvider);

		public static string G(this FormatStringUtilitiesHandle<DateTimeOffset> hnd, IFormatProvider formatProvider)
			=> hnd.Object.ToString(format: null, formatProvider: formatProvider);

		public static string G(this FormatStringUtilitiesHandle<DateTimeOffset> hnd)
			=> hnd.Object.ToString(format: null, formatProvider: null);

		/// <summary>
		/// Возвращает дату в формате 'yyyy-MM-dd'.
		/// </summary>
		/// <param name="hnd">
		/// Дескриптор утилит <seealso cref="FormatStringUtilities"/>.
		/// </param>
		/// <returns>Объект <seealso cref="string"/>.</returns>
		public static string DateIso8601(this FormatStringUtilitiesHandle<DateTimeOffset> hnd)
			=> hnd.Object.ToString(format: "yyyy-MM-dd", formatProvider: CultureInfo.InvariantCulture);

		/// <summary>
		/// Возвращает дату в формате 'yyyy-MM-ddTHHmmsszzz'.
		/// </summary>
		/// <param name="hnd">
		/// Дескриптор утилит <seealso cref="FormatStringUtilities"/>.
		/// </param>
		/// <returns>Объект <seealso cref="string"/>.</returns>
		public static string DateTimeZoneIso8601(this FormatStringUtilitiesHandle<DateTimeOffset> hnd)
			=> hnd.Object.ToString(format: "yyyy-MM-ddTHHmmsszzz", formatProvider: CultureInfo.InvariantCulture).Replace(":", string.Empty);

		/// <summary>
		/// Возвращает дату в формате 'yyyy-MM-dd'.
		/// </summary>
		/// <param name="hnd">
		/// Дескриптор утилит <seealso cref="FormatStringUtilities"/>.
		/// </param>
		/// <param name="formatProvider">
		/// Поставщик форматов.
		/// </param>
		/// <returns>Объект <seealso cref="string"/>.</returns>
		public static string DateIso8601(this FormatStringUtilitiesHandle<DateTimeOffset?> hnd, IFormatProvider formatProvider)
			=> hnd.Object.HasValue ? hnd.Object.Value.ToString(format: "yyyy-MM-dd", formatProvider: CultureInfo.InvariantCulture) : GetNullValueText(formatProvider: formatProvider);

		public static string DateIso8601(this FormatStringUtilitiesHandle<DateTimeOffset?> hnd)
			=> hnd.Object.HasValue ? hnd.Object.Value.ToString(format: "yyyy-MM-dd", formatProvider: CultureInfo.InvariantCulture) : GetNullValueText(formatProvider: null);

		/// <summary>
		/// Возвращает дату в формате 'yyyy-MM-ddTHHmmsszzz'.
		/// </summary>
		/// <param name="hnd">
		/// Дескриптор утилит <seealso cref="FormatStringUtilities"/>.
		/// </param>
		/// <param name="formatProvider">
		/// Поставщик форматов.
		/// </param>
		/// <returns>Объект <seealso cref="string"/>.</returns>
		public static string DateTimeZoneIso8601(this FormatStringUtilitiesHandle<DateTimeOffset?> hnd, IFormatProvider formatProvider)
			=> hnd.Object.HasValue ? hnd.Object.Value.ToString(format: "yyyy-MM-ddTHHmmsszzz", formatProvider: CultureInfo.InvariantCulture).Replace(":", string.Empty) : GetNullValueText(formatProvider: formatProvider);

		public static string DateTimeZoneIso8601(this FormatStringUtilitiesHandle<DateTimeOffset?> hnd)
			=> hnd.Object.HasValue ? hnd.Object.Value.ToString(format: "yyyy-MM-ddTHHmmsszzz", formatProvider: CultureInfo.InvariantCulture).Replace(":", string.Empty) : GetNullValueText(formatProvider: null);

	}

}