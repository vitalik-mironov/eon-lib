using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		static readonly int __UpperValueOfUnicodeCategory =
			Enum
			.GetValues(enumType: typeof(UnicodeCategory))
			.Cast<int>()
			.Max()
			.Arg(name: $"{nameof(ArgumentUtilitiesCoreL1)}.{nameof(__UpperValueOfUnicodeCategory)}")
			.EnsureNotGreaterThan(operand: 64)
			.Value;

		static readonly int __LowerValueOfUnicodeCategory =
			Enum
			.GetValues(enumType: typeof(UnicodeCategory))
			.Cast<int>()
			.Min()
			.Arg(name: $"{nameof(ArgumentUtilitiesCoreL1)}.{nameof(__LowerValueOfUnicodeCategory)}")
			.EnsureNotLessThan(operand: 0)
			.Value;

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<string> EnsureNotNullOrWhiteSpace(this ArgumentUtilitiesHandle<string> hnd) {
			if (string.IsNullOrWhiteSpace(hnd.Value)) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(string), "CanNotNullOrWhiteSpace")}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<string> EnsureNotNullOrEmpty(this ArgumentUtilitiesHandle<string> hnd) {
			if (string.IsNullOrEmpty(hnd.Value))
				throw new ArgumentOutOfRangeException(hnd.Name, FormatXResource(typeof(string), "CanNotNullOrEmpty"));
			else
				return hnd;
		}

		public static ArgumentUtilitiesHandle<string> EnsureNotEmptyOrWhiteSpace(this string value, string name)
			=> value.Arg(name).EnsureNotEmptyOrWhiteSpace();

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<string> EnsureNotEmptyOrWhiteSpace(this ArgumentUtilitiesHandle<string> hnd) {
			if (hnd.Value != null && string.IsNullOrWhiteSpace(hnd.Value)) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(string), "CanNotEmptyOrWhiteSpace")}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			return hnd;
		}

		public static void EnsureNotNullOrEmpty(this string value, string name) {
			if (string.IsNullOrEmpty(value))
				throw new ArgumentOutOfRangeException(string.IsNullOrEmpty(name) ? nameof(name) : name, FormatXResource(typeof(string), "CanNotNullOrEmpty"));
		}

		public static void EnsureNotEmpty(this string value, string name) {
			if (value == string.Empty)
				throw new ArgumentOutOfRangeException(string.IsNullOrEmpty(name) ? nameof(name) : name, FormatXResource(typeof(string), "CanNotEmpty"));
		}

		public static ArgumentUtilitiesHandle<string> EnsureHasMinLength(this ArgumentUtilitiesHandle<string> hnd, int minLength) {
			minLength.Arg(nameof(minLength)).EnsureNotLessThan(1);
			//
			if (hnd.Value != null && hnd.Value.Length < minLength) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(string), "TooSmall/MinLength", minLength.ToString("d"))}{Environment.NewLine}\tДлина указанного значения (строки):{Environment.NewLine}{hnd.Value.Length.ToString("d").FmtStr().GNLI2()}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(message: exceptionMessage, paramName: hnd.Name);
			}
			return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<string> EnsureHasMaxLength(this ArgumentUtilitiesHandle<string> hnd, int maxLength) {
			maxLength
				.Arg(nameof(maxLength))
				.EnsureNotLessThanZero();
			//
			if (hnd.Value != null && hnd.Value.Length > maxLength) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(string), "TooLong/MaxLength", maxLength.ToString("d"))}{Environment.NewLine}\tДлина указанного значения (строки):{Environment.NewLine}{hnd.Value.Length.ToString("d").FmtStr().GNLI2()}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(message: exceptionMessage, paramName: hnd.Name);
			}
			return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<string> EnsureNoWhiteSpace(this ArgumentUtilitiesHandle<string> hnd) {
			if (string.IsNullOrEmpty(hnd.Value))
				return hnd;
			else {
				for (var i = 0; i < hnd.Value.Length; i++) {
					if (char.IsWhiteSpace(hnd.Value[ i ]))
						throw
							new ArgumentOutOfRangeException(
								message: $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}В значении (в позиции '{i.ToString("d")}') обнаружен пробельный символ, что является недопустимым.{Environment.NewLine}\tЗначение (строка):{Environment.NewLine}{hnd.Value.FmtStr().GNLI2()}",
								paramName: hnd.Name);
				}
				return hnd;
			}
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<string> EnsureStartsWithOrdinalCI(this ArgumentUtilitiesHandle<string> hnd, string startsWithValue) {
			startsWithValue
				.Arg(nameof(startsWithValue))
				.EnsureNotNullOrEmpty();
			//
			if (hnd.Value == null || hnd.Value.StartsWith(startsWithValue, StringComparison.OrdinalIgnoreCase))
				return hnd;
			else
				throw
					new ArgumentOutOfRangeException(
						message: $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}Начало указанной строки (значения) не соответствует требуемому.{Environment.NewLine}\tЗначение (строка):{Environment.NewLine}{hnd.Value.FmtStr().GNLI2()}{Environment.NewLine}\tТребуемое начало строки (значения):{Environment.NewLine}{startsWithValue.FmtStr().GNLI2()}",
						paramName: hnd.Name);
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<string> EnsureNotEmpty(this ArgumentUtilitiesHandle<string> hnd) {
			if (hnd.Value == string.Empty) {
				var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}{FormatXResource(typeof(string), "CanNotEmpty")}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
			else
				return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<string> EnsureMatchRegex(this ArgumentUtilitiesHandle<string> hnd, string regexPattern, ArgumentPlaceholder<RegexOptions> options = default) {
			regexPattern.Arg(nameof(regexPattern)).EnsureNotNullOrEmpty();
			if (hnd.Value is null || (options.HasExplicitValue ? Regex.IsMatch(input: hnd.Value, pattern: regexPattern, options: options.ExplicitValue) : Regex.IsMatch(input: hnd.Value, pattern: regexPattern)))
				return hnd;
			else {
				var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'." : "Invalid value specified.")}{Environment.NewLine}{FormatXResource(typeof(string), "NotMatchRegex", hnd.Value.FmtStr().G(), regexPattern.FmtStr().G())}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
		}

		public static ArgumentUtilitiesHandle<string> EnsureMatchRegex(this ArgumentUtilitiesHandle<string> hnd, Regex regex, string regexName = default) {
			regex.EnsureNotNull(nameof(regex));
			//
			if (hnd.Value is null || regex.IsMatch(input: hnd.Value))
				return hnd;
			else {
				var exceptionMessage = $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'." : "Invalid value specified.")}{Environment.NewLine}{FormatXResource(typeof(string), "NotMatchRegex", hnd.Value.FmtStr().G(), (string.IsNullOrEmpty(regexName) ? "(n/a)" : $"({regexName})").FmtStr().G())}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionMessage)
					?? new ArgumentOutOfRangeException(paramName: hnd.Name, message: exceptionMessage);
			}
		}

		public static ArgumentUtilitiesHandle<Regex> ParseRegex(this ArgumentUtilitiesHandle<string> hnd, RegexOptions regexOptions) {
			if (hnd.Value == null)
				return new ArgumentUtilitiesHandle<Regex>(null, hnd.Name, isPropertyValue: hnd.IsProp);
			else {
				try {
					hnd.EnsureNotEmptyOrWhiteSpace();
					//
					return
						new ArgumentUtilitiesHandle<Regex>(
							value: new Regex(hnd.Value, regexOptions),
							name: hnd.Name,
							isPropertyValue: hnd.IsProp);
				}
				catch (Exception exception) {
					throw
						new ArgumentException(
							message: $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}Ошибка преобразования указанной строки паттерна регулярного выражения в объект выражения (значение типа '{typeof(Regex)}').{Environment.NewLine}\tСтрока:{hnd.Value.FmtStr().GNLI2()}{Environment.NewLine}\tОпции выражения:{regexOptions.FmtStr().GNLI2()}",
							paramName: hnd.Name,
							innerException: exception);
				}
			}
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<bool?> ParseBoolNull(this ArgumentUtilitiesHandle<string> hnd) {
			if (hnd.Value == null)
				return new ArgumentUtilitiesHandle<bool?>(null, hnd.Name);
			else {
				try {
					hnd.EnsureNotNullOrWhiteSpace();
					//
					bool result;
					if (hnd.Value == "1" || string.Equals(hnd.Value, bool.TrueString, StringComparison.OrdinalIgnoreCase))
						result = true;
					else if (hnd.Value == "0" || string.Equals(hnd.Value, bool.FalseString, StringComparison.OrdinalIgnoreCase))
						result = false;
					else
						result = bool.Parse(hnd.Value);
					return new ArgumentUtilitiesHandle<bool?>(result, hnd.Name);
				}
				catch (Exception firstException) {
					throw
						new ArgumentException(
							message: $"Ошибка преобразования строки '{hnd.Value}' в значение типа '{typeof(bool?)}'.{Environment.NewLine}Параметр: '{hnd.Name}'.",
							innerException: firstException);
				}
			}
		}

		public static ArgumentUtilitiesHandle<bool> ParseBool(this ArgumentUtilitiesHandle<string> hnd)
			=>
			new ArgumentUtilitiesHandle<bool>(
				hnd
				.EnsureNotNull()
				.ParseBoolNull()
				.Value
				.Value,
				hnd.Name);

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<TEnum?> ParseEnumNull<TEnum>(this ArgumentUtilitiesHandle<string> hnd, bool ignoreCase = false)
			where TEnum : struct {
			if (hnd.Value == null)
				return new ArgumentUtilitiesHandle<TEnum?>(null, hnd.Name);
			else {
				try {
					hnd.EnsureNotNullOrWhiteSpace();
					//
					var canonizationBuffer = new List<char>();
					var previousCanonizationChar = default(char);
					for (var i = 0; i < hnd.Value.Length; i++) {
						var currentChar = hnd.Value[ i ];
						switch (currentChar) {
							case ' ':
							case '|':
							case '\x0009':
								if (canonizationBuffer.Count > 0 && previousCanonizationChar != ',') {
									previousCanonizationChar = ',';
									canonizationBuffer.Add(previousCanonizationChar);
								}
								break;
							default:
								canonizationBuffer.Add(currentChar);
								previousCanonizationChar = currentChar;
								break;
						}
					}
					if (canonizationBuffer.Count > 0 && canonizationBuffer[ canonizationBuffer.Count - 1 ] == ',')
						canonizationBuffer.RemoveAt(canonizationBuffer.Count - 1);
					var canonizedValue = new string(canonizationBuffer.ToArray());
					//
					return
						new ArgumentUtilitiesHandle<TEnum?>(
							value: (TEnum)Enum.Parse(typeof(TEnum), canonizedValue, ignoreCase),
							name: hnd.Name);
				}
				catch (Exception firstException) {
					throw
						new ArgumentException(
							message: $"Ошибка преобразования строки в значение типа перечисления.{Environment.NewLine}\tСтрока:{hnd.Value.FmtStr().GNLI2()}{Environment.NewLine}\tТип перечисления:{typeof(TEnum).FmtStr().GNLI2()}{Environment.NewLine}Имя параметра: '{hnd.Name}'.",
							innerException: firstException);
				}
			}
		}

		public static ArgumentUtilitiesHandle<TEnum> ParseEnum<TEnum>(this ArgumentUtilitiesHandle<string> hnd, bool ignoreCase = false)
			where TEnum : struct
			=>
			new ArgumentUtilitiesHandle<TEnum>(
				value:
					hnd
					.EnsureNotNull()
					.ParseEnumNull<TEnum>(ignoreCase: ignoreCase)
					.Value
					.Value,
				name: hnd.Name);

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<Uri> ParseUri(this ArgumentUtilitiesHandle<string> hnd) {
			if (hnd.Value == null)
				return new ArgumentUtilitiesHandle<Uri>(null, hnd.Name);
			else {
				try {
					hnd.EnsureNotNullOrWhiteSpace();
					//
					return new ArgumentUtilitiesHandle<Uri>(new Uri(hnd.Value), hnd.Name);
				}
				catch (Exception firstException) {
					throw new ArgumentException($"Ошибка преобразования строки '{hnd.Value}' в значение типа '{typeof(Uri)}'.{Environment.NewLine}Параметр: '{hnd.Name}'.", firstException);
				}
			}
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<int?> ParseInt32NullInvariant(this ArgumentUtilitiesHandle<string> hnd) {
			if (hnd.Value == null)
				return new ArgumentUtilitiesHandle<int?>(null, hnd.Name);
			else {
				try {
					hnd.EnsureNotNullOrWhiteSpace();
					//
					return new ArgumentUtilitiesHandle<int?>(int.Parse(hnd.Value, NumberStyles.Integer, CultureInfo.InvariantCulture), hnd.Name);
				}
				catch (Exception firstException) {
					throw new ArgumentException($"Ошибка преобразования строки '{hnd.Value}' в значение типа '{typeof(int?)}'.{Environment.NewLine}Параметр: '{hnd.Name}'.", firstException);
				}
			}
		}

		public static ArgumentUtilitiesHandle<int> ParseInt32Invariant(this ArgumentUtilitiesHandle<string> hnd)
			=> new ArgumentUtilitiesHandle<int>(value: hnd.EnsureNotNull().ParseInt32NullInvariant().Value.Value, name: hnd.Name);

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<string> EnsureNotEquals(this ArgumentUtilitiesHandle<string> hnd, ArgumentUtilitiesHandle<string> operand, StringComparison comparison) {
			if (string.Equals(hnd.Value, operand.Value, comparison)) {
				throw
					new ArgumentOutOfRangeException(
						paramName: hnd.Name,
						message: $"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}Значение должно быть неэквивалентным значению {(operand.IsProp ? $"свойства" : "аргумента")} '{operand.Name}'.{Environment.NewLine}\tЗначение:{hnd.Value.FmtStr().GNLI2()}{Environment.NewLine}\tТип компаратора строк:{comparison.FmtStr().GNLI2()}");
			}
			else
				return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<string> EnsureNotContains(this ArgumentUtilitiesHandle<string> hnd, char operand) {
			int index;
			if (string.IsNullOrEmpty(hnd.Value) || (index = hnd.Value.IndexOf(operand)) < 0)
				return hnd;
			else {
				var exceptionmessage =
					$"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}Указанное значение содержит недопустимый символ '{operand.ToString()}' в позиции '{index.ToString("d")}'.{Environment.NewLine}\tЗначение:{hnd.Value.FmtStr().GNLI2()}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionmessage)
					?? new ArgumentException(message: exceptionmessage, paramName: hnd.Name);
			}
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<string> EnsureNotContains(this ArgumentUtilitiesHandle<string> hnd, UnicodeCategory category) {
			if (string.IsNullOrEmpty(hnd.Value))
				return hnd;
			else {
				for (var i = 0; i < hnd.Value.Length; i++) {
					if (char.GetUnicodeCategory(s: hnd.Value, index: i) == category) {
						var exceptionmessage =
							$"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}В указанной строке, в позиции '{i.ToString("d")}' присутствует символ, относящийся к недопустимой Unicode-категории '{category.ToString()}' (код символа '0x{((int)hnd.Value[ i ]).ToString("x4")}').{Environment.NewLine}\tСтрока:{hnd.Value.FmtStr().GNLI2()}";
						throw
							hnd.ExceptionFactory?.Invoke(message: exceptionmessage)
							?? new ArgumentException(message: exceptionmessage, paramName: hnd.Name);
					}
				}
				return hnd;
			}
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<string> EnsureNotContainsAny(this ArgumentUtilitiesHandle<string> hnd, char[ ] chars, int? startIndex = default, int? count = default) {
			chars.EnsureNotNull(nameof(chars));
			//
			if (!(hnd.Value is null)) {
				var stringSegmentBoundary = ArrayPartitionBoundary.Get(arrayLength: hnd.Value.Length, offset: startIndex.Arg(nameof(startIndex)), count: count.Arg(nameof(count)));
				foreach (var currentChar in chars) {
					var indexOfCurrentChar = hnd.Value.IndexOf(value: currentChar, startIndex: stringSegmentBoundary.Offset, count: stringSegmentBoundary.Count);
					if (indexOfCurrentChar > -1) {
						var exceptionmessage =
							$"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}В позиции '{indexOfCurrentChar.ToString("d")}' указанной строки обнаружен недопустимый символ '{new string(currentChar, 1)}' (код символа '0x{((int)currentChar).ToString("x4")}').{Environment.NewLine}\tСтрока:{hnd.Value.FmtStr().GNLI2()}";
						throw
							hnd.ExceptionFactory?.Invoke(message: exceptionmessage)
							?? new ArgumentException(message: exceptionmessage, paramName: hnd.Name);
					}
				}
			}
			return hnd;
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<string> EnsureContains(this ArgumentUtilitiesHandle<string> hnd, UnicodeCategory category) {
			if (string.IsNullOrEmpty(hnd.Value))
				return hnd;
			else {
				for (var i = 0; i < hnd.Value.Length; i++) {
					if (char.GetUnicodeCategory(s: hnd.Value, index: i) == category)
						return hnd;
				}
				var exceptionmessage =
					$"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}В указанной строке отсутствует символ, относящийся к Unicode-категории '{(category).ToString()}'.{Environment.NewLine}\tСтрока:{hnd.Value.FmtStr().GNLI2()}";
				throw
					hnd.ExceptionFactory?.Invoke(message: exceptionmessage)
					?? new ArgumentException(message: exceptionmessage, paramName: hnd.Name);
			}
		}

		public static ArgumentUtilitiesHandle<string> EnsureSubset(this ArgumentUtilitiesHandle<string> hnd, UnicodeCategory category) {
			if (string.IsNullOrEmpty(hnd.Value))
				return hnd;
			else
				return EnsureSubset(hnd: hnd, categories: new UnicodeCategory[ ] { category });
		}

		public static ArgumentUtilitiesHandle<string> EnsureSubset(this ArgumentUtilitiesHandle<string> hnd, UnicodeCategory category1, UnicodeCategory category2) {
			if (string.IsNullOrEmpty(hnd.Value))
				return hnd;
			else
				return EnsureSubset(hnd: hnd, categories: new UnicodeCategory[ ] { category1, category2 });
		}

		public static ArgumentUtilitiesHandle<string> EnsureSubset(this ArgumentUtilitiesHandle<string> hnd, UnicodeCategory category1, UnicodeCategory category2, UnicodeCategory category3) {
			if (string.IsNullOrEmpty(hnd.Value))
				return hnd;
			else
				return EnsureSubset(hnd: hnd, categories: new UnicodeCategory[ ] { category1, category2, category3 });
		}

		public static ArgumentUtilitiesHandle<string> EnsureSubset(this ArgumentUtilitiesHandle<string> hnd, UnicodeCategory category1, UnicodeCategory category2, UnicodeCategory category3, UnicodeCategory category4) {
			if (string.IsNullOrEmpty(hnd.Value))
				return hnd;
			else
				return EnsureSubset(hnd: hnd, categories: new UnicodeCategory[ ] { category1, category2, category3, category4 });
		}

		// TODO: Put strings into the resources.
		//
		public static ArgumentUtilitiesHandle<string> EnsureSubset(this ArgumentUtilitiesHandle<string> hnd, params UnicodeCategory[ ] categories) {
			categories.EnsureNotNull(nameof(categories)).EnsureHasMinLength(minLength: 1);
			//
			if (string.IsNullOrEmpty(hnd.Value))
				return hnd;
			else {
				var catBitMask = new bool[ __UpperValueOfUnicodeCategory + 1 ];
				var lowerBound = categories.GetLowerBound(dimension: 0);
				for (var offset = 0; offset < categories.Length; offset++) {
					var y = lowerBound + offset;
					catBitMask[ ((int)categories[ y ]).Arg(name: $"{nameof(categories)}[{y:d}]").EnsureNotLessThan(operand: __LowerValueOfUnicodeCategory).EnsureNotGreaterThan(operand: __UpperValueOfUnicodeCategory).Value ] = true;
				}
				for (var i = 0; i < hnd.Value.Length; i++) {
					var catInt32 = (int)char.GetUnicodeCategory(s: hnd.Value, index: i);
					if (!catBitMask[ catInt32 ]) {
						var exceptionmessage =
							$"{(hnd.IsProp ? $"Invalid value of property '{hnd.Name}'.{Environment.NewLine}" : string.Empty)}В позиции '{i.ToString("d")}' указанной строки обнаружен символ, Unicode-категории которого '{((UnicodeCategory)catInt32).ToString()}' не входит в набор допустимых (код символа '0x{((int)hnd.Value[ i ]).ToString("x4")}').{Environment.NewLine}\tСтрока:{hnd.Value.FmtStr().GNLI2()}";
						throw
							hnd.ExceptionFactory?.Invoke(message: exceptionmessage)
							?? new ArgumentException(message: exceptionmessage, paramName: hnd.Name);
					}
				}
				return hnd;
			}
		}

	}

}