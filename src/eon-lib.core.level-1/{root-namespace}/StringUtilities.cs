using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

using Eon.Text;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {
#pragma warning disable IDE0034 // Simplify 'default' expression

	public static class StringUtilities {

		#region Nested types

		[DataContract]
		public enum TruncateMode {

			[EnumMember]
			Middle = 0,

			[EnumMember]
			End,

			[EnumMember]
			Start

		}

		#endregion

		/// <summary>
		/// Возвращает подстроку после последнего вхождения указанного символа <paramref name="chr"/>.
		/// <para>Если входная строка есть <see langword="null"/>, то метод возвращает либо явно заданное значение <paramref name="defaultStr"/>, либо <paramref name="str"/>.</para>
		/// <para>Если последнее вхождение символа <paramref name="chr"/> соответствует последнему символу входной строки, то метод возвращает <see cref="string.Empty"/>.</para>
		/// <para>Если символ <paramref name="chr"/> отсутствует во входной строке, то метод возвращает либо явно заданное значение <paramref name="defaultStr"/>, либо <paramref name="str"/>.</para>
		/// </summary>
		/// <param name="str">
		/// Входная строка.
		/// <para>Может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="chr">Символ.</param>
		/// <param name="defaultStr">
		/// Строка, возвращаемая в случае, если во входной строке указанный символ <paramref name="chr"/> отсутствует, либо входная строка есть <see langword="null"/>.
		/// </param>
		/// <returns>Объект <see cref="string"/>.</returns>
		public static string SubstringAfterLast(this string str, char chr, ArgumentPlaceholder<string> defaultStr = default) {
			if (str is null)
				return defaultStr.Substitute(str);
			else {
				var index = str.LastIndexOf(chr);
				if (index < 0)
					return defaultStr.Substitute(str);
				else if (index == str.Length - 1)
					return string.Empty;
				else
					return str.Substring(index + 1);
			}
		}

		public static string SubstringBefore(this string str, char chr, bool nullIfCharNotFound = default) {
			if (str is null)
				return null;
			else {
				var index = str.IndexOf(chr);
				if (index < 0)
					return nullIfCharNotFound ? null : str;
				else if (index == 0)
					return string.Empty;
				else
					return str.Substring(0, index);
			}
		}

		public static string SubstringBeforeLast(this string @string, char @char, bool nullIfCharNotFound = default(bool)) {
			if (@string == null)
				return null;
			else {
				var index = @string.LastIndexOf(@char);
				if (index < 0)
					return nullIfCharNotFound ? null : @string;
				else if (index == 0)
					return string.Empty;
				else
					return @string.Substring(0, index);
			}
		}

		public static string RemoveLast(this string str, string value, StringComparison comparison) {
			str.EnsureNotNull(nameof(str));
			value.EnsureNotNull(nameof(value));
			//
			if (str == string.Empty || value == string.Empty)
				return str;
			var lastIndexOf = str.LastIndexOf(value, comparison);
			if (lastIndexOf < 0)
				return str;
			else if (lastIndexOf == 0)
				return string.Empty;
			else
				return value.Substring(0, value.Length - lastIndexOf);
		}

		public static string SubstringExclusive(this string str, int startIndex) {
			str.EnsureNotNull(nameof(str));
			if (startIndex < 0)
				throw new ArgumentOutOfRangeException("startIndex", FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanZero"));
			if (startIndex > str.Length - 1)
				throw new ArgumentOutOfRangeException("startIndex", FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", (str.Length - 1).ToString("d")));
			if (startIndex == str.Length - 1)
				return string.Empty;
			return str.Substring(startIndex + 1);
		}

		public static string SubstringBetweenExclusive(this string str, int leftIndex, int rigthIndex) {
			str.EnsureNotNull(nameof(str));
			if (leftIndex < 0)
				throw new ArgumentOutOfRangeException("leftIndex", FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanZero"));
			if (rigthIndex < 0)
				throw new ArgumentOutOfRangeException("rigthIndex", FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThanZero"));
			if (leftIndex > rigthIndex)
				throw new ArgumentOutOfRangeException("rigthIndex", FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotLessThan/OtherArg", leftIndex.ToString("d"), "leftIndex"));
			if (leftIndex > str.Length - 1)
				throw new ArgumentOutOfRangeException("leftIndex", FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", (str.Length - 1).ToString("d")));
			if (rigthIndex > str.Length - 1)
				throw new ArgumentOutOfRangeException("rigthIndex", FormatXResource(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", (str.Length - 1).ToString("d")));
			var length = rigthIndex - leftIndex - 1;
			if (length < 1)
				return string.Empty;
			return str.Substring(leftIndex + 1, length);
		}

		public static string Left(this string str, int length) {
			str.EnsureNotNull(nameof(str));
			length
				.Arg(nameof(length))
				.EnsureNotLessThanZero()
				.EnsureNotGreaterThan(str.Length);
			//
			return
				length == 0
				? string.Empty
				: str.Substring(0, length);
		}

		public static string Right(this string str, int length) {
			str.EnsureNotNull(nameof(str));
			length
				.EnsureNotLessThanZero(nameof(length))
				.EnsureNotGreaterThan(str.Length);
			//
			return length == 0 ? string.Empty : str.Substring(str.Length - length, length);
		}

		public static string GetEqualSubstring(this string str1, string str2) {
			if (str1 == null || str2 == null)
				return null;
			var minLength = Math.Min(str1.Length, str2.Length);
			var lastEqualCharIndex = -1;
			for (var i = 0; i < minLength; i++) {
				if (str1[ i ] == str2[ i ])
					lastEqualCharIndex = i;
				else
					break;
			}
			if (lastEqualCharIndex < 0)
				return string.Empty;
			else
				return str1.Substring(0, lastEqualCharIndex + 1);
		}

		public static string ConcatNotEmptyStrings(this IEnumerable<string> strs, char separator)
			=> ConcatStrings(strs, new string(separator, 1), true);

		public static string ConcatNotEmptyStrings(this IEnumerable<string> strs, string separator)
			=> ConcatStrings(strs, separator, true);

		public static string ConcatNotEmptyStrings(string separator, params string[ ] strs)
			=> ConcatStrings(strs.IsNullOrEmpty() ? null : strs, separator, skipEmptyStrings: true);

		public static string ConcatStrings(this IEnumerable<string> strs, string separator)
			=> ConcatStrings(strs, separator, false);

		public static string ConcatStrings(this IEnumerable<string> strs, string separator, bool skipEmptyStrings) {
			if (strs is null)
				return null;
			else
				using (var buffer = StringBuilderUtilities.AcquireBuffer()) {
					var sb = buffer.StringBuilder;
					if (skipEmptyStrings)
						strs = strs.Where(locString => !string.IsNullOrEmpty(locString));
					var notFirstString = false;
					foreach (var @string in strs) {
						if (notFirstString)
							sb.Append(separator);
						else
							notFirstString = true;
						sb.Append(@string);
					}
					//
					return sb.ToString();
				}
		}

		/// <summary>
		/// Вставляет отступ в начале каждой новой строки текста <paramref name="inputText"/>.
		/// <para>Если значение <paramref name="inputText"/> равно null или <see cref="string.Empty"/>, то метод возвратит значение <paramref name="inputText"/>.</para>
		/// </summary>
		/// <param name="inputText">Входной текст.</param>
		/// <param name="lineSeparator">
		/// Строка, определяющая разделитель строк в <paramref name="inputText"/>.
		/// <para>Если значение не задано или равно null, то будет использоваться значение <see cref="Environment.NewLine"/>.</para>
		/// </param>
		/// <param name="indent">
		/// Строка, определяющая отступ в начале каждой новой строки <paramref name="inputText"/>.
		/// <para>Если значение не задано или равно null, то будет использоваться символ табуляции (\u0009).</para>
		/// </param>
		/// <param name="indentSize">
		/// Размер отступа.
		/// <para>Если не задан или равен null, то будет использоваться значение 1.</para>
		/// </param>
		/// <param name="inputTextSplitOptions">
		/// Опции разбиения входного текста <paramref name="inputText"/> на строки.
		/// <para>Если не задан или равен null, то будет использоваться значение <see cref="StringSplitOptions.None"/>.</para>
		/// </param>
		/// <param name="skipFirstLine">
		/// Указывает, оставить ли первую строку текста <paramref name="inputText"/> без отступа.
		/// </param>
		/// <returns>
		/// Объект <see cref="string"/>.
		/// </returns>
		public static string IndentLines(
			this string inputText,
			string lineSeparator = null,
			string indent = null,
			int? indentSize = null,
			StringSplitOptions? inputTextSplitOptions = null,
			bool skipFirstLine = false) {
			//
			indentSize
				.Arg(nameof(indentSize))
				.EnsureNotLessThanZero();
			//
			if (string.IsNullOrEmpty(inputText) || indent == string.Empty || indentSize < 1)
				return inputText;
			lineSeparator = lineSeparator ?? Environment.NewLine;
			indent = indent ?? new string('\u0009', 1);
			inputTextSplitOptions = inputTextSplitOptions ?? StringSplitOptions.None;
			indentSize = indentSize ?? 1;
			if (indentSize > 1) {
				if (indent.Length == 1)
					indent = new string(c: indent[ 0 ], count: indentSize.Value);
				else {
					using (var indentBuffer = StringBuilderUtilities.AcquireBuffer()) {
						var indentBuilder = indentBuffer.StringBuilder;
						for (var i = 0; i < indentSize; i++)
							indentBuilder.Append(indent);
						indent = indentBuilder.ToString();
					}
				}
			}
			//
			using (var outputBuffer = StringBuilderUtilities.AcquireBuffer()) {
				var outputBuilder = outputBuffer.StringBuilder;
				var lines = inputText.Split(separator: new string[ ] { lineSeparator }, options: inputTextSplitOptions.Value);
				//
				if (lines.Length > 0) {
					if (!skipFirstLine)
						outputBuilder.Append(indent);
					outputBuilder.Append(lines[ 0 ]);
					//
					for (var i = 1; i < lines.Length; i++) {
						outputBuilder.Append(lineSeparator);
						outputBuilder.Append(indent);
						outputBuilder.Append(lines[ i ]);
					}
				}
				//
				return outputBuilder.ToString();
			}
		}

		/// <summary>
		/// Вставляет отступ в начале каждой новой строки текста <paramref name="inputText"/>.
		/// <para>Размер отступа равен 2.</para>
		/// <para>Если значение <paramref name="inputText"/> равно null или <see cref="string.Empty"/>, то метод возвратит значение <paramref name="inputText"/>.</para>
		/// </summary>
		/// <param name="inputText">Входной текст.</param>
		/// <param name="lineSeparator">
		/// Строка, определяющая разделитель строк в <paramref name="inputText"/>.
		/// <para>Если значение не задано или равно null, то будет использоваться значение <see cref="Environment.NewLine"/>.</para>
		/// </param>
		/// <param name="indent">
		/// Строка, определяющая отступ в начале каждой новой строки <paramref name="inputText"/>.
		/// <para>Если значение не задано или равно null, то будет использоваться символ табуляции (\u0009).</para>
		/// </param>
		/// <param name="inputTextSplitOptions">
		/// Опции разбиения входного текста <paramref name="inputText"/> на строки.
		/// <para>Если не задан или равен null, то будет использоваться значение <see cref="StringSplitOptions.RemoveEmptyEntries"/>.</para>
		/// </param>
		/// <returns>
		/// Объект <see cref="string"/>.
		/// </returns>
		public static string IndentLines2(this string inputText, string lineSeparator = null, string indent = null, StringSplitOptions? inputTextSplitOptions = null)
			=> IndentLines(inputText: inputText, lineSeparator: lineSeparator, indent: indent, indentSize: 2, inputTextSplitOptions: inputTextSplitOptions);

		/// <summary>
		/// Вставляет отступ в начале каждой новой строки текста <paramref name="inputText"/>.
		/// <para>Размер отступа равен 3.</para>
		/// <para>Если значение <paramref name="inputText"/> равно null или <see cref="string.Empty"/>, то метод возвратит значение <paramref name="inputText"/>.</para>
		/// </summary>
		/// <param name="inputText">Входной текст.</param>
		/// <param name="lineSeparator">
		/// Строка, определяющая разделитель строк в <paramref name="inputText"/>.
		/// <para>Если значение не задано или равно null, то будет использоваться значение <see cref="Environment.NewLine"/>.</para>
		/// </param>
		/// <param name="indent">
		/// Строка, определяющая отступ в начале каждой новой строки <paramref name="inputText"/>.
		/// <para>Если значение не задано или равно null, то будет использоваться символ табуляции (\u0009).</para>
		/// </param>
		/// <param name="inputTextSplitOptions">
		/// Опции разбиения входного текста <paramref name="inputText"/> на строки.
		/// <para>Если не задан или равен null, то будет использоваться значение <see cref="StringSplitOptions.RemoveEmptyEntries"/>.</para>
		/// </param>
		/// <returns>
		/// Объект <see cref="string"/>.
		/// </returns>
		public static string IndentLines3(this string inputText, string lineSeparator = null, string indent = null, StringSplitOptions? inputTextSplitOptions = null)
			=> IndentLines(inputText: inputText, lineSeparator: lineSeparator, indent: indent, indentSize: 3, inputTextSplitOptions: inputTextSplitOptions);

		public static IFormattable ToFormattableString(this object value, bool propagateNull)
			=> Equals(objA: value, objB: null) ? (propagateNull ? null : new Globalization.FormattableString(format: FormatStringUtilities.GetNullValueText(), args: null)) : new Globalization.FormattableString("{0}", value);

		public static IFormattable ToFormattableString(this object value)
			=> Equals(objA: value, objB: null) ? new Globalization.FormattableString(format: FormatStringUtilities.GetNullValueText(), args: null) : new Globalization.FormattableString("{0}", value);

		public static IFormattable ToFormattableString<T>(this T value, bool propagateNull)
			where T : class
			=> value is null ? (propagateNull ? null : new Globalization.FormattableString(format: FormatStringUtilities.GetNullValueText(), args: null)) : new Globalization.FormattableString("{0}", value);

		public static IFormattable ToFormattableString<T>(this T value)
			where T : class
			=> value is null ? new Globalization.FormattableString(format: FormatStringUtilities.GetNullValueText(), args: null) : new Globalization.FormattableString("{0}", value);

		public static IFormattable ToFormattableString<T>(this T? value, bool propagateNull)
			where T : struct
			=> !value.HasValue ? (propagateNull ? null : new Globalization.FormattableString(format: FormatStringUtilities.GetNullValueText(), args: null)) : new Globalization.FormattableString("{0}", value);

		public static IFormattable ToFormattableString<T>(this T? value)
			where T : struct
			=> value.HasValue ? new Globalization.FormattableString("{0}", value) : new Globalization.FormattableString(format: FormatStringUtilities.GetNullValueText(), args: null);

		public static IFormattable ToFormattableString(string separator, params IFormattable[ ] values)
			=> ToFormattableString(strs: values, separator: separator);

		public static IFormattable ToFormattableString(this IEnumerable<IFormattable> strs, string separator) {
			if (strs is null)
				return null;
			else {
				string format;
				var args = new List<object>();
				using (var buffer = StringBuilderUtilities.AcquireBuffer()) {
					var stringBuilder = buffer.StringBuilder;
					foreach (var @string in strs) {
						if (@string == null)
							continue;
						else if (stringBuilder.Length > 0)
							stringBuilder.Append(separator);
						stringBuilder.Append("{" + args.Count.ToString("d", CultureInfo.InvariantCulture) + "}");
						args.Add(@string);
					}
					format = stringBuilder.ToString();
				}
				if (format == string.Empty)
					return new Globalization.FormattableString();
				else
					return new Globalization.FormattableString(format: format, args: args.ToArray());
			}
		}

		public static bool HasLeadingWhiteSpace(this string @string) {
			if (string.IsNullOrEmpty(@string))
				return false;
			else
				return char.IsWhiteSpace(@string, 0);
		}

		public static bool HasTrailingWhiteSpace(this string @string) {
			if (string.IsNullOrEmpty(@string))
				return false;
			else
				return char.IsWhiteSpace(@string, @string.Length - 1);
		}

		/// <summary>
		/// Выполняет проверку эквивалентности двух строк, используя правила, соответствующие для <seealso cref="StringComparison.OrdinalIgnoreCase"/>.
		/// </summary>
		/// <param name="string">Первая строка.</param>
		/// <param name="otherString">Вторая строка.</param>
		/// <returns>Значение <seealso cref="bool"/>.</returns>
		public static bool EqualsOrdinalCI(this string @string, string otherString)
			=> string.Equals(@string, otherString, StringComparison.OrdinalIgnoreCase);

		/// <summary>
		/// Выполняет проверку эквивалентности двух строк, используя правила, соответствующие для <seealso cref="StringComparison.CurrentCulture"/>.
		/// </summary>
		/// <param name="string">Первая строка.</param>
		/// <param name="otherString">Вторая строка.</param>
		/// <returns>Значение <seealso cref="bool"/>.</returns>
		public static bool EqualsCultureCS(this string @string, string otherString)
			=> string.Equals(@string, otherString, StringComparison.CurrentCulture);

		/// <summary>
		/// Выполняет проверку эквивалентности двух строк, используя правила, соответствующие для <seealso cref="StringComparison.CurrentCultureIgnoreCase"/>.
		/// </summary>
		/// <param name="string">Первая строка.</param>
		/// <param name="otherString">Вторая строка.</param>
		/// <returns>Значение <seealso cref="bool"/>.</returns>
		public static bool EqualsCultureCI(this string @string, string otherString)
			=> string.Equals(@string, otherString, StringComparison.CurrentCultureIgnoreCase);

		/// <summary>
		/// Выполняет проверку эквивалентности двух строк, используя правила, соответствующие для <seealso cref="StringComparison.Ordinal"/>.
		/// </summary>
		/// <param name="string">Первая строка.</param>
		/// <param name="otherString">Вторая строка.</param>
		/// <returns>Значение <seealso cref="bool"/>.</returns>
		public static bool EqualsOrdinalCS(this string @string, string otherString)
			=> string.Equals(@string, otherString, StringComparison.Ordinal);

		public static bool StringEquals(this string @string, string otherString, StringComparer comparer)
			=>
			comparer
			.EnsureNotNull(nameof(comparer))
			.Value
			.Equals(x: @string, y: otherString);

		public static bool TryParseInvariant(this string value, out long? result)
			=> NumberUtilities.TryParseInvariant(value, out result);

		public static bool TryParse(this string value, out long? result)
			=> NumberUtilities.TryParse(value, out result);

		public static bool TryParse(this string value, IFormatProvider formatProvider, out long? result)
			=> NumberUtilities.TryParse(value, formatProvider, out result);

		public static bool TryParseInvariant(this string value, out int? result)
			=> NumberUtilities.TryParseInvariant(value, out result);

		public static bool TryParse(this string value, out int? result)
			=> NumberUtilities.TryParse(value, out result);

		public static bool TryParse(this string value, IFormatProvider formatProvider, out int? result)
			=> NumberUtilities.TryParse(value, formatProvider, out result);

		// TODO: Put strings into the resources.
		//	
		public static string Truncate(this string original, int maxTruncatedLength, string ellipsis, TruncateMode mode) {
			maxTruncatedLength.EnsureNotLessThanZero(nameof(maxTruncatedLength));
			ellipsis.EnsureNotNull(nameof(ellipsis));
			if (maxTruncatedLength < ellipsis.Length)
				throw
					new ArgumentException(
						message: $"{FormatXResource(typeof(string), "TooLong")} Длина строки не может быть больше значения параметра '{nameof(maxTruncatedLength)}'.",
						paramName: nameof(ellipsis));
			if (!(mode == TruncateMode.End || mode == TruncateMode.Middle || mode == TruncateMode.Start))
				throw
					new ArgumentOutOfRangeException(
						paramName: nameof(mode),
						message: $"Значение '{mode}' не поддерживается.");
			//
			if (string.IsNullOrEmpty(original))
				return original;
			else if (maxTruncatedLength == 0)
				return string.Empty;
			else if (original.Length <= maxTruncatedLength)
				return original;
			else if (maxTruncatedLength == ellipsis.Length)
				return original.Length > ellipsis.Length ? ellipsis : original;
			else if (mode == TruncateMode.Middle) {
				var stringValueBeginLength = (maxTruncatedLength - ellipsis.Length) / 2;
				var stringValueEndLength = (maxTruncatedLength - ellipsis.Length) - stringValueBeginLength;
				return
					original.Substring(0, stringValueBeginLength)
					+ ellipsis
					+ original.Substring(original.Length - stringValueEndLength, stringValueEndLength);
			}
			else if (mode == TruncateMode.Start)
				return ellipsis + original.Right(maxTruncatedLength - ellipsis.Length);
			else
				return original.Left(maxTruncatedLength - ellipsis.Length) + ellipsis;
		}

		public static string Truncate(this string original, int maxTruncatedLength, TruncateMode mode)
			=> Truncate(original: original, maxTruncatedLength: maxTruncatedLength, ellipsis: FormatStringUtilities.TruncateEllipsis, mode: mode);

		public static string Truncate(this string original, int maxTruncatedLength)
			=> Truncate(original: original, maxTruncatedLength: maxTruncatedLength, ellipsis: FormatStringUtilities.TruncateEllipsis, mode: TruncateMode.Middle);


		public static string Truncate(this string original, float maxOriginalLengthPercent, string ellipsis, TruncateMode mode) {
			maxOriginalLengthPercent
				.EnsureNumber(nameof(maxOriginalLengthPercent))
				.EnsureNotLessThan(.0F)
				.EnsureNotGreaterThan(1.0F);
			ellipsis.EnsureNotNull(nameof(ellipsis));
			//
			if (string.IsNullOrEmpty(original) || maxOriginalLengthPercent == 1.0F)
				return original;
			else {
				int maxOriginalLength;
				checked {
					maxOriginalLength = (int)Math.Ceiling(original.Length * maxOriginalLengthPercent);
				}
				if (maxOriginalLength < original.Length) {
					checked {
						return Truncate(original, maxOriginalLength + ellipsis.Length, ellipsis, mode);
					}
				}
				else
					return original;
			}
		}

		public static string Truncate(this string original, float maxOriginalLengthPercent, TruncateMode mode)
			=> Truncate(original: original, maxOriginalLengthPercent: maxOriginalLengthPercent, ellipsis: FormatStringUtilities.TruncateEllipsis, mode: mode);

		public static string TrimEndSingle(this string value, char trimChar) {
			if (!string.IsNullOrEmpty(value) && value[ value.Length - 1 ] == trimChar)
				return value.Length == 1 ? string.Empty : value.Substring(0, value.Length - 1);
			else
				return value;
		}

		public static string TrimEndSingle(this string value, char trimChar1, char trimChar2) {
			if (string.IsNullOrEmpty(value))
				return value;
			else {
				var endChar = value[ value.Length - 1 ];
				if (endChar == trimChar1 || endChar == trimChar2)
					return value.Length == 1 ? string.Empty : value.Substring(startIndex: 0, length: value.Length - 1);
				else
					return value;
			}
		}

		public static string TrimStartSingle(this string value, char trimChar) {
			if (!string.IsNullOrEmpty(value) && value[ 0 ] == trimChar)
				return value.Length == 1 ? string.Empty : value.Substring(1);
			else
				return value;
		}

		public static string TrimStartSingle(this string value, char trimChar1, char trimChar2) {
			if (string.IsNullOrEmpty(value))
				return value;
			else {
				var startChar = value[ 0 ];
				if (startChar == trimChar1 || startChar == trimChar2)
					return value.Length == 1 ? string.Empty : value.Substring(startIndex: 1);
				else
					return value;
			}
		}

		public static string TrimStart(this string str, int maxCount, char ch1)
			=> TrimStart(str: str, maxCount: maxCount, chars: new char[ ] { ch1 });

		public static string TrimStart(this string str, int maxCount, char ch1, char ch2)
			=> TrimStart(str: str, maxCount: maxCount, chars: new char[ ] { ch1, ch2 });

		public static string TrimStart(this string str, int maxCount, char ch1, char ch2, char ch3)
			=> TrimStart(str: str, maxCount: maxCount, chars: new char[ ] { ch1, ch2, ch3 });

		public static string TrimStart(this string str, int maxCount, char ch1, char ch2, char ch3, char ch4)
			=> TrimStart(str: str, maxCount: maxCount, chars: new char[ ] { ch1, ch2, ch3, ch4 });

		public static string TrimStart(this string str, int maxCount, char[ ] chars) {
			maxCount.Arg(nameof(maxCount)).EnsureNotLessThanZero();
			chars.EnsureNotNull(nameof(chars)).EnsureHasMaxLength(maxLength: 65536);
			//
			if (maxCount == 0 || chars.Length == 0 || string.IsNullOrEmpty(str))
				return str;
			else {
				var charsCopy = new char[ chars.Length ];
				Buffer.BlockCopy(src: chars, srcOffset: 0, dst: charsCopy, dstOffset: 0, count: Buffer.ByteLength(array: charsCopy));
				Array.Sort(array: charsCopy);
				var resultStrOffset = 0;
				var strLength = str.Length;
				for (var i = 0; i < strLength && resultStrOffset < maxCount; i++) {
					if (Array.BinarySearch(array: charsCopy, value: str[ i ]) > -1)
						resultStrOffset++;
					else
						break;
				}
				return str.Substring(startIndex: resultStrOffset);
			}
		}

		public static string NullIfEmptyString(this string @string)
			=> string.IsNullOrEmpty(@string) ? null : @string;

		public static string RepeatString(this string @string, int count) {
			count
				.Arg(nameof(count))
				.EnsureNotLessThanZero();
			//
			if (string.IsNullOrEmpty(@string) || count < 1)
				return @string;
			else
				using (var acquiredBuffer = StringBuilderUtilities.AcquireBuffer()) {
					var sb = acquiredBuffer.StringBuilder;
					sb.Append(@string);
					for (var i = 0; i < count; i++)
						sb.Append(@string);
					return sb.ToString();
				}
		}

		public static string RemoveHyphens(string value) {
			if (string.IsNullOrWhiteSpace(value))
				return value;
			else {
				using (var acquiredBuffer = StringBuilderUtilities.AcquireBuffer()) {
					var sb = acquiredBuffer.StringBuilder;
					//
					var previousChar = '-';
					for (var i = 0; i < value.Length; i++) {
						var currentChar = value[ i ];
						if (currentChar != '-') {
							if (previousChar == '-') {
								if (i + 1 < value.Length && char.IsUpper(value[ i + 1 ]))
									sb.Append(currentChar);
								else
									sb.Append(char.ToUpper(currentChar));
							}
							else
								sb.Append(currentChar);
						}
						//
						previousChar = currentChar;
					}
					//
					return sb.ToString();
				}
			}
		}

		public static char? LastChar(this string @string) {
			if (string.IsNullOrEmpty(@string))
				return default(char?);
			else
				return @string[ @string.Length - 1 ];
		}

	}

#pragma warning restore IDE0034 // Simplify 'default' expression
}