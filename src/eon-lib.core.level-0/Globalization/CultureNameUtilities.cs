using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Eon.Globalization {

	public static class CultureNameUtilities {

		/// <summary>
		/// Значение: '^([a-z]{2}(-[A-Z]{2})?)$'.
		/// </summary>
		public const string RegexPattern = @"^([a-z]{2}(-[A-Z]{2})?)$";

		/// <summary>
		/// Значение: '^([a-z]{2}(-[A-Z]{2})?)$''.
		/// </summary>
		public static readonly Regex Regex = new Regex(pattern: RegexPattern, options: RegexOptions.Compiled);

		/// <summary>
		/// Значение: <see cref="StringComparer.Ordinal"/>.
		/// </summary>
		public static readonly StringComparer Comparer = StringComparer.Ordinal;

		/// <summary>
		/// Значение: <see cref="StringComparison.Ordinal"/>.
		/// </summary>
		public static readonly StringComparison Comparison = StringComparison.Ordinal;

		public static bool IsValid(string name) {
			name.EnsureNotNull(name: nameof(name));
			//
			return IsInvariant(name: name) || Regex.IsMatch(name);
		}

		public static bool IsInvariant(string name) {
			name.EnsureNotNull(name: nameof(name));
			//
			return string.Equals(name, CultureInfo.InvariantCulture.Name, StringComparison.Ordinal);
		}

	}

}