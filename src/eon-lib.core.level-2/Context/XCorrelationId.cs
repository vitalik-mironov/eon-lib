using System;
using System.Text.RegularExpressions;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Context {

	public sealed class XCorrelationId
		:IEquatable<XCorrelationId>, IComparable<XCorrelationId> {

		#region Static & constant members

		/// <summary>
		/// Значение: '3'.
		/// </summary>
		public static readonly int DefaultLengthBytes = 3;

		/// <summary>
		/// Значение: '36'.
		/// </summary>
		public static readonly int MaxLength = 36;

		/// <summary>
		/// Значение: '^([\p{L}\p{Nd}\x5f\-]{1,36})$'.
		/// </summary>
		public static readonly string RegexPatternString;

		/// <summary>
		/// Значение: <see cref="RegexPatternString"/>.
		/// </summary>
		public static readonly Regex RegexPattern;

		/// <summary>
		/// Значение: <see cref="StringCompareOp.OrdinalCS"/>.
		/// </summary>
		public static readonly StringCompareOp Comparison = StringCompareOp.OrdinalCS;

		static XCorrelationId() {
			RegexPatternString = @"^([\p{L}\p{Nd}\x5f\-]{1,36})$";
			RegexPattern = new Regex(pattern: RegexPatternString, options: RegexOptions.Compiled);
		}

		public static bool IsValid(string id, out string validationMessage) {
			var locator = typeof(string);
			if (string.IsNullOrEmpty(id)) {
				validationMessage = FormatXResource(locator: locator, subpath: "CanNotNullOrEmpty");
				return false;
			}
			else if (id.Length > MaxLength) {
				validationMessage = FormatXResource(locator: locator, subpath: "TooLong/MaxLength", args: MaxLength.ToString("d"));
				return false;
			}
			else if (!RegexPattern.IsMatch(input: id)) {
				validationMessage = FormatXResource(locator: locator, subpath: "NotMatchRegex", args: new object[ ] { id.FmtStr().G(), RegexPatternString.FmtStr().G() });
				return false;
			}
			else {
				validationMessage = null;
				return true;
			}
		}

		public static bool IsValid(string id) {
			string validationMessage;
			return IsValid(id, out validationMessage);
		}

		public static void EnsureValid(ArgumentUtilitiesHandle<string> id) {
			string validationMessage;
			if (!IsValid(id.Value, out validationMessage))
				throw
					new ArgumentException(message: validationMessage, paramName: id.Name);
		}

		public static void EnsureValid(string id)
			=> EnsureValid(id: id.Arg(nameof(id)));

		public static implicit operator string(XCorrelationId id)
			=> id?._value;

		public static explicit operator XCorrelationId(string id) {
			if (id is null)
				return null;
			XCorrelationId result;
			try {
				result = new XCorrelationId(correlationId: id);
			}
			catch (ArgumentException exception) {
				throw
					new InvalidCastException(
						message: FormatXResource(locator: typeof(InvalidCastException), subpath: "InnerException", args: new object[ ] { typeof(string), typeof(XCorrelationId) }),
						innerException: exception);
			}
			return result;
		}

		public static bool operator ==(XCorrelationId a, XCorrelationId b)
			=> Equals(a, b);

		public static bool operator !=(XCorrelationId a, XCorrelationId b)
			=> !Equals(a, b);

		public static int Compare(XCorrelationId a, XCorrelationId b)
			=> string.CompareOrdinal(a?._value, b?._value);

		public static bool Equals(XCorrelationId a, XCorrelationId b) {
			if (ReferenceEquals(a, b))
				return true;
			else if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
				return false;
			else
				return string.Equals(a._value, b._value, StringComparison.Ordinal);
		}

		public static bool EqualsCI(XCorrelationId a, XCorrelationId b) {
			if (ReferenceEquals(a, b))
				return true;
			else if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
				return false;
			else
				return string.Equals(a._value, b._value, StringComparison.OrdinalIgnoreCase);
		}

		public static bool TryParse(string id, out XCorrelationId result) {
			if (IsValid(id)) {
				result = CreateUnsafe(idString: id);
				return true;
			}
			else {
				result = null;
				return false;
			}
		}

		public static XCorrelationId CreateUnsafe(string idString)
			=> new XCorrelationId() { _value = idString };

		#endregion

		string _value;

		public XCorrelationId(string correlationId) {
			string validationMessage;
			if (!IsValid(correlationId, out validationMessage))
				throw
					new ArgumentException(
						message: $"{FormatXResource(locator: typeof(ArgumentException), subpath: "ValueIsInvalid")}\r\n{validationMessage}",
						paramName: nameof(correlationId));
			_value = correlationId;
		}

		XCorrelationId() { }

		public string Value
			=> _value;

		public int CompareTo(XCorrelationId other)
			=> string.CompareOrdinal(_value, other?._value);

		public override int GetHashCode()
			=> _value.GetHashCode();

		public bool Equals(XCorrelationId other) {
			if (other is null)
				return false;
			else
				return string.Equals(_value, other._value, StringComparison.Ordinal);
		}

		public override bool Equals(object other)
			=> Equals(other: other as XCorrelationId);

		public override string ToString()
			=> _value;

		public static XCorrelationId New()
			=> CreateUnsafe(idString: NewString(prefix: null));

		public static string NewString(string prefix = default) {
			const int randomGuidStringMaxLength = 8;
			const int randomGuidMaxByteLength = 3;
			//
			if (string.IsNullOrEmpty(prefix))
				return GuidUtilities.NewGuidString(count: randomGuidMaxByteLength);
			else {
				var prefixLastChar = prefix.LastChar().Value;
				var randomGuidLength = Math.Min(MaxLength - prefix.Length - (prefixLastChar == '-' ? 0 : 1), randomGuidStringMaxLength);
				if (randomGuidLength > 0) {
					EnsureValid(id: prefix.Arg(nameof(prefix)));
					var randomStringSeed = GuidUtilities.NewGuidString(count: randomGuidMaxByteLength);
					return
						prefixLastChar == '-'
						? prefix + randomStringSeed.Left(length: randomGuidLength)
						: prefix + "-" + randomStringSeed.Left(length: randomGuidLength);
				}
				else
					throw
						new ArgumentException(
							message: FormatXResource(typeof(string), "TooLong/MaxLength", (MaxLength - 2).ToString()),
							paramName: nameof(prefix));
			}
		}


	}

}