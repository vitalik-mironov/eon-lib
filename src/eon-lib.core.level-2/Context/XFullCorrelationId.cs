using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Eon.Threading;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Context {

	public sealed class XFullCorrelationId
		:IEquatable<XFullCorrelationId>, IComparable<XFullCorrelationId>, IFormattable, ITextViewSupport {

		#region Static members

		static readonly string __ComponentDelimiterAsString;

		static readonly Dictionary<string, XFullCorrelationId> __Cache;

		static readonly PrimitiveSpinLock __CacheSpinLock;

		static readonly bool __UseCache;

		/// <summary>
		/// This app domain correlation ID.
		/// <para>Value: <see cref="XCorrelationId.New"/>.</para>
		/// </summary>
		public static readonly XFullCorrelationId AppDomain;

		/// <summary>
		/// Value: '^(([\p{L}\p{Nd}_\-]{1,36}){1}(\.[\p{L}\p{Nd}_\-]{1,36}){0,127})$'.
		/// </summary>
		public static readonly string RegexPattern;

		/// <summary>
		/// Value: <see cref="RegexPattern"/>.
		/// </summary>
		public static readonly Regex Regex;

		/// <summary>
		/// Value: '4735'.
		/// </summary>
		public static readonly int MaxLength = 4735;

		/// <summary>
		/// Value: '128'.
		/// </summary>
		public static readonly int MaxComponentCount = 128;

		/// <summary>
		/// Value: '.'.
		/// </summary>
		public static readonly char ComponentDelimiter = '.';

		/// <summary>
		/// Value: '8'.
		/// </summary>
		public static readonly int ShortFormatComponentCount = 8;

		/// <summary>
		/// Value: <see cref="XCorrelationId.Comparison"/>.
		/// </summary>
		public static readonly StringCompareOp Comparison = XCorrelationId.Comparison;

		static XFullCorrelationId() {
			__ComponentDelimiterAsString = new string(c: ComponentDelimiter, count: 1);
			__CacheSpinLock = new PrimitiveSpinLock();
			__Cache = new Dictionary<string, XFullCorrelationId>(StringComparer.Ordinal);
			__UseCache = false;
			RegexPattern = @"^(([\p{L}\p{Nd}_\-]{1,36}){1}(\.[\p{L}\p{Nd}_\-]{1,36}){0,127})$";
			Regex = new Regex(pattern: RegexPattern, options: RegexOptions.Compiled);
			AppDomain = XCorrelationId.New();
		}

		static bool P_TryParse(string id, out string validationMessage, out string parsedValue, out XCorrelationId[ ] parsedComponents) {
			if (id != null) {
				if (P_GetFromCache(idString: id, id: out var fromCacheInstance)) {
					validationMessage = null;
					parsedValue = fromCacheInstance._value;
					parsedComponents = fromCacheInstance._components;
					return true;
				}
			}
			//
			if (IsValid(id, out validationMessage)) {
				var components = id.Split(ComponentDelimiter);
				if (components.Length > MaxComponentCount) {
					parsedValue = null;
					parsedComponents = null;
					validationMessage = FormatXResource(typeof(string), "TooManyComponents", MaxComponentCount);
					return false;
				}
				parsedValue = id;
				parsedComponents = new XCorrelationId[ components.Length ];
				for (var i = 0; i < components.Length; ++i)
					parsedComponents[ i ] = XCorrelationId.CreateUnsafe(idString: components[ i ]);
				return true;
			}
			else {
				parsedValue = null;
				parsedComponents = null;
				return false;
			}
		}

		public static bool IsValid(string id, out string validationMessage) {
			if (string.IsNullOrEmpty(id)) {
				validationMessage = FormatXResource(typeof(string), "CanNotNullOrEmpty");
				return false;
			}
			else if (id.Length > MaxLength) {
				validationMessage = FormatXResource(typeof(string), "TooLong/MaxLength", MaxLength.ToString("d"));
				return false;
			}
			else if (!Regex.IsMatch(input: id)) {
				validationMessage = FormatXResource(typeof(string), "NotMatchRegex", id.FmtStr().G(), RegexPattern.FmtStr().G());
				return false;
			}
			else {
				validationMessage = null;
				return true;
			}
		}

		public static implicit operator string(XFullCorrelationId id)
			=> id?._value;

		public static implicit operator XFullCorrelationId(XCorrelationId id) {
			XFullCorrelationId locId;
			if (id is null)
				return null;
			else if (P_GetFromCache(idString: id.Value, id: out locId))
				return locId;
			else {
				locId = new XFullCorrelationId(value: id.Value, components: new XCorrelationId[ ] { id });
				P_PutInCache(locId);
				return locId;
			}
		}

		public static explicit operator XFullCorrelationId(string id) {
			if (id is null)
				return null;
			else {
				if (P_GetFromCache(id, out var locId))
					return locId;
				else {
					try {
						return new XFullCorrelationId(id: id.Arg(nameof(id)));
					}
					catch (ArgumentException exception) {
						throw new InvalidCastException(message: FormatXResource(locator: typeof(InvalidCastException), subpath: "InnerException", args: new[ ] { typeof(string).FmtStr().G(), typeof(XFullCorrelationId).FmtStr().G() }), innerException: exception);
					}
				}
			}
		}

		public static XFullCorrelationId operator +(XFullCorrelationId a, XCorrelationId b) {
			if (ReferenceEquals(b, null))
				return a;
			else if (a?.IsEmpty != false) {
				if (P_GetFromCache(idString: b.Value, id: out var id))
					return id;
				else {
					id = new XFullCorrelationId(value: b.Value, components: new XCorrelationId[ ] { b });
					P_PutInCache(id);
					return id;
				}
			}
			else if (a._components.Length < MaxComponentCount) {
				var stringValue = string.Concat(a._value, __ComponentDelimiterAsString, b.Value);
				if (P_GetFromCache(stringValue, out var id))
					return id;
				else {
					id = new XFullCorrelationId(value: stringValue, components: new XCorrelationId[ a._components.Length + 1 ]);
					Array.Copy(sourceArray: a._components, sourceIndex: 0, destinationArray: id._components, destinationIndex: 0, length: a._components.Length);
					id._components[ id._components.Length - 1 ] = b;
					P_PutInCache(id);
					return id;
				}
			}
			else
				throw
					new OverflowException(
						message: FormatXResource(typeof(OverflowException), "InnerException"),
						innerException: new ArgumentException(message: FormatXResource(typeof(string), "TooManyComponents", MaxComponentCount)));
		}

		static bool P_GetFromCache(string idString, out XFullCorrelationId id) {
			idString.EnsureNotNull(nameof(idString));
			//
			XFullCorrelationId locId = null;
			if (__UseCache && __CacheSpinLock.Invoke(() => __Cache.TryGetValue(key: idString, value: out locId))) {
				id = locId;
				return true;
			}
			else {
				id = null;
				return false;
			}
		}

		static bool P_PutInCache(XFullCorrelationId id) {
			if (__UseCache)
				return
					__CacheSpinLock
					.Invoke(
						func:
							() => {
								if (__Cache.ContainsKey(id._value))
									return false;
								else {
									__Cache.Add(id._value, id);
									return true;
								}
							});
			else
				return false;
		}

		public static int Compare(XFullCorrelationId a, XFullCorrelationId b)
			=> string.CompareOrdinal(a?._value, b?._value);

		public static bool Equals(XFullCorrelationId a, XFullCorrelationId b) {
			if (ReferenceEquals(a, b))
				return true;
			else if (a is null || b is null)
				return false;
			else
				return string.Equals(a._value, b._value, StringComparison.Ordinal);
		}

		public static bool operator ==(XFullCorrelationId a, XFullCorrelationId b)
			=> Equals(a: a, b: b);

		public static bool operator !=(XFullCorrelationId a, XFullCorrelationId b)
			=> !Equals(a: a, b: b);

		/// <summary>
		/// Конвертирует строковое представление пути корреляции (<paramref name="idString"/>) в объект <see cref="XFullCorrelationId"/>.
		/// <para><see langword="null"/>-значение <paramref name="idString"/> конвертируется в <see langword="null"/>.</para>
		/// </summary>
		/// <param name="idString">
		/// Строковое представление пути корреляции.
		/// <para>Может быть <see langword="null"/>.</para>
		/// </param>
		/// <returns>Объект <see cref="XFullCorrelationId"/>.</returns>
		public static XFullCorrelationId Parse(string idString)
			=> Parse(idString: idString.Arg(nameof(idString)));

		/// <summary>
		/// Конвертирует строковое представление пути корреляции (<paramref name="idString"/>) в объект <see cref="XFullCorrelationId"/>.
		/// <para><see langword="null"/>-значение <paramref name="idString"/> конвертируется в <see langword="null"/>.</para>
		/// </summary>
		/// <param name="idString">
		/// Дескриптор аргумента строкового представления пути корреляции.
		/// </param>
		/// <returns>Объект <see cref="XFullCorrelationId"/>.</returns>
		public static XFullCorrelationId Parse(ArgumentUtilitiesHandle<string> idString) {
			if (idString.Value is null)
				return null;
			else {
				XFullCorrelationId path;
				if (P_GetFromCache(idString: idString.Value, out path))
					return path;
				else
					return new XFullCorrelationId(id: idString);
			}
		}

		#endregion

		readonly string _value;

		readonly XCorrelationId[ ] _components;

		XFullCorrelationId() { }

		XFullCorrelationId(string value, XCorrelationId[ ] components) {
			_value = value;
			_components = components;
		}

		public XFullCorrelationId(ArgumentUtilitiesHandle<string> id) {
			if (P_TryParse(id: id, validationMessage: out var validationMessage, parsedValue: out _value, parsedComponents: out _components))
				P_PutInCache(this);
			else
				throw
					new ArgumentException(
						message: $"{FormatXResource(typeof(ArgumentException), "ValueIsInvalid")}{Environment.NewLine}{validationMessage}",
						paramName: id.Name);
		}

		public XFullCorrelationId(string id)
			: this(id: id.Arg(nameof(id))) { }

		public string Value
			=> _value;

		public bool IsEmpty
			=> string.IsNullOrEmpty(_value) || _components is null || _components.Length < 1;

		public int ComponentCount
			=> _components == null ? 0 : _components.Length;

		public IEnumerable<XCorrelationId> Components {
			get {
				if (!IsEmpty)
					for (var i = 0; i < _components.Length; i++)
						yield return _components[ i ];
			}
		}

		public XCorrelationId this[ int componentIndex ] {
			get {
				if (componentIndex < 0)
					throw new ArgumentOutOfRangeException(nameof(componentIndex));
				else if (_components is null || componentIndex > _components.Length - 1)
					throw new IndexOutOfRangeException();
				else
					return _components[ componentIndex ];
			}
		}

		public override string ToString()
			=> ToString(format: FormatStringUtilities.LongFormatSpecifier, provider: null);

		public string ToString(string format, IFormatProvider provider) {
			if (string.IsNullOrEmpty(format) || string.Equals(format, FormatStringUtilities.LongFormatSpecifier, StringComparison.Ordinal))
				return this;
			else if (string.Equals(format, FormatStringUtilities.ShortFormatSpecifier)) {
				var result = _components.Length > ShortFormatComponentCount ? FormatStringUtilities.TruncateEllipsis : string.Empty;
				for (var i = Math.Max(0, _components.Length - ShortFormatComponentCount); i < _components.Length; i++)
					result += (result.Length > 0 ? __ComponentDelimiterAsString : string.Empty) + _components[ i ].Value;
				return result;
			}
			else
				throw new ArgumentOutOfRangeException(message: FormatXResource(typeof(FormatException), "ExceptionMessages/InvalidFormatSpecifier", format), paramName: nameof(format));
		}

		string ITextViewSupport.ToShortTextView(IFormatProvider provider)
			=> ToString(format: FormatStringUtilities.ShortFormatSpecifier, provider: provider);

		string ITextViewSupport.ToLongTextView(IFormatProvider provider)
			=> ToString(format: FormatStringUtilities.LongFormatSpecifier, provider: provider);

		public int CompareTo(XFullCorrelationId other)
			=> string.CompareOrdinal(_value, other?._value);

		public override int GetHashCode()
			=> _value.GetHashCode();

		public bool Equals(XFullCorrelationId other) {
			if (other is null)
				return false;
			else
				return string.Equals(a: _value, b: other._value, comparisonType: StringComparison.Ordinal);
		}

		public override bool Equals(object other)
			=> Equals(other: other as XFullCorrelationId);

	}

}