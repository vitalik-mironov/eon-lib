using System;
using System.Collections.Generic;

namespace Eon {

	public static partial class ArgumentUtilitiesCoreL1 {

		/// <summary>
		/// Символ-разделитель имени параметра и его значения, используемый в методах парсинга словаря параметров <seealso cref="ParseParametersDictionary(string[])"/>.
		/// </summary>
		public const char ParameterNameDefaultDelimiter = '?';

		/// <summary>
		/// Выполняет парсинг параметров, заданных в виде массива строк.
		/// <para>Каждый элемент массива <paramref name="parameters"/> представляет пару параметр-значение.</para>
		/// <para>Если элемент массива является null, пустой строкой или строкой, содержащей только пробельные символы, то этот элемент пропускается.</para>
		/// <para>Ожидаемый формат параметра (элемента массива <paramref name="parameters"/>): &lt;имя параметра&gt;&lt;разделитель имени параметра и значения&gt;&lt;значение параметра&gt;. Значение параметра может быть пустой строкой.</para>
		/// <para>В качестве компаратора ключей словаря, где ключ — имя параметра, используется <see cref="StringComparer.OrdinalIgnoreCase"/>.</para>
		/// </summary>
		/// <param name="parameters">
		/// Массив строк, каждый элемент которого представляет отдельную пару параметр-значение.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <returns>Словарь <see cref="IDictionary{TKey, TValue}"/>.</returns>
		public static IDictionary<string, string> ParseParametersDictionary(string[ ] parameters)
			=> ParseParametersDictionary(parameters: parameters.Arg(nameof(parameters)), parameterNameDelimiter: ParameterNameDefaultDelimiter.Arg("parameterNameDelimiter"));

		/// <summary>
		/// Выполняет парсинг параметров, заданных в виде массива строк.
		/// <para>Каждый элемент массива <paramref name="parameters"/> представляет пару параметр-значение.</para>
		/// <para>Если элемент массива является null, пустой строкой или строкой, содержащей только пробельные символы, то этот элемент пропускается.</para>
		/// <para>Ожидаемый формат параметра (элемента массива <paramref name="parameters"/>): &lt;имя параметра&gt;&lt;разделитель имени параметра и значения&gt;&lt;значение параметра&gt;. Значение параметра может быть пустой строкой.</para>
		/// <para>В качестве компаратора ключей словаря, где ключ — имя параметра, используется <see cref="StringComparer.OrdinalIgnoreCase"/>.</para>
		/// </summary>
		/// <param name="parameters">
		/// Массив строк, каждый элемент которого представляет отдельную пару параметр-значение.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <returns>Словарь <see cref="IDictionary{TKey, TValue}"/>.</returns>
		public static IDictionary<string, string> ParseParametersDictionary(ArgumentUtilitiesHandle<string[ ]> parameters)
			=> ParseParametersDictionary(parameters: parameters, parameterNameDelimiter: ParameterNameDefaultDelimiter.Arg("parameterNameDelimiter"));

		// TODO: Put strings into the resources.
		//
		/// <summary>
		/// Выполняет парсинг параметров, заданных в виде массива строк.
		/// <para>Каждый элемент массива <paramref name="parameters"/> представляет пару параметр-значение.</para>
		/// <para>Если элемент массива является <see langword="null"/>, пустой строкой или строкой, содержащей только пробельные символы, то этот элемент пропускается.</para>
		/// <para>Ожидаемый формат параметра (элемента массива <paramref name="parameters"/>): &lt;имя параметра&gt;&lt;разделитель имени параметра и значения (<paramref name="parameterNameDelimiter"/>)&gt;&lt;значение параметра&gt;. Значение параметра может быть пустой строкой.</para>
		/// <para>В качестве компаратора ключей словаря, где ключ — имя параметра, используется <see cref="StringComparer.OrdinalIgnoreCase"/>.</para>
		/// </summary>
		/// <param name="parameters">
		/// Массив строк, каждый элемент которого представляет отдельную пару параметр-значение.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="parameterNameDelimiter">
		/// Символ-разделитель имени параметра и его значения.
		/// <para>Не может быть пробельным символом (см. <seealso cref="char.IsWhiteSpace(char)"/>).</para>
		/// </param>
		/// <returns>Словарь <see cref="IDictionary{TKey, TValue}"/>.</returns>
		public static IDictionary<string, string> ParseParametersDictionary(ArgumentUtilitiesHandle<string[ ]> parameters, ArgumentUtilitiesHandle<char> parameterNameDelimiter) {
			parameters.EnsureNotNull();
			if (char.IsWhiteSpace(parameterNameDelimiter.Value))
				throw new ArgumentOutOfRangeException(paramName: parameterNameDelimiter.Name, message: "Значение не может быть пробельным символом.");
			//
			Func<string, char, int, string, string[ ]> parseArgument =
				(locParametersArrayArgName, locParameterNameDelimiter, locParameterPosition, locParameterText) => {
					if (string.IsNullOrWhiteSpace(locParameterText))
						return null;
					else {
						var locNamePart = new List<char>();
						var locValuePart = new List<char>();
						var locCurrentPart = locNamePart;
						var locIsParameterNameDelimiterOccurred = false;
						for (var i = 0; i < locParameterText.Length; i++) {
							var locCurrentChar = locParameterText[ i ];
							if (locCurrentChar == locParameterNameDelimiter) {
								// Символ-разделитель имени и значения параметра.
								//
								if (ReferenceEquals(locCurrentPart, locNamePart)) {
									locIsParameterNameDelimiterOccurred = true;
									// Удаление конечных "пробелов" в имени.
									//
									for (var y = locNamePart.Count - 1; y > -1; y--) {
										if (char.IsWhiteSpace(locNamePart[ y ]))
											locNamePart.RemoveAt(y);
										else
											break;
									}
									if (locNamePart.Count < 1)
										throw
											new ArgumentException(
												message: $"Параметр в позиции '{locParameterPosition.ToString("d")}' имеет недопустимый формат.{Environment.NewLine}\tПараметр:{locParameterText.FmtStr().GNLI2()}",
												paramName: locParametersArrayArgName);
									else
										locCurrentPart = locValuePart;
								}
								else
									locCurrentPart.Add(locCurrentChar);
							}
							else if (ReferenceEquals(locCurrentPart, locNamePart)) {
								if (locCurrentPart.Count > 0 || !char.IsWhiteSpace(locCurrentChar))
									locCurrentPart.Add(locCurrentChar);
							}
							else
								locCurrentPart.Add(locCurrentChar);
						}
						//
						if (!locIsParameterNameDelimiterOccurred || locNamePart.Count < 1)
							throw
								new ArgumentException(
									message: $"Параметр в позиции '{locParameterPosition.ToString("d")}' имеет недопустимый формат.{Environment.NewLine}\tПараметр:{locParameterText.FmtStr().GNLI2()}",
									paramName: locParametersArrayArgName);
						return
							new string[ ] {
								new string(locNamePart.ToArray()),
								locValuePart.Count < 1 ? null : new string(locValuePart.ToArray()) };
					}
				};
			//
			var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			string[ ] parameterNameAndValue;
			for (var i = 0; i < parameters.Value.Length; i++) {
				parameterNameAndValue =
					parseArgument(
						parameters.Name,
						parameterNameDelimiter.Value,
						i,
						parameters.Value[ i ]);
				if (parameterNameAndValue == null)
					continue;
				else if (result.ContainsKey(parameterNameAndValue[ 0 ]))
					throw
						new ArgumentException(
							message: $"Параметр '{parameters.Value[ i ]}' (в позиции {i.ToString("d")}) указан по меньшей мере дважды. Каждый аргумент должен указываться единажды.",
							paramName: parameters.Name);
				result.Add(parameterNameAndValue[ 0 ], parameterNameAndValue[ 1 ]);
			}
			return result;
		}

		/// <summary>
		/// Выполняет парсинг параметров, заданных в виде массива строк.
		/// <para>Каждый элемент массива <paramref name="parameters"/> представляет пару параметр-значение.</para>
		/// <para>Если элемент массива является null, пустой строкой или строкой, содержащей только пробельные символы, то этот элемент пропускается.</para>
		/// <para>Ожидаемый формат параметра (элемента массива <paramref name="parameters"/>): &lt;имя параметра&gt;&lt;разделитель имени параметра и значения (<paramref name="parameterNameDelimiter"/>)&gt;&lt;значение параметра&gt;. Значение параметра может быть пустой строкой.</para>
		/// <para>В качестве компаратора ключей словаря, где ключ — имя параметра, используется <see cref="StringComparer.OrdinalIgnoreCase"/>.</para>
		/// </summary>
		/// <param name="parameters">
		/// Массив строк, каждый элемент которого представляет отдельную пару параметр-значение.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="parameterNameDelimiter">
		/// Символ-разделитель имени параметра и его значения.
		/// <para>Не может быть пробельным символом (см. <seealso cref="char.IsWhiteSpace(char)"/>).</para>
		/// </param>
		/// <returns>Словарь <see cref="IDictionary{TKey, TValue}"/>.</returns>
		public static IDictionary<string, string> ParseParametersDictionary(string[ ] parameters, char parameterNameDelimiter)
			=> ParseParametersDictionary(parameters: parameters.Arg(nameof(parameters)), parameterNameDelimiter: parameterNameDelimiter.Arg(nameof(parameterNameDelimiter)));

	}

}