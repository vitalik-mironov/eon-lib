using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

using Eon.Threading;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Metadata {

	/// <summary>
	/// The metadata path name. Each component of the path is a metadata name (<see cref="MetadataName"/>). Instance is always immutable.
	/// </summary>
	[DataContract]
	public sealed class MetadataPathName
		:IEquatable<MetadataPathName>, IComparable<MetadataPathName>, IFormattable, ITextViewSupport {

		#region Static & constant members

		public const string RegexPattern = @"^(([_\p{L}]{1}[\p{L}\p{Nd}_\-]{0,127}){1}(\.[_\p{L}]{1}[\p{L}\p{Nd}_\-]{0,127}){0,31})$";

		public const int MaxLength = 4127;

		public const int MaxComponentCount = 32;

		public const char ComponentDelimiter = '.';

		public const int ShortFormatComponentCount = 5;

		static readonly string __ComponentDelimiterAsString;

		public static readonly Regex Regex;

		static readonly Dictionary<string, MetadataPathName> __Cache;

		static readonly PrimitiveSpinLock __CacheSpinLock;

		static MetadataPathName() {
			__ComponentDelimiterAsString = new string(ComponentDelimiter, 1);
			Regex = new Regex(pattern: RegexPattern, options: RegexOptions.Compiled);
			__CacheSpinLock = new PrimitiveSpinLock();
			__Cache = new Dictionary<string, MetadataPathName>(StringComparer.Ordinal);
		}

		static bool P_TryParse(string metadataPathName, out string validationMessage, out string parsedValue, out MetadataName[ ] parsedComponents) {
			if (!(metadataPathName is null)) {
				if (P_TryGetFromCache(metadataPathName, out var fromCacheInstance)) {
					validationMessage = null;
					parsedValue = fromCacheInstance._value;
					parsedComponents = fromCacheInstance._components;
					return true;
				}
			}
			//
			if (IsValid(metadataPathName, out validationMessage)) {
				var components = metadataPathName.Split(ComponentDelimiter);
				if (components.Length > MaxComponentCount) {
					parsedValue = null;
					parsedComponents = null;
					validationMessage = FormatXResource(typeof(InvalidMetadataNameException), "PathNameContainsTooManyComponents", metadataPathName, components.Length, MaxComponentCount);
					return false;
				}
				parsedValue = metadataPathName;
				parsedComponents = new MetadataName[ components.Length ];
				for (var i = 0; i < components.Length; ++i)
					parsedComponents[ i ] = MetadataName.CreateUnsafe(components[ i ]);
				return true;
			}
			else {
				parsedValue = null;
				parsedComponents = null;
				return false;
			}
		}

		public static bool IsValid(string metadataPathName, out string validationMessage) {
			if (string.IsNullOrEmpty(metadataPathName)) {
				validationMessage = FormatXResource(typeof(InvalidMetadataNameException), "NullOrEmpty");
				return false;
			}
			else if (metadataPathName.Length > MaxLength) {
				validationMessage = FormatXResource(typeof(InvalidMetadataNameException), "TooLong", MaxLength, metadataPathName.Length);
				return false;
			}
			else if (!Regex.IsMatch(input: metadataPathName)) {
				validationMessage = FormatXResource(typeof(InvalidMetadataNameException), "ContainsInvalidChars", metadataPathName);
				return false;
			}
			else {
				validationMessage = null;
				return true;
			}
		}

		public static implicit operator string(MetadataPathName name)
			=> name?._value;

		public static implicit operator MetadataPathName(MetadataName name) {
			if (name is null)
				return null;
			else if (P_TryGetFromCache(name.Value, out var result))
				return result;
			else {
				result = new MetadataPathName() { _value = name.Value, _components = new MetadataName[ ] { name } };
				P_PutInCache(result);
				return result;
			}
		}

		static bool P_TryGetFromCache(string metadataPathNameString, out MetadataPathName result) {
			metadataPathNameString.EnsureNotNull(nameof(metadataPathNameString));
			//
			MetadataPathName locResult = null;
			if (__CacheSpinLock.Invoke(() => __Cache.TryGetValue(metadataPathNameString, out locResult))) {
				result = locResult;
				return true;
			}
			else {
				result = null;
				return false;
			}
		}

		static bool P_PutInCache(MetadataPathName instance) {
			instance.EnsureNotNull(nameof(instance));
			//
			return
				__CacheSpinLock
				.Invoke(
					func:
						() => {
							if (__Cache.ContainsKey(instance._value))
								return false;
							else {
								__Cache.Add(instance._value, instance);
								return true;
							}
						});
		}

		public static MetadataPathName operator +(MetadataPathName a, MetadataName b) {
			if (b is null)
				return a;
			else if (a?.IsEmpty != false) {
				if (P_TryGetFromCache(b.Value, out var name))
					return name;
				else {
					name = new MetadataPathName() { _value = name.Value, _components = new MetadataName[ ] { b } };
					P_PutInCache(name);
					return name;
				}
			}
			else {
				if (a._components.Length < MaxComponentCount) {
					MetadataPathName name;
					var stringValue = string.Concat(a._value, __ComponentDelimiterAsString, b.Value);
					if (P_TryGetFromCache(stringValue, out name))
						return name;
					name = new MetadataPathName();
					name._value = stringValue;
					name._components = (MetadataName[ ])a._components?.Clone();
					Array.Resize(ref name._components, name._components.Length + 1);
					name._components[ name._components.Length - 1 ] = b;
					P_PutInCache(name);
					return name;
				}
				else
					throw
						new OverflowException(
							message: FormatXResource(typeof(OverflowException), "InnerException"),
							innerException: new InvalidMetadataNameException(FormatXResource(typeof(InvalidMetadataNameException), "PathNameContainsTooManyComponents", a + ComponentDelimiter + (string)b, a._components.Length + 1, MaxComponentCount)));
			}
		}

		#endregion

		[DataMember(Order = 0, Name = nameof(Value), IsRequired = true)]
		string _value;

		MetadataName[ ] _components;

		MetadataPathName() { }

		public MetadataPathName(ArgumentUtilitiesHandle<string> metadataPathName) {
			string validationMessage;
			if (P_TryParse(metadataPathName, out validationMessage, out _value, out _components))
				P_PutInCache(this);
			else
				throw new ArgumentException(message: $"{FormatXResource(typeof(ArgumentException), "ValueIsInvalid")}{Environment.NewLine}{validationMessage}", paramName: metadataPathName.Name);
		}

		public MetadataPathName(string path)
			: this(metadataPathName: path.Arg(nameof(path))) { }

		public string Value => _value;

		public override string ToString()
			=> ToString(FormatStringUtilities.LongFormatSpecifier, null);

		public string ToString(string formatSpecifier, IFormatProvider formatProvider) {
			if (string.IsNullOrEmpty(formatSpecifier) || string.Equals(formatSpecifier, FormatStringUtilities.LongFormatSpecifier, StringComparison.Ordinal))
				return this;
			else if (string.Equals(formatSpecifier, FormatStringUtilities.ShortFormatSpecifier)) {
				// Последние 4 компонента.
				//
				var result = _components.Length > ShortFormatComponentCount ? FormatStringUtilities.TruncateEllipsis : string.Empty;
				for (var i = Math.Max(0, _components.Length - ShortFormatComponentCount); i < _components.Length; i++)
					result += (result.Length > 0 ? __ComponentDelimiterAsString : string.Empty) + _components[ i ].Value;
				return result;
			}
			else
				throw new ArgumentOutOfRangeException(paramName: FormatXResource(locator: typeof(FormatException), subpath: "ExceptionMessages/InvalidFormatSpecifier", args: formatSpecifier), message: "formatSpecifier");
		}

		string ITextViewSupport.ToShortTextView(IFormatProvider formatProvider)
			=> ToString(FormatStringUtilities.ShortFormatSpecifier, formatProvider);

		string ITextViewSupport.ToLongTextView(IFormatProvider formatProvider)
			=> ToString(FormatStringUtilities.LongFormatSpecifier, formatProvider);

		[OnDeserialized]
		void P_OnDeserialized(StreamingContext context) {
			string validationMessage;
			if (P_TryParse(_value, out validationMessage, out _value, out _components))
				P_PutInCache(this);
			else
				throw new InvalidMetadataNameException(validationMessage);
		}

		public int CompareTo(MetadataPathName other)
			=> string.CompareOrdinal(_value, other?._value);

		public static int Compare(MetadataPathName a, MetadataPathName b) {
			if (ReferenceEquals(a, b))
				return 0;
			return string.CompareOrdinal(a?._value, b?._value);
		}

		public bool Equals(MetadataPathName other) {
			if (ReferenceEquals(null, other))
				return false;
			return string.Equals(_value, other._value, StringComparison.Ordinal);
		}

		public override bool Equals(object obj) => Equals(obj as MetadataPathName);

		public override int GetHashCode() => _value.GetHashCode();

		public static bool Equals(MetadataPathName a, MetadataPathName b) {
			if (ReferenceEquals(a, b))
				return true;
			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
				return false;
			return string.Equals(a._value, b._value, StringComparison.Ordinal);
		}

		/// <summary>
		/// Выполняет парсинг строки <paramref name="metadataPathName"/> и возвращает объект пути метаданных — <seealso cref="MetadataPathName"/>.
		/// </summary>
		/// <param name="metadataPathName">
		/// Входная строка, представляющая путь метаданных.
		/// <para>Может быть null. В этом случае метод возвращает null.</para>
		/// </param>
		/// <returns>Объект <seealso cref="MetadataPathName"/>.</returns>
		public static MetadataPathName Parse(string metadataPathName)
			=>
			Parse(
				metadataPathName: metadataPathName.Arg(nameof(metadataPathName)));

		/// <summary>
		/// Выполняет парсинг строки <paramref name="metadataPathName"/> и возвращает объект пути метаданных — <seealso cref="MetadataPathName"/>.
		/// </summary>
		/// <param name="metadataPathName">
		/// Входная строка, представляющая путь метаданных.
		/// <para>Может быть null. В этом случае метод возвращает null.</para>
		/// </param>
		/// <returns>Объект <seealso cref="MetadataPathName"/>.</returns>
		public static MetadataPathName Parse(ArgumentUtilitiesHandle<string> metadataPathName) {
			if (metadataPathName.Value == null)
				return null;
			else {
				MetadataPathName result;
				if (P_TryGetFromCache(metadataPathName.Value, out result))
					return result;
				else
					return
						new MetadataPathName(
							metadataPathName: metadataPathName);
			}
		}

		public static explicit operator MetadataPathName(string value) {
			if (value == null)
				return null;
			else {
				MetadataPathName result;
				if (P_TryGetFromCache(value, out result))
					return result;
				else {
					try {
						result = new MetadataPathName(value.Arg(nameof(value)));
					}
					catch (ArgumentException exception) {
						throw
							new InvalidCastException(
								message: FormatXResource(locator: typeof(InvalidCastException), subpath: "InnerException", args: new[ ] { typeof(string).FmtStr().G(), typeof(MetadataName).FmtStr().G() }),
							innerException: exception);
					}
					return result;
				}
			}
		}

		public bool IsEmpty
			=> (string.IsNullOrEmpty(_value) || _components is null || _components.Length < 1);

		public int ComponentCount
			=> _components is null ? 0 : _components.Length;

		public IEnumerable<MetadataName> Components {
			get {
				if (!IsEmpty)
					for (var i = 0; i < _components.Length; i++)
						yield return _components[ i ];
			}
		}

		public MetadataName this[ int componentIndex ] {
			get {
				if (componentIndex < 0)
					throw new ArgumentOutOfRangeException(nameof(componentIndex));
				else if (IsEmpty || componentIndex > _components.Length - 1)
					throw new IndexOutOfRangeException();
				else
					return _components[ componentIndex ];
			}
		}

	}

}