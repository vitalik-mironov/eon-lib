using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using DigitalFlare.Text;
using rt = DigitalFlare.Resources.XResource.XResourceTreeManager;

namespace DigitalFlare.Data.Csv {

	/// <summary>
	/// Утилиты для работы со строками CSV.
	/// </summary>
	//[DebuggerNonUserCode]
	//[DebuggerStepThrough]
	public static class CsvUtilities {

		#region Nested types

		//[DebuggerNonUserCode]
		//[DebuggerStepThrough]
		class P_CsvTextLine {

			readonly string _value;

			readonly int _length;

			internal P_CsvTextLine(string textLine) {
				if (string.IsNullOrWhiteSpace(textLine))
					throw new ArgumentException(rt.Format(typeof(string), "CanNotNullOrWhiteSpace"), "textLine");
				_value = textLine;
				_length = textLine.Length;
			}

			public int Length { get { return _length + 1; } }

			public char this[int charIndex] {
				get {
					if (charIndex < 0)
						throw new ArgumentOutOfRangeException("charIndex", rt.Format(typeof(ArgumentOutOfRangeException), "CanNotLessThanZero"));
					if (charIndex < _length)
						return _value[charIndex];
					if (charIndex == _length)
						return CsvFieldDefaultSeparator;
					throw new ArgumentOutOfRangeException("charIndex", rt.Format(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", _length.TextViewInvariant("d")));
				}
			}

			public bool NextAfter(int charIndex, out char @char) {
				if (charIndex < 0)
					throw new ArgumentOutOfRangeException("charIndex", rt.Format(typeof(ArgumentOutOfRangeException), "CanNotLessThanZero"));
				if (charIndex > _length)
					throw new ArgumentOutOfRangeException("charIndex", rt.Format(typeof(ArgumentOutOfRangeException), "CanNotGreaterThan", _length.TextViewInvariant("d")));
				if (charIndex < _length) {
					@char = this[charIndex + 1];
					return true;
				}
				@char = default(char);
				return false;
			}

		}

		#endregion

		/// <summary>
		/// Символ-разделитель полей (колонок) в CSV-строке, используемый по умолчанию. Значение: &#59; (U+003B).
		/// </summary>
		public static readonly char CsvFieldDefaultSeparator = ';';

		/// <summary>
		/// Символ, используемый для обозначения значений полей (колонок), содержащих зарезервированные CSV-символы. Значение: &#34; (U+0022).
		/// </summary>
		public static readonly char CsvDefaultQuotationChar = '"';

		static readonly char[] __CsvFormatChars = new char[] { ';', '"' };

		static CsvUtilities() {
			var newLine = Environment.NewLine ?? string.Empty;
			__CsvFormatChars = new char[newLine.Length + 2];
			var j = -1;
			for (var k = 0; k < newLine.Length; k++) {
				__CsvFormatChars[++j] = newLine[k];
			}
			__CsvFormatChars[++j] = ';';
			__CsvFormatChars[++j] = '"';
		}

		public static string Format(params string[] values) {
			using (var acquiredBuffer = StringBuilderUtilities.AcquireBuffer()) {
				var stringBuilder = acquiredBuffer.StringBuilder;
				AppendTo(stringBuilder, values);
				return stringBuilder.ToString();
			}
		}

		public static string FormatLine(params string[] values) {
			using (var acquiredBuffer = StringBuilderUtilities.AcquireBuffer()) {
				var stringBuilder = acquiredBuffer.StringBuilder;
				AppendLineTo(stringBuilder, values);
				return stringBuilder.ToString();
			}
		}

		public static void AppendLineTo(StringBuilder stringBuilder, params string[] values) {
			if (stringBuilder == null)
				throw new ArgumentNullException("stringBuilder");
			stringBuilder.Append(Environment.NewLine);
			AppendTo(stringBuilder, values);
		}

		public static void AppendTo(StringBuilder stringBuilder, params string[] values) {
			if (stringBuilder == null)
				throw new ArgumentNullException("stringBuilder");
			if (values == null)
				throw new ArgumentNullException("values");
			var valuesLength = values.Length;
			if (valuesLength < 1)
				return;
			var valuesEndIndex = valuesLength - 1;
			var value = default(string);
			for (int i = 0; i < valuesLength; i++) {
				value = values[i];
				if (!string.IsNullOrEmpty(value)) {
					if (P_IsCsvFieldValueQuotationNeeded(value)) {
						stringBuilder.Append(CsvDefaultQuotationChar);
						for (var j = 0; j < value.Length; j++) {
							stringBuilder.Append(value[j]);
							if (value[j] == CsvDefaultQuotationChar)
								stringBuilder.Append(CsvDefaultQuotationChar);
						}
						stringBuilder.Append(CsvDefaultQuotationChar);
					} else
						stringBuilder.Append(value);
				}
				if (i < valuesEndIndex)
					stringBuilder.Append(CsvFieldDefaultSeparator);
			}
		}

		public static void AppendFieldSeparator(StringBuilder stringBuilder) {
			stringBuilder.EnsureNotNull("stringBuilder");
			stringBuilder.Append(CsvFieldDefaultSeparator);
		}

		static bool P_IsCsvFieldValueQuotationNeeded(string value) {
			if (string.IsNullOrEmpty(value))
				return false;
			if (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[value.Length - 1]))
				return true;
			for (var i = 0; i < value.Length; i++) {
				for (var j = 0; j < __CsvFormatChars.Length; j++)
					if (value[i] == __CsvFormatChars[j])
						return true;
			}
			return false;
		}

		public static string[] SplitLine(string textLine) {
			var buffer = new List<string>();
			SplitLine(textLine, 1, buffer);
			return buffer.ToArray();
		}

		// TODO: Put exception messages into the resources.
		// TODO: Заменить тип параметра buffer на string[].
		//
		public static int SplitLine(string textLine, int textLineNumber, List<string> buffer) {
			if (string.IsNullOrWhiteSpace(textLine))
				throw new ArgumentException(rt.Format(typeof(string), "CanNotNullOrWhiteSpace"), "textLine");
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			var splitCount = 0;
			using (var acquiredStringBuilder = StringBuilderUtilities.AcquireBuffer()) {
				char currentChar, nextChar;
				var valueBuffer = acquiredStringBuilder.StringBuilder;
				var isQuoteOpened = false;
				var csvTextLine = new P_CsvTextLine(textLine);
				var newLine = Environment.NewLine ?? string.Empty;
				for (var charIndex = 0; charIndex < csvTextLine.Length; charIndex++) {
					currentChar = csvTextLine[charIndex];
					if (newLine.IndexOf(currentChar) > -1)
						throw new DigitalFlareApplicationException(string.Format("Формат строки данных (номер '{0}') не соответствует формату CSV. В строке данных обнаружены символы переноса строки текста. Данные строки: '{1}'.", textLineNumber.TextViewInvariant("d"), textLine.TextView()));
					if (currentChar == CsvDefaultQuotationChar) {
						if (isQuoteOpened) {
							if (!csvTextLine.NextAfter(charIndex, out nextChar))
								throw new DigitalFlareApplicationException(string.Format("Формат строки данных (номер '{0}') не соответствует формату CSV. Данные строки: '{1}'.", textLineNumber.TextViewInvariant("d"), textLine.TextView()));
							else if (nextChar == CsvFieldDefaultSeparator)
								isQuoteOpened = false;
							else if (nextChar == CsvDefaultQuotationChar) {
								valueBuffer.Append(CsvDefaultQuotationChar);
								charIndex++;
							} else
								throw new DigitalFlareApplicationException(string.Format("Формат строки данных (номер '{0}') не соответствует формату CSV. Данные строки: '{1}'.", textLineNumber.TextViewInvariant("d"), textLine.TextView()));
						} else if (valueBuffer.Length > 0)
							throw new DigitalFlareApplicationException(string.Format("Формат строки данных (номер '{0}') не соответствует формату CSV. Данные строки: '{1}'.", textLineNumber.TextViewInvariant("d"), textLine.TextView()));
						else
							isQuoteOpened = true;
					} else if (currentChar == CsvFieldDefaultSeparator) {
						if (isQuoteOpened)
							valueBuffer.Append(CsvFieldDefaultSeparator);
						else {
							buffer.Add(valueBuffer.ToString());
							valueBuffer.Length = 0;
							splitCount++;
						}
					} else
						valueBuffer.Append(currentChar);
				}
				if (isQuoteOpened || valueBuffer.Length > 0)
					throw new DigitalFlareApplicationException(string.Format("Формат строки данных (номер '{0}') не соответствует формату CSV. Данные строки: '{1}'.", textLineNumber.TextViewInvariant("d"), textLine.TextView()));
			}
			return splitCount;
		}

		public static void WriteCsvHeader(this TextWriter writer, string columnHeader, params string[] othersColumnsHeaders) {
			if (writer == null)
				throw new ArgumentNullException("writer");
			string[] columnsHeaders;
			if (othersColumnsHeaders.IsNullOrEmpty())
				columnsHeaders = new string[] { columnHeader };
			else {
				columnsHeaders = new string[1 + othersColumnsHeaders.Length];
				columnsHeaders[0] = columnHeader;
				Array.Copy(othersColumnsHeaders, 0, columnsHeaders, 1, othersColumnsHeaders.Length);
			}
			using (var acquiredBuffer = StringBuilderUtilities.AcquireBuffer()) {
				var sb = acquiredBuffer.StringBuilder;
				AppendTo(sb, columnsHeaders);
				writer.Write(sb);
			}
		}

		public static void WriteCsvLine(this TextWriter writer, string value, params string[] othersValues) {
			if (writer == null)
				throw new ArgumentNullException("writer");
			string[] values;
			if (othersValues.IsNullOrEmpty())
				values = new string[] { value };
			else {
				values = new string[1 + othersValues.Length];
				values[0] = value;
				Array.Copy(othersValues, 0, values, 1, othersValues.Length);
			}
			using (var acquiredBuffer = StringBuilderUtilities.AcquireBuffer()) {
				var sb = acquiredBuffer.StringBuilder;
				AppendTo(sb, values);
				writer.Write(Environment.NewLine);
				writer.Write(sb);
			}
		}

	}

}