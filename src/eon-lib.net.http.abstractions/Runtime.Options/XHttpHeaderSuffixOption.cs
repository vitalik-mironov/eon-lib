using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Eon.Runtime.Options {

	public sealed class XHttpHeaderSuffixOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value: '8'.
		/// </summary>
		public static readonly int MaxLengthOfSuffix;

		static readonly UnicodeCategory[ ] __AllowedSuffixCharCategories;

		static readonly IReadOnlyList<UnicodeCategory> __AllowedSuffixCharCategoriesReadOnlyWrap;

		/// <summary>
		/// Value: '<see cref="UnicodeCategory.DecimalDigitNumber"/>, <see cref="UnicodeCategory.LowercaseLetter"/>, <see cref="UnicodeCategory.UppercaseLetter"/>'.
		/// </summary>
		public static IReadOnlyList<UnicodeCategory> AllowedCharCategoriesOfSuffix
			=> __AllowedSuffixCharCategoriesReadOnlyWrap;

		/// <summary>
		/// Value: <see cref="string.Empty"/>.
		/// </summary>
		public static readonly XHttpHeaderSuffixOption Fallback;

		static XHttpHeaderSuffixOption() {
			MaxLengthOfSuffix = 8;
			__AllowedSuffixCharCategories =
				new UnicodeCategory[ ] {
					UnicodeCategory.DecimalDigitNumber,
					UnicodeCategory.LowercaseLetter,
					UnicodeCategory.UppercaseLetter,
				};
			__AllowedSuffixCharCategoriesReadOnlyWrap = __AllowedSuffixCharCategories.AsCountableEnumerable();
			//
			Fallback = new XHttpHeaderSuffixOption(suffix: string.Empty);
			RuntimeOptions.Option<XHttpHeaderSuffixOption>.SetFallback(option: Fallback);
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static string Require()
			=> RuntimeOptions.Option<XHttpHeaderSuffixOption>.Require().Suffix;

		#endregion

		readonly string _suffix;

		public XHttpHeaderSuffixOption(string suffix) {
			suffix.EnsureNotNull(nameof(suffix)).EnsureHasMaxLength(maxLength: MaxLengthOfSuffix).EnsureSubset(categories: __AllowedSuffixCharCategories);
			//
			_suffix = suffix;
		}

		public string Suffix
			=> _suffix;

	}

}