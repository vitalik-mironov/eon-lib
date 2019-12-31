using System.Collections.Generic;
using System.Globalization;

using Eon.Diagnostics;

namespace Eon.Runtime.Options {

	public sealed class ErrorCodeIdentifierPrefixOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value: '16'.
		/// <para>See <see cref="ErrorCode.IdentifierMaxLength"/>.</para>
		/// </summary>
		public static readonly int MaxLengthOfPrefix;

		static readonly UnicodeCategory[ ] __AllowedPrefixCharCategories;

		static readonly IReadOnlyList<UnicodeCategory> __AllowedPrefixCharCategoriesReadOnlyWrap;

		/// <summary>
		/// Value: '<see cref="UnicodeCategory.DecimalDigitNumber"/>, <see cref="UnicodeCategory.LetterNumber"/>, <see cref="UnicodeCategory.LowercaseLetter"/>, <see cref="UnicodeCategory.OtherLetter"/>, <see cref="UnicodeCategory.TitlecaseLetter"/>, <see cref="UnicodeCategory.UppercaseLetter"/>, <see cref="UnicodeCategory.OtherPunctuation"/>, <see cref="UnicodeCategory.CurrencySymbol"/>'.
		/// </summary>
		public static IReadOnlyList<UnicodeCategory> AllowedCharCategoriesOfPrefix
			=> __AllowedPrefixCharCategoriesReadOnlyWrap;

		/// <summary>
		/// Value: 'Oxy' (see <see cref="ErrorCodeIdentifierPrefixOption.Prefix"/>).
		/// </summary>
		public static readonly ErrorCodeIdentifierPrefixOption Fallback;

		static ErrorCodeIdentifierPrefixOption() {
			MaxLengthOfPrefix = 16;
			__AllowedPrefixCharCategories =
				new UnicodeCategory[ ] {
					UnicodeCategory.DecimalDigitNumber,
					UnicodeCategory.LetterNumber,
					UnicodeCategory.LowercaseLetter,
					UnicodeCategory.OtherLetter,
					UnicodeCategory.TitlecaseLetter,
					UnicodeCategory.UppercaseLetter,
					UnicodeCategory.OtherPunctuation,
					UnicodeCategory.CurrencySymbol
				};
			__AllowedPrefixCharCategoriesReadOnlyWrap = __AllowedPrefixCharCategories.AsCountableEnumerable();
			//
			Fallback = new ErrorCodeIdentifierPrefixOption(prefix: "Oxy");
			RuntimeOptions.Option<ErrorCodeIdentifierPrefixOption>.SetFallback(option: Fallback);
		}

		public static string Require()
			=> RuntimeOptions.Option<ErrorCodeIdentifierPrefixOption>.Require().Prefix;

		#endregion

		readonly string _prefix;

		public ErrorCodeIdentifierPrefixOption(string prefix) {
			prefix.EnsureNotNull(nameof(prefix)).EnsureNotEmpty().EnsureHasMaxLength(maxLength: MaxLengthOfPrefix).EnsureSubset(categories: __AllowedPrefixCharCategories);
			//
			_prefix = prefix;
		}

		public string Prefix
			=> _prefix;

	}

}