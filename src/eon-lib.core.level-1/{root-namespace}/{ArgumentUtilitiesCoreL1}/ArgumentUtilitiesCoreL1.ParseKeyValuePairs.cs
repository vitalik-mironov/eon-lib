using System;
using System.Collections.Generic;
using System.Text;
using Eon.Text;
using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		/// <summary>
		/// Символ-разделитель ключа и значения, используемый по умолчанию в методе(-ах) <seealso cref="ParseKeyValuePairs(ArgumentUtilitiesHandle{string}, StringComparer, char, char, char)"/>.
		/// </summary>
		public const char KeyValueDefaultDelimiter = '=';

		/// <summary>
		/// Символ-разделитель пар ключ-значение, используемый по умолчанию в методе(-ах) <seealso cref="ParseKeyValuePairs(ArgumentUtilitiesHandle{string}, StringComparer, char, char, char)"/>.
		/// </summary>
		public const char KeyValuePairDefaultDelimiter = ';';

		/// <summary>
		/// Символ отмены (экранирования), используемый по умолчанию в методе(-ах) <seealso cref="ParseKeyValuePairs(ArgumentUtilitiesHandle{string}, StringComparer, char, char, char)"/>.
		/// </summary>
		public const char EscapeDelimiterDefaultMarker = '/';

		// TODO: Put strings into the resources.
		//
		/// <summary>
		/// Выполняет парсинг набора пар ключ-значение из строки.
		/// </summary>
		/// <param name="argument">
		/// Аргумент входной строки.
		/// </param>
		/// <param name="keyComparer">
		/// Компаратор ключей, используемый для проверки уникальности ключей.
		/// <para>По умолчанию используется <see cref="StringComparer"/>.<see cref="StringComparer.OrdinalIgnoreCase"/>.</para>
		/// </param>
		/// <param name="keyValueDelimiter">
		/// Символ-разделитель ключа и значения.
		/// <para>По умолчанию используется '='.</para>
		/// </param>
		/// <param name="keyValuePairDelimiter">
		/// Символ-разделитель пар ключ-значение.
		/// <para>По умолчанию используется ';'.</para>
		/// </param>
		/// <param name="escapeDelimiterMarker">
		/// Символ отмены (экранирования).
		/// <para>По умолчанию используется '/'.</para>
		/// </param>
		/// <returns>Словарь пар ключ-значение <seealso cref="IDictionary{TKey, TValue}"/>.</returns>
		public static IDictionary<string, string> ParseKeyValuePairs(
			this ArgumentUtilitiesHandle<string> argument,
			StringComparer keyComparer = null,
			char keyValueDelimiter = '=',
			char keyValuePairDelimiter = ';',
			char escapeDelimiterMarker = '/') {
			//
			if (keyValueDelimiter == keyValuePairDelimiter)
				throw
					new ArgumentException(
						message: $"Значение аргумента не может равняться значению аргумента '{nameof(keyValuePairDelimiter)}'.",
						paramName: nameof(keyValueDelimiter));
			else if (keyValuePairDelimiter == escapeDelimiterMarker)
				throw
					new ArgumentException(
						message: $"Значение аргумента не может равняться значению аргумента '{nameof(escapeDelimiterMarker)}'.",
						paramName: nameof(keyValuePairDelimiter));
			else if (keyValueDelimiter == escapeDelimiterMarker)
				throw
					new ArgumentException(
						message: $"Значение аргумента не может равняться значению аргумента '{nameof(escapeDelimiterMarker)}'.",
						paramName: nameof(keyValueDelimiter));
			//
			var input = argument.Value;
			var resultDictionary = new Dictionary<string, string>(keyComparer = keyComparer ?? StringComparer.OrdinalIgnoreCase);
			if (string.IsNullOrWhiteSpace(input))
				return resultDictionary;
			else if (input.Length < 2) {
				argument.EnsureHasMinLength(minLength: 2);
				throw new ArgumentException(paramName: argument.Name, message: FormatXResource(locator: typeof(string), subpath: "TooShort"));
			}
			else {
				var inputBuffer = new StringBuilder(input);
				if (inputBuffer.Last() == keyValuePairDelimiter) {
					if (inputBuffer.Last(offset: 1) == escapeDelimiterMarker)
						inputBuffer.Append(keyValuePairDelimiter);
				}
				else if (inputBuffer.Last() == escapeDelimiterMarker) {
					if (inputBuffer.Last(offset: 1) != escapeDelimiterMarker)
						inputBuffer.Append(escapeDelimiterMarker);
					inputBuffer.Append(keyValuePairDelimiter);
				}
				else
					inputBuffer.Append(keyValuePairDelimiter);
				//
				var keyContext = new StringBuilder();
				var valueContext = new StringBuilder();
				var currentContext = keyContext;
				var escapeContext = false;
				for (var inputCharIndex = 0; inputCharIndex < inputBuffer.Length; inputCharIndex++) {
					var inputChar = inputBuffer[ inputCharIndex ];
					if (inputChar == keyValueDelimiter) {
						if (escapeContext) {
							currentContext.Append(inputChar);
							escapeContext = false;
						}
						else if (ReferenceEquals(keyContext, currentContext) && keyContext.Length > 0)
							currentContext = valueContext;
						else
							goto throw_format_exc;
					}
					else if (inputChar == keyValuePairDelimiter) {
						if (escapeContext) {
							currentContext.Append(inputChar);
							escapeContext = false;
						}
						else if (ReferenceEquals(valueContext, currentContext)) {
							var key = keyContext.ToString();
							keyContext.Clear();
							var value = valueContext.ToString();
							valueContext.Clear();
							if (resultDictionary.ContainsKey(key))
								throw
									new ArgumentException(
										message: $"{(argument.IsProp ? $"Указано недопустимое значение свойства '{argument.Name}'.{Environment.NewLine}" : string.Empty)}Невозможно выполнить парсинг набора пар ключ-значение из входной строки. Один из ключей во входной строке не уникален.{Environment.NewLine}\tВходная строка:{input.FmtStr().GNLI2()}{Environment.NewLine}\tНеуникальный ключ:{key.FmtStr().GNLI2()}{Environment.NewLine}\tИспользуемый компаратор ключей:{keyComparer.FmtStr().GNLI2()}",
										paramName: argument.Name);
							resultDictionary.Add(key, value);
							currentContext = keyContext;
						}
						else
							goto throw_format_exc;
					}
					else if (inputChar == escapeDelimiterMarker) {
						if (escapeContext) {
							currentContext.Append(inputChar);
							escapeContext = false;
						}
						else
							escapeContext = true;
					}
					else if (escapeContext) {
						currentContext.Append(escapeDelimiterMarker);
						currentContext.Append(inputChar);
						escapeContext = false;
					}
					else
						currentContext.Append(inputChar);
				}
				return resultDictionary;
			}
			throw_format_exc:
			throw
				new FormatException(
					message: $"{(argument.IsProp ? $"Указано недопустимое значение свойства '{argument.Name}'.{Environment.NewLine}" : string.Empty)}Невозможно выполнить парсинг набора пар ключ-значение из входной строки, так как входная строка имеет некорректный формат.{Environment.NewLine}\tВходная строка:{input.FmtStr().GNLI2()}{Environment.NewLine}\tСпециальные символы:{Environment.NewLine}\t\tРазд. ключа и значения: '{keyValueDelimiter.ToString()}'{Environment.NewLine}\t\tРазд. пар ключ-значение: '{keyValuePairDelimiter.ToString()}'{Environment.NewLine}\t\tСимвол отмены (экранирования): '{escapeDelimiterMarker.ToString()}'");
		}

	}

}