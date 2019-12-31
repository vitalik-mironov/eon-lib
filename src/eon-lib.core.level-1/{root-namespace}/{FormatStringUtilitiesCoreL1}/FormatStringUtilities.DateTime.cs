using System;
using System.Globalization;

namespace Eon {

	public static partial class FormatStringUtilities {

		public static string GShort(this FormatStringUtilitiesHandle<DateTime?> obj)
			=>
			GShort(
				obj: obj,
				formatProvider: null);

		public static string GShort(this FormatStringUtilitiesHandle<DateTime?> obj, IFormatProvider formatProvider)
			=>
			obj.Object.HasValue
			? $"{obj.Object.Value.ToString(format: "g", provider: formatProvider)} ({obj.Object.Value.ToString(format: "zzz", provider: formatProvider)})"
			: FormatStringUtilities.GetNullValueText(formatProvider: formatProvider);

		public static string GShort(this FormatStringUtilitiesHandle<DateTime> obj)
			=> GShort(obj: obj, formatProvider: null);

		public static string GShort(this FormatStringUtilitiesHandle<DateTime> obj, IFormatProvider formatProvider)
			=> $"{obj.Object.ToString(format: "g", provider: formatProvider)} ({obj.Object.ToString(format: "zzz", provider: formatProvider)})";

		public static string G(this FormatStringUtilitiesHandle<DateTime?> obj, IFormatProvider formatProvider = null)
			=>
			obj.Object.HasValue
			? obj.Object.Value.ToString(format: null, provider: formatProvider)
			: FormatStringUtilities.GetNullValueText(formatProvider: formatProvider);

		public static string RoundTrip(this FormatStringUtilitiesHandle<DateTime?> obj, IFormatProvider formatProvider)
			=>
			obj.Object.HasValue
			? obj.Object.Value.ToString(format: "o", provider: formatProvider)
			: FormatStringUtilities.GetNullValueText(formatProvider: formatProvider);

		public static string RoundTrip(this FormatStringUtilitiesHandle<DateTime?> obj)
			=> obj.Object.HasValue ? obj.Object.Value.ToString(format: "o", provider: null) : GetNullValueText(formatProvider: null);

		public static string G(this FormatStringUtilitiesHandle<DateTime> hnd, IFormatProvider formatProvider)
			=> hnd.Object.ToString(format: null, provider: formatProvider);

		public static string G(this FormatStringUtilitiesHandle<DateTime> hnd)
			=> hnd.Object.ToString(format: null, provider: null);

		/// <summary>
		/// Возвращает дату в формате 'yyyy-MM-dd'.
		/// </summary>
		/// <param name="hnd">
		/// Дескриптор утилит <seealso cref="FormatStringUtilities"/>.
		/// </param>
		/// <returns>Объект <seealso cref="string"/>.</returns>
		public static string DateIso8601(this FormatStringUtilitiesHandle<DateTime> hnd)
			=> hnd.Object.ToString(format: "yyyy-MM-dd", provider: CultureInfo.InvariantCulture);

		/// <summary>
		/// Возвращает дату в формате 'yyyy-MM-ddTHHmmsszzz'.
		/// </summary>
		/// <param name="hnd">
		/// Дескриптор утилит <seealso cref="FormatStringUtilities"/>.
		/// </param>
		/// <returns>Объект <seealso cref="string"/>.</returns>
		public static string DateTimeZoneIso8601(this FormatStringUtilitiesHandle<DateTime> hnd)
			=> hnd.Object.ToString(format: "yyyy-MM-ddTHHmmsszzz", provider: CultureInfo.InvariantCulture).Replace(":", string.Empty);

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
		public static string DateIso8601(this FormatStringUtilitiesHandle<DateTime?> hnd, IFormatProvider formatProvider)
			=> hnd.Object.HasValue ? hnd.Object.Value.ToString(format: "yyyy-MM-dd", provider: CultureInfo.InvariantCulture) : GetNullValueText(formatProvider: formatProvider);

		public static string DateIso8601(this FormatStringUtilitiesHandle<DateTime?> hnd)
			=> hnd.Object.HasValue ? hnd.Object.Value.ToString(format: "yyyy-MM-dd", provider: CultureInfo.InvariantCulture) : GetNullValueText(formatProvider: null);

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
		public static string DateTimeZoneIso8601(this FormatStringUtilitiesHandle<DateTime?> hnd, IFormatProvider formatProvider)
			=> hnd.Object.HasValue ? hnd.Object.Value.ToString(format: "yyyy-MM-ddTHHmmsszzz", provider: CultureInfo.InvariantCulture).Replace(":", string.Empty) : GetNullValueText(formatProvider: formatProvider);

		public static string DateTimeZoneIso8601(this FormatStringUtilitiesHandle<DateTime?> hnd)
			=> hnd.Object.HasValue ? hnd.Object.Value.ToString(format: "yyyy-MM-ddTHHmmsszzz", provider: CultureInfo.InvariantCulture).Replace(":", string.Empty) : FormatStringUtilities.GetNullValueText(formatProvider: null);

	}

}