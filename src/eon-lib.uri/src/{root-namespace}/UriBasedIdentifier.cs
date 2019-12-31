using System;
using System.Runtime.Serialization;

using Eon.Collections;
using Eon.Collections.Specialized;
using Eon.Runtime.Serialization;

namespace Eon {

	/// <summary>
	/// Представляет идентификатор в форме абсолютного URI, не содержащего информацию пользователя (UserInfo) и данные фрагмента.
	/// </summary>
	[DataContract]
	public sealed class UriBasedIdentifier
		:IEquatable<UriBasedIdentifier> {

		#region Static memebers

		/// <summary>
		/// Value: 'urn:undefined' (see <seealso cref="UriUtilities.UndefinedUri"/>).
		/// </summary>
		public static readonly UriBasedIdentifier Undefined;

		/// <summary>
		/// Value: <see cref="StringComparer.Ordinal"/>.
		/// </summary>
		public static readonly StringComparer Comparer;

		/// <summary>
		/// Value: <see cref="StringComparison.Ordinal"/>.
		/// </summary>
		public static readonly StringComparison Comparison;

		/// <summary>
		/// Value: '2048'.
		/// </summary>
		static readonly int __CacheStringLengthLimit;

		static readonly RingRegistry<string, UriBasedIdentifier> __CacheByString;

		static readonly RingRegistry<Uri, UriBasedIdentifier> __CacheByUri;

		static UriBasedIdentifier() {
			Comparer = StringComparer.OrdinalIgnoreCase;
			Comparison = StringComparison.OrdinalIgnoreCase;
			__CacheStringLengthLimit = 2048;
			__CacheByString = new RingRegistry<string, UriBasedIdentifier>(maxCapacity: 4096, keyComparer: Comparer);
			__CacheByUri = new RingRegistry<Uri, UriBasedIdentifier>(maxCapacity: 4096, keyComparer: ReferenceEqualityComparer<Uri>.Instance);
			Undefined = new UriBasedIdentifier(uri: UriUtilities.UndefinedUri);
		}

		public static implicit operator Uri(UriBasedIdentifier value)
			=> value?._uriValue;

		static bool P_TryGetFromCache(string uriString, out UriBasedIdentifier id) {
			if (uriString is null || uriString.Length > __CacheStringLengthLimit) {
				id = null;
				return false;
			}
			else
				return __CacheByString.Get(key: uriString, value: out id);
		}

		static bool P_TryGetFromCache(Uri uri, out UriBasedIdentifier id) {
			if (uri is null) {
				id = null;
				return false;
			}
			else
				return __CacheByUri.Get(key: uri, value: out id);
		}

		static void P_TryPutInCache(UriBasedIdentifier instance) {
			if (!(instance is null || instance._stringValue.Length > __CacheStringLengthLimit)) {
				__CacheByString.Put(key: instance._stringValue, value: instance);
				__CacheByUri.Put(key: instance._uriValue, value: instance);
			}
		}

		public static bool Equals(UriBasedIdentifier a, UriBasedIdentifier b)
			=> a?._uriValue == b?._uriValue;

		static Uri P_ParseUri(ArgumentUtilitiesHandle<string> argument) {
			Uri uriValue;
			try {
				uriValue = new Uri(uriString: argument.Value, uriKind: UriKind.Absolute);
			}
			catch (Exception exception) {
				throw new ArgumentException(paramName: argument.Name, message: $"Specified string can't be converted to URI object.{Environment.NewLine}\tString:{argument.Value.FmtStr().GNLI2()}", innerException: exception);
			}
			return uriValue.Arg(name: argument.Name).EnsureNoFragmentNoUserInfo();
		}

		public static UriBasedIdentifier Parse(string uriString) {
			if (uriString is null)
				return null;
			else if (P_TryGetFromCache(uriString: uriString, id: out var cached))
				return cached;
			else
				return new UriBasedIdentifier(uriString: uriString);
		}

		public static UriBasedIdentifier Parse(ArgumentUtilitiesHandle<string> uriString) {
			if (uriString.Value is null)
				return null;
			else if (P_TryGetFromCache(uriString: uriString.Value, id: out var cached))
				return cached;
			else
				return new UriBasedIdentifier(uriString: uriString, skipCacheCheck: true);
		}

		public static bool TryParse(string uriString, out UriBasedIdentifier id, out Exception exception) {
			if (uriString is null) {
				id = null;
				exception = null;
				return true;
			}
			else if (P_TryGetFromCache(uriString: uriString, id: out var cached)) {
				id = cached;
				exception = null;
				return true;
			}
			else {
				try {
					id = new UriBasedIdentifier(uriString: uriString);
					exception = null;
					return true;
				}
				catch (Exception locException) {
					id = null;
					exception = locException;
					return false;
				}
			}
		}

		public static bool TryParse(string uriString, out UriBasedIdentifier id)
			=> TryParse(uriString: uriString, id: out id, exception: out _);

		public static bool TryParse(Uri uri, out UriBasedIdentifier id, out Exception exception) {
			if (uri is null) {
				id = null;
				exception = null;
				return true;
			}
			else if (P_TryGetFromCache(uri: uri, id: out var cached)) {
				id = cached;
				exception = null;
				return true;
			}
			else {
				try {
					id = new UriBasedIdentifier(uri: uri);
					exception = null;
					return true;
				}
				catch (Exception locException) {
					id = null;
					exception = locException;
					return false;
				}
			}
		}

		public static bool TryParse(Uri uri, out UriBasedIdentifier id)
			=> TryParse(uri: uri, id: out id, exception: out _);

		#endregion

		string _stringValue;
		[DataMember(Order = 0, Name = "Value", IsRequired = true)]
		string m_Value {
			get => _stringValue;
			set {
				if (P_TryGetFromCache(uriString: value, id: out var cached)) {
					_uriValue = cached._uriValue;
					_stringValue = cached._stringValue;
				}
				else {
					Uri uriValue;
					try {
						uriValue = P_ParseUri(value.Arg(nameof(value)));
					}
					catch (Exception exception) {
						throw new MemberDeserializationException(type: GetType(), memberName: "Value", innerException: exception);
					}
					_uriValue = uriValue;
					_stringValue = uriValue.ToString();
					P_TryPutInCache(instance: this);
				}
			}
		}

		Uri _uriValue;

		public UriBasedIdentifier(Uri uri)
			: this(argument: uri.Arg(nameof(uri))) { }

		public UriBasedIdentifier(ArgumentUtilitiesHandle<Uri> argument) {
			argument.EnsureNotNull().EnsureAbsolute().EnsureNoFragmentNoUserInfo();
			//
			var stringValue = argument.Value.ToString();
			if (P_TryGetFromCache(uriString: stringValue, id: out var cached)) {
				_uriValue = cached._uriValue;
				_stringValue = cached._stringValue;
			}
			else {
				_uriValue = argument.Value;
				_stringValue = stringValue;
				P_TryPutInCache(instance: this);
			}
		}

		public UriBasedIdentifier(string uriString)
			: this(uriString: uriString.Arg(nameof(uriString))) { }

		public UriBasedIdentifier(ArgumentUtilitiesHandle<string> uriString)
			: this(uriString: uriString, skipCacheCheck: false) { }

		UriBasedIdentifier(ArgumentUtilitiesHandle<string> uriString, bool skipCacheCheck) {
			uriString.EnsureNotNullOrWhiteSpace();
			//
			if (!skipCacheCheck && P_TryGetFromCache(uriString: uriString, id: out var cached)) {
				_uriValue = cached._uriValue;
				_stringValue = cached._stringValue;
			}
			else {
				var uriValue = P_ParseUri(uriString);
				_uriValue = uriValue;
				_stringValue = uriValue.ToString();
				P_TryPutInCache(instance: this);
			}
		}

		[Newtonsoft.Json.JsonConstructor]
		UriBasedIdentifier(SerializationContext ctx) { }

		/// <summary>
		/// Возвращает строковое представление данного идентификатора.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </summary>
		public string StringValue
			=> _stringValue;

		/// <summary>
		/// Возвращает объект <seealso cref="Uri"/>, представляющий данный идентификатор.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </summary>
		public Uri UriValue
			=> _uriValue;

		public override int GetHashCode()
			=> _uriValue.GetHashCode();

		public override bool Equals(object other)
			=> Equals(other: other as UriBasedIdentifier);

		public bool Equals(UriBasedIdentifier other)
			=> _uriValue == other?._uriValue;

		public static bool operator ==(UriBasedIdentifier a, UriBasedIdentifier b)
			=> a?._uriValue == b?._uriValue;

		public static bool operator !=(UriBasedIdentifier a, UriBasedIdentifier b)
			=> a?._uriValue != b?._uriValue;

		public override string ToString()
			=> _stringValue;

	}

}