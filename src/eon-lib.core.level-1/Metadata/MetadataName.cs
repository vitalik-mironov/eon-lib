using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Metadata {

	[DataContract]
	public sealed class MetadataName
		:IEquatable<MetadataName>, IComparable<MetadataName> {

		#region Static & constant members

		public const string RegexPattern = @"^([\x5f\p{L}]{1}[\p{L}\p{Nd}\x5f\-]{0,127})$";

		public static readonly Regex Regex;

		public const int MaxLength = 128;

		public const string DefaultNewNamePrefix = "auto";

		public const int MaxNewNameRandomPartLength = 23;

		public const int DefaultNewNameRandomPartLength = MaxNewNameRandomPartLength;

		/// <summary>
		/// Значение: <see cref="System.StringComparer.Ordinal"/>.
		/// </summary>
		public static readonly StringComparer StringComparer = StringComparer.Ordinal;

		/// <summary>
		/// Значение: <see cref="System.StringComparison.Ordinal"/>.
		/// </summary>
		public static readonly StringComparison StringComparison = StringComparison.Ordinal;

		public static implicit operator string(MetadataName name)
			=> name?._value;

		static MetadataName() {
			Regex = new Regex(pattern: RegexPattern, options: RegexOptions.Compiled);
		}

		public static explicit operator MetadataName(string value) {
			if (value is null)
				return null;
			MetadataName result;
			try {
				result = new MetadataName(value);
			}
			catch (ArgumentException ex) {
				throw new InvalidCastException(FormatXResource(typeof(InvalidCastException), "InnerException", typeof(string), typeof(MetadataName)), ex);
			}
			return result;
		}

		public static bool IsValid(string name)
			=> IsValid(name, out _);

		public static bool IsValid(string name, out string validationMessage) {
			if (string.IsNullOrEmpty(name)) {
				validationMessage = FormatXResource(typeof(InvalidMetadataNameException), "NullOrEmpty");
				return false;
			}
			if (name.Length > MaxLength) {
				validationMessage = FormatXResource(typeof(InvalidMetadataNameException), "TooLong", MaxLength, name.Length);
				return false;
			}
			if (!Regex.IsMatch(input: name)) {
				validationMessage = FormatXResource(typeof(InvalidMetadataNameException), "ContainsInvalidChars", name);
				return false;
			}
			else {
				validationMessage = null;
				return true;
			}
		}

		public static void EnsureValid(string name, string argName = default)
			=> EnsureValid(name: name.Arg(name: string.IsNullOrEmpty(argName) ? nameof(name) : argName));

		// TODO: Put strings into the resources.
		//
		public static void EnsureValid(ArgumentUtilitiesHandle<string> name) {
			if (!IsValid(name.Value, out var validationMessage)) {
				var exceptionMessage = (name.IsProp ? $"Указано недопустимое значение свойства '{name.Name}'.{Environment.NewLine}" : string.Empty) + validationMessage;
				throw name.ExceptionFactory?.Invoke(message: exceptionMessage) ?? new ArgumentException(message: exceptionMessage, paramName: name.Name);
			}
		}

		public static bool Equals(MetadataName a, MetadataName b) {
			if (ReferenceEquals(a, b))
				return true;
			else if (a is null || b is null)
				return false;
			else
				return string.Equals(a._value, b._value, StringComparison.Ordinal);
		}

		public static bool EqualsCI(MetadataName a, MetadataName b) {
			if (ReferenceEquals(a, b))
				return true;
			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
				return false;
			return string.Equals(a._value, b._value, StringComparison.OrdinalIgnoreCase);
		}

		public static MetadataName CreateUnsafe(string nameString)
			=> new MetadataName() { _value = nameString };

		/// <summary>
		/// Генерирует имя метаданных (идентификатор) на основе GUID (<see cref="GuidUtilities.NewGuidString"/>).
		/// </summary>
		/// <param name="randomPartLength">
		/// Длина части имени, которая будет сгенерирована на основе GUID.
		/// <para>Не может быть меньше 1 и не может быть больше <see cref="MaxNewNameRandomPartLength"/>.</para>
		/// </param>
		/// <returns>Объект <see cref="MetadataName"/>.</returns>
		public static MetadataName NewName(int randomPartLength)
			=> CreateUnsafe(nameString: NewNameString(prefix: null, randomPartLength: randomPartLength));

		/// <summary>
		/// Генерирует имя метаданных (идентификатор) на основе GUID (<see cref="GuidUtilities.NewGuidString"/>).
		/// </summary>
		/// <returns>Объект <see cref="MetadataName"/>.</returns>
		public static MetadataName NewName()
			=> CreateUnsafe(NewNameString(prefix: null, randomPartLength: 8));

		/// <summary>
		/// Генерирует имя метаданных (идентификатор) на основе GUID (<see cref="GuidUtilities.NewGuidString"/>).
		/// </summary>
		/// <param name="prefix">
		/// Прификс с которым будет сгенерировано имя.
		/// <para>По умолчанию — <see cref="DefaultNewNamePrefix"/>.</para>
		/// <para>Длина префикса не может быть больше, чем разница между <see cref="MaxLength"/> и <see cref="DefaultNewNameRandomPartLength"/>.</para>
		/// </param>
		/// <returns>Объект <see cref="MetadataName"/>.</returns>
		public static MetadataName NewName(string prefix)
			=> CreateUnsafe(nameString: NewNameString(prefix: prefix, randomPartLength: null));

		/// <summary>
		/// Генерирует имя метаданных (идентификатор) на основе GUID (<see cref="GuidUtilities.NewGuidString"/>).
		/// </summary>
		/// <param name="prefix">
		/// Прификс с которым будет сгенерировано имя.
		/// <para>По умолчанию — <see cref="DefaultNewNamePrefix"/>.</para>
		/// <para>Длина префикса не может быть больше, чем разница между <see cref="MaxLength"/> и <paramref name="randomPartLength"/>.</para>
		/// </param>
		/// <param name="randomPartLength">
		/// Длина части имени, которая будет сгенерирована на основе GUID.
		/// <para>По умолчанию — <see cref="MaxNewNameRandomPartLength"/>.</para>
		/// <para>Не может быть меньше 1 и не может быть больше <see cref="MaxNewNameRandomPartLength"/>.</para>
		/// </param>
		/// <returns>Объект <see cref="string"/>.</returns>
		public static string NewNameString(string prefix = default, int? randomPartLength = default) {
			randomPartLength.Arg(nameof(randomPartLength)).EnsureNotLessThan(1).EnsureNotGreaterThan(MaxNewNameRandomPartLength);
			//
			randomPartLength = randomPartLength.HasValue ? randomPartLength : MaxNewNameRandomPartLength;
			if (string.IsNullOrEmpty(prefix))
				return DefaultNewNamePrefix + "_" + GuidUtilities.NewGuidString().Substring(0, randomPartLength.Value);
			else if (prefix.Length > (MaxLength - (randomPartLength.Value + 1)))
				throw
					new ArgumentException(FormatXResource(typeof(string), "TooLong/MaxLength", (MaxLength - (randomPartLength.Value + 1))), nameof(prefix));
			EnsureValid(name: prefix, argName: nameof(prefix));
			return prefix + "_" + GuidUtilities.NewGuidString().Substring(0, randomPartLength.Value);
		}

		public static bool operator ==(MetadataName a, MetadataName b) { return Equals(a, b); }

		public static bool operator !=(MetadataName a, MetadataName b) { return !Equals(a, b); }

		public static bool TryParse(string value, out MetadataName result) {
			if (IsValid(value)) {
				result = CreateUnsafe(value);
				return true;
			}
			else {
				result = null;
				return false;
			}
		}

		public static int Compare(MetadataName a, MetadataName b) {
			if (ReferenceEquals(a, b))
				return 0;
			return string.CompareOrdinal(a?._value, b?._value);
		}

		#endregion

		[DataMember(Name = "Value", IsRequired = true, Order = 1)]
		string _value;

		public MetadataName(string name) {
			string validationMessage;
			if (!IsValid(name: name, validationMessage: out validationMessage))
				throw new ArgumentException(string.Format("{0}\r\n{1}", FormatXResource(typeof(ArgumentException), "ValueIsInvalid"), validationMessage), "metadataName");
			_value = name;
		}

		MetadataName() { }

		public string Value
			=> _value;

		public int CompareTo(MetadataName other)
			=> string.CompareOrdinal(_value, other?._value);

		public bool Equals(MetadataName other) {
			if (other is null)
				return false;
			else
				return string.Equals(_value, other._value, StringComparison.Ordinal);
		}

		public override bool Equals(object obj)
			=> Equals(obj as MetadataName);

		public override int GetHashCode()
			=> _value.GetHashCode();

		public override string ToString()
			=> this;

		[OnDeserialized]
		void P_OnDeserialized(StreamingContext context) {
			if (!IsValid(_value, out var validationMessage))
				throw new InvalidMetadataNameException(validationMessage);
		}

	}

}