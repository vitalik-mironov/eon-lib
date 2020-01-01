using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Eon.Context;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Text {

	public static class TextUtilities {

		public const string TemplateRegexSubstituionGroupName = "SubstitutionMarker";

		public const string SquareBracketTemplateRegexPattern = @"\[\[([^\[\]]+)\]([^\[\]]*)\]";

		public static readonly Regex SquareBracketTemplateRegex = new Regex(pattern: SquareBracketTemplateRegexPattern, options: RegexOptions.Compiled);

		static long __VersionCounter;

		static ImmutableDictionary<(string openToken, string closeToken), (Regex regex, long version)> __SubstitutionTemplateRegexCache;

		static readonly int __MaxSizeOfSubstitutionRegexCache;

		static readonly UnicodeCategory[ ] __SubstitutionOpenTokenSet;

		static readonly UnicodeCategory[ ] __SubstitutionCloseTokenSet;

		static TextUtilities() {
			__VersionCounter = long.MinValue;
			__SubstitutionTemplateRegexCache = ImmutableDictionary.Create<(string openToken, string closeToken), (Regex regex, long version)>(keyComparer: EqualityComparer<(string, string)>.Default);
			__MaxSizeOfSubstitutionRegexCache = 128;
			__SubstitutionOpenTokenSet =
				new UnicodeCategory[ ] {
					UnicodeCategory.LetterNumber,
					UnicodeCategory.LowercaseLetter,
					UnicodeCategory.UppercaseLetter,
					UnicodeCategory.DecimalDigitNumber,
					UnicodeCategory.CurrencySymbol,
					UnicodeCategory.DashPunctuation,
					UnicodeCategory.OtherPunctuation,
					UnicodeCategory.OpenPunctuation
				};
			__SubstitutionCloseTokenSet =
				new UnicodeCategory[ ] {
					UnicodeCategory.LetterNumber,
					UnicodeCategory.LowercaseLetter,
					UnicodeCategory.UppercaseLetter,
					UnicodeCategory.DecimalDigitNumber,
					UnicodeCategory.CurrencySymbol,
					UnicodeCategory.DashPunctuation,
					UnicodeCategory.OtherPunctuation,
					UnicodeCategory.ClosePunctuation
				};
		}

		static bool P_TryGetSubstitutionTemplateRegexFromCache((string openToken, string closeToken) key, out Regex regex) {
			if (__MaxSizeOfSubstitutionRegexCache > 0) {
				if (itrlck.Get(ref __SubstitutionTemplateRegexCache).TryGetValue(key: key, value: out var value)) {
					regex = value.regex;
					return true;
				}
				else {
					regex = null;
					return false;
				}
			}
			else {
				regex = null;
				return false;
			}
		}

		static bool P_TryPutSubstitutionTemplateRegexInCache((string openToken, string closeToken) key, Regex regex) {
			if (__MaxSizeOfSubstitutionRegexCache > 0) {
				ImmutableInterlocked
				.Update(
					location: ref __SubstitutionTemplateRegexCache,
					transformer:
						locCurrent => {
							var locNewVersion = itrlck.Increment(location: ref __VersionCounter, maxInclusive: long.MaxValue, locationName: $"{nameof(TextUtilities)}.{nameof(__VersionCounter)}");
							if (locCurrent.Count < __MaxSizeOfSubstitutionRegexCache || locCurrent.ContainsKey(key: key))
								return locCurrent.SetItem(key: key, value: (regex: regex, version: locNewVersion));
							else {
								var locSelectedReplaceVersion = long.MaxValue;
								var locSelectedReplaceKey = default((string substitutionStartToken, string substitutionEndToken));
								foreach (var locItem in locCurrent) {
									if (locItem.Value.version <= locSelectedReplaceVersion) {
										locSelectedReplaceVersion = locItem.Value.version;
										locSelectedReplaceKey = locItem.Key;
									}
								}
								return locCurrent.Remove(key: locSelectedReplaceKey).Add(key: key, value: (regex: regex, version: locNewVersion));
							}
						});
				return true;
			}
			else
				return false;
		}

		static Regex P_GetSubstitutionTemplateRegex(string openToken, string closeToken) {
			openToken.EnsureNotNull(nameof(openToken)).EnsureHasMaxLength(maxLength: 8).EnsureNotEmptyOrWhiteSpace();
			closeToken.EnsureNotNull(nameof(closeToken)).EnsureHasMaxLength(maxLength: 8).EnsureNotEmptyOrWhiteSpace();
			//
			if (!P_TryGetSubstitutionTemplateRegexFromCache(key: (openToken: openToken, closeToken: closeToken), regex: out var regex)) {
				openToken.Arg(nameof(openToken)).EnsureSubset(categories: __SubstitutionOpenTokenSet).EnsureContains(category: UnicodeCategory.OpenPunctuation);
				closeToken.Arg(nameof(closeToken)).EnsureSubset(categories: __SubstitutionCloseTokenSet).EnsureContains(category: UnicodeCategory.ClosePunctuation);
				var sb = new StringBuilder();
				sb.Append('(');
				for (var y = 0; y < openToken.Length; y++)
					appendRegexPattern(sb, openToken[ y ]);
				sb.Append(@"(?<");
				sb.Append(TemplateRegexSubstituionGroupName);
				sb.Append(@">[^");
				for (var y = 0; y < openToken.Length; y++)
					appendRegexPattern(sb, openToken[ y ]);
				for (var y = 0; y < closeToken.Length; y++)
					appendRegexPattern(sb, closeToken[ y ]);
				sb.Append('\\');
				sb.Append("n]+)");
				for (var y = 0; y < closeToken.Length; y++)
					appendRegexPattern(sb, closeToken[ y ]);
				sb.Append(')');
				regex = new Regex(pattern: sb.ToString(), options: RegexOptions.Compiled);
				P_TryPutSubstitutionTemplateRegexInCache(key: (openToken: openToken, closeToken: closeToken), regex: regex);
			}
			return regex;
			//
			void appendRegexPattern(StringBuilder sb, char ch) {
				switch (char.GetUnicodeCategory(c: ch)) {
					case UnicodeCategory.LetterNumber:
					case UnicodeCategory.LowercaseLetter:
					case UnicodeCategory.UppercaseLetter:
					case UnicodeCategory.DecimalDigitNumber:
						sb.Append(ch);
						break;
					default:
						sb.Append('\\');
						sb.Append(ch);
						break;
				}
			}
		}

		// TODO: Put strings into the resources.
		//
		static string P_GetSubstitutionValue(in TextTemplateVar var, bool required, IReadOnlyDictionary<string, string> values) {
			var key = var.Var.PropArg($"{nameof(var)}.{nameof(var.Var)}").EnsureNotNull().Value;
			values.EnsureNotNull(nameof(values));
			//
			if (values.TryGetValue(key: key, value: out var value))
				return value;
			else if (required)
				throw new KeyNotFoundException(message: $"Specified dictionary doesn't contain a value for the specified name.{Environment.NewLine}\tName:{key.FmtStr().GNLI2()}");
			else
				return null;
		}

		/// <summary>
		/// Substitutes variables with a values provided by <paramref name="substitutor"/>.
		/// </summary>
		/// <typeparam name="TState">Constraint of the user state type.</typeparam>
		/// <param name="openToken">
		/// The opening token of the variable's span.
		/// <para>Can't be <see langword="null"/>, <see cref="string.Empty"/> or whitespace string.</para>
		/// <para>Length can't be greater than <see langword="8"/>.</para>
		/// </param>
		/// <param name="closeToken">
		/// The closing token of the variable's span.
		/// <para>Can't be <see langword="null"/>, <see cref="string.Empty"/> or whitespace string.</para>
		/// <para>Length can't be greater than <see langword="8"/>.</para>
		/// </param>
		/// <param name="template">Template.</param>
		/// <param name="substitutor">
		/// Maps the variable name to a value. That value will be placed instead of variable's span in the template.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="state">
		/// User state that passes to <paramref name="substitutor"/>.
		/// </param>
		/// <param name="culture">
		/// Default culture to be used for substitution.
		/// </param>
		/// <param name="filter">
		/// Special method to filter the variables to be substituted.
		/// <para><see langword="true"/> — substitute the variable, variable value will be taken from <paramref name="substitutor"/> method.</para>
		/// <para><see langword="false"/> — skip the variable (see <paramref name="filterThrows"/>).</para>
		/// </param>
		/// <param name="filterThrows">
		/// Indicates whether an exception should be thrown when the variable don't match to the <paramref name="filter"/>.
		/// <para><see langword="false"/> — the variable's span will be passed to result string as is.</para>
		/// </param>
		/// <param name="ctx">
		/// Operation context.
		/// </param>
		public static async Task<string> SubstituteTemplateAsync<TState>(
			string openToken,
			string closeToken,
			string template,
			Func<(TextTemplateVar var, CultureInfo culture, TState state, IContext ctx), Task<string>> substitutor,
			TState state,
			CultureInfo culture = default,
			InParamFunc<TextTemplateVar, bool> filter = default,
			bool filterThrows = default,
			IContext ctx = default) {
			//
			openToken.EnsureNotNull(nameof(openToken)).EnsureHasMaxLength(maxLength: 8).EnsureNotEmptyOrWhiteSpace();
			closeToken.EnsureNotNull(nameof(closeToken)).EnsureHasMaxLength(maxLength: 8).EnsureNotEmptyOrWhiteSpace();
			substitutor.EnsureNotNull(nameof(substitutor));
			//
			ctx.ThrowIfCancellationRequested();
			//
			if (string.IsNullOrEmpty(template))
				return template;
			else {
				var vars = GetTemplateVariables(openToken: openToken, closeToken: closeToken, template: template);
				if (vars.Length > 0) {
					using (var buffer = EonStringBuilderUtilities.AcquireBuffer()) {
						var sb = buffer.StringBuilder;
						var varsUpperBound = vars.Length - 1;
						var templateReadPosition = 0;
						var templateUpperIndex = template.Length - 1;
						for (var i = 0; ; i++) {
							ctx.ThrowIfCancellationRequested();
							//
							var templateForepartLength = vars[ i ].SpanStart - templateReadPosition;
							if (templateForepartLength != 0)
								sb.Append(value: template, startIndex: templateReadPosition, count: templateForepartLength);
							templateReadPosition = vars[ i ].SpanStart + vars[ i ].SpanLength;
							string variableSubstitution;
							if (filter?.Invoke(arg: in vars[ i ]) ?? true)
								variableSubstitution = await substitutor(arg: (var: vars[ i ], culture, state, ctx)).ConfigureAwait(false);
							else if (filterThrows)
								throw new EonException(message: $"Template variable not match the specified filter and not allowed to be substituted.{Environment.NewLine}\tVariable:{vars[ i ].Var.FmtStr().GNLI2()}");
							else
								variableSubstitution = vars[ i ].Span;
							sb.Append(value: variableSubstitution);
							if (i == varsUpperBound) {
								var templateTailLength = template.Length - templateReadPosition;
								if (templateTailLength != 0)
									sb.Append(value: template, startIndex: templateReadPosition, count: templateTailLength);
								break;
							}
						}
						return sb.ToString();
					}
				}
				else
					return template;
			}
		}

		/// <summary>
		/// Substitutes variables with a values provided by <paramref name="substitutor"/>.
		/// </summary>
		/// <param name="openToken">
		/// The opening token of the variable's span.
		/// <para>Can't be <see langword="null"/>, <see cref="string.Empty"/> or whitespace string.</para>
		/// <para>Length can't be greater than <see langword="8"/>.</para>
		/// </param>
		/// <param name="closeToken">
		/// The closing token of the variable's span.
		/// <para>Can't be <see langword="null"/>, <see cref="string.Empty"/> or whitespace string.</para>
		/// <para>Length can't be greater than <see langword="8"/>.</para>
		/// </param>
		/// <param name="template">Template.</param>
		/// <param name="substitutor">
		/// Maps the variable name to a value. That value will be placed instead of variable's span in the template.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="culture">
		/// Default culture to be used for substitution.
		/// </param>
		/// <param name="filter">
		/// Special method to filter the variables to be substituted.
		/// <para><see langword="true"/> — substitute the variable, variable value will be taken from <paramref name="substitutor"/> method.</para>
		/// <para><see langword="false"/> — skip the variable (see <paramref name="filterThrows"/>).</para>
		/// </param>
		/// <param name="filterThrows">
		/// Indicates whether an exception should be thrown when the variable don't match to the <paramref name="filter"/>.
		/// <para><see langword="false"/> — the variable's span will be passed to result string as is.</para>
		/// </param>
		/// <param name="ctx">
		/// Operation context.
		/// </param>
		public static async Task<string> SubstituteTemplateAsync(
			string openToken,
			string closeToken,
			string template,
			Func<(TextTemplateVar var, CultureInfo culture, IContext ctx), Task<string>> substitutor,
			CultureInfo culture = default,
			InParamFunc<TextTemplateVar, bool> filter = default,
			bool filterThrows = default,
			IContext ctx = default) {
			substitutor.EnsureNotNull(nameof(substitutor));
			//
			return await SubstituteTemplateAsync(openToken: openToken, closeToken: closeToken, template: template, substitutor: locSubstitutor, state: Nil.Value, culture: culture, filter: filter, filterThrows: filterThrows, ctx: ctx).ConfigureAwait(false);
			//
			async Task<string> locSubstitutor((TextTemplateVar var, CultureInfo culture, Nil state, IContext ctx) locArgs)
				=> await substitutor(arg: (locArgs.var, locArgs.culture, locArgs.ctx)).ConfigureAwait(false);
		}

		/// <summary>
		/// Substitutes variables with a values provided by <paramref name="values"/>.
		/// </summary>
		/// <param name="openToken">
		/// The opening token of the variable's span.
		/// <para>Can't be <see langword="null"/>, <see cref="string.Empty"/> or whitespace string.</para>
		/// <para>Length can't be greater than <see langword="8"/>.</para>
		/// </param>
		/// <param name="closeToken">
		/// The closing token of the variable's span.
		/// <para>Can't be <see langword="null"/>, <see cref="string.Empty"/> or whitespace string.</para>
		/// <para>Length can't be greater than <see langword="8"/>.</para>
		/// </param>
		/// <param name="template">Template.</param>
		/// <param name="values">
		/// Maps the variable name to a value. That value will be placed instead of variable's span in the template.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="culture">
		/// Culture to be used for formatting.
		/// </param>
		/// <param name="filter">
		/// Special method to filter the variables to be substituted.
		/// <para><see langword="true"/> — substitute the variable, variable value will be taken from <paramref name="values"/> method.</para>
		/// <para><see langword="false"/> — skip the variable (see <paramref name="filterThrows"/>).</para>
		/// </param>
		/// <param name="filterThrows">
		/// Indicates whether an exception should be thrown when the variable don't match to the <paramref name="filter"/>.
		/// <para><see langword="false"/> — the variable's span will be passed to result string as is.</para>
		/// </param>
		/// <param name="ctx">
		/// Operation context.
		/// </param>
		public static async Task<string> SubstituteTemplateAsync(
			string openToken,
			string closeToken,
			string template,
			ITextTemplateVarSubstitution values,
			CultureInfo culture = default,
			InParamFunc<TextTemplateVar, bool> filter = default,
			bool filterThrows = default,
			IContext ctx = default) {
			//
			values.EnsureNotNull(nameof(values));
			//
			return await SubstituteTemplateAsync(openToken: openToken, closeToken: closeToken, template: template, substitutor: locSubstitutor, state: values, culture: culture, filter: filter, filterThrows: filterThrows, ctx: ctx).ConfigureAwait(false);
			//
			async Task<string> locSubstitutor((TextTemplateVar var, CultureInfo culture, ITextTemplateVarSubstitution state, IContext ctx) locArgs)
				=> await locArgs.state.SubstituteAsync(var: locArgs.var, culture: locArgs.culture, ctx: locArgs.ctx).ConfigureAwait(false);
		}

		public static string SubstituteTemplate(string openToken, string closeToken, string template, InParamFunc<TextTemplateVar, string> substitutor) {
			openToken.EnsureNotNull(nameof(openToken)).EnsureHasMaxLength(maxLength: 8).EnsureNotEmptyOrWhiteSpace();
			closeToken.EnsureNotNull(nameof(closeToken)).EnsureHasMaxLength(maxLength: 8).EnsureNotEmptyOrWhiteSpace();
			substitutor.EnsureNotNull(nameof(substitutor));
			//
			if (string.IsNullOrEmpty(template))
				return template;
			else {
				var regex = P_GetSubstitutionTemplateRegex(openToken: openToken, closeToken: closeToken);
				return
					regex
					.Replace(
						input: template,
						evaluator:
							locMatch => {
								var locGroup = locMatch.Groups[ TemplateRegexSubstituionGroupName ];
								if (locGroup?.Success == true) {
									var locVar = new TextTemplateVar(template: template, span: locMatch.Value, spanStart: locMatch.Index, spanLength: locMatch.Length, var: locGroup.Value);
									return substitutor(arg: in locVar);
								}
								else
									return default;
							});
			}
		}

		public static string SubstituteTemplate(string openToken, string closeToken, string template, ITextTemplateVarSubstitution values, CultureInfo culture = default) {
			values.EnsureNotNull(nameof(values));
			//
			return SubstituteTemplate(openToken: openToken, closeToken: closeToken, template: template, substitutor: locSubstitutor);
			//
			string locSubstitutor(in TextTemplateVar locVar)
				=> values.Substitute(var: in locVar, culture: culture, ctx: null);
		}

		public static string SubstituteTemplate(string openToken, string closeToken, string template, IReadOnlyDictionary<string, string> values) {
			values.EnsureNotNull(nameof(values));
			//
			return
				SubstituteTemplate(
					openToken: openToken,
					closeToken: closeToken,
					template: template,
					substitutor:
						(in TextTemplateVar locVar)
						=>
						P_GetSubstitutionValue(var: in locVar, values: values, required: true));
		}

		public static string SubstituteBraceTemplate(string template, InParamFunc<TextTemplateVar, string> substitutor)
			=> SubstituteTemplate(openToken: "{", closeToken: "}", template: template, substitutor: substitutor);

		public static string SubstituteBraceTemplate(string template, ITextTemplateVarSubstitution values, CultureInfo culture = default) {
			values.EnsureNotNull(nameof(values));
			//
			return SubstituteBraceTemplate(template: template, substitutor: locSubstitutor);
			//
			string locSubstitutor(in TextTemplateVar locVar)
				=> values.Substitute(var: in locVar, culture: culture, ctx: null);
		}

		public static string SubstituteBraceTemplate(string template, IReadOnlyDictionary<string, string> values) {
			values.EnsureNotNull(nameof(values));
			//
			return SubstituteBraceTemplate(template: template, substitutor: (in TextTemplateVar locVar) => P_GetSubstitutionValue(var: in locVar, values: values, required: true));
		}

		public static TextTemplateVar[ ] GetTemplateVariables(string openToken, string closeToken, string template) {
			template.EnsureNotNull(nameof(template));
			//
			if (template.Length < 2)
				return new TextTemplateVar[ 0 ];
			else {
				var regex = P_GetSubstitutionTemplateRegex(openToken: openToken, closeToken: closeToken);
				return
					regex
					.Matches(input: template)
					.Cast<Match>()
					.Where(locMatch => locMatch.Success)
					.Select(
						locMatch => {
							var locGroup = locMatch.Groups[ TemplateRegexSubstituionGroupName ];
							if (locGroup?.Success == true)
								return new TextTemplateVar(template: template, span: locMatch.Value, spanStart: locMatch.Index, spanLength: locMatch.Length, var: locGroup.Value);
							else
								return default;
						})
					.Where(locItem => !(locItem.Template is null))
					.ToArray();
			}
		}

		// TODO: Put exception messages into the resources.
		//
		public static string GetEncodingWebName(KnownEncodingWebName webName) {
			if (webName == KnownEncodingWebName.Cyrillic_Windows)
				return "windows-1251";
			else if (webName == KnownEncodingWebName.Cyrillic_Dos)
				return "cp866";
			else if (webName == KnownEncodingWebName.UTF8)
				return Encoding.UTF8.WebName;
			else
				throw new ArgumentOutOfRangeException(paramName: nameof(webName), message: $"Указанное значение '{webName}' не поддерживается.");
		}

		// TODO: Put strings into the resources.
		//
		/// <summary>
		/// Выполняет парсинг набора пар ключ-значение из строки.
		/// </summary>
		/// <param name="input">Входная строка, представляющая набор пар ключ-значение.</param>
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
		/// <param name="inputArgName">Альтернативное имя аргумента <paramref name="input"/> входной строки. Используется для указания в исключениях <see cref="ArgumentException"/>, которые могут быть вызваны данным методом. Если имеет значение null или <see cref="string.Empty"/>, то используется имя <paramref name="input"/>.</param>
		/// <returns>Словарь пар ключ-значение.</returns>
		public static IDictionary<string, string> ParseKeyValuePairs(
			this string input,
			StringComparer keyComparer = default,
			char keyValueDelimiter = '=',
			char keyValuePairDelimiter = ';',
			char escapeDelimiterMarker = '/',
			string inputArgName = default)
			=> ArgumentUtilitiesCoreL1.ParseKeyValuePairs(argument: input.Arg(string.IsNullOrEmpty(inputArgName) ? nameof(input) : inputArgName), keyComparer: keyComparer, keyValueDelimiter: keyValueDelimiter, keyValuePairDelimiter: keyValuePairDelimiter, escapeDelimiterMarker: escapeDelimiterMarker);

	}

}