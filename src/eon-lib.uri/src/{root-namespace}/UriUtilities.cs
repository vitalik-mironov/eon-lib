using System;
using System.Collections.Specialized;
using System.Web;

using Eon.Collections;

namespace Eon {

	public static class UriUtilities {

		/// <summary>
		/// Value: 'mailto'.
		/// </summary>
		public static readonly string UriSchemeMailto = "mailto";

		/// <summary>
		/// Value: 'https'.
		/// </summary>
		public static readonly string UriSchemeHttps = "https";

		/// <summary>
		/// Value: 'http'.
		/// </summary>

		public static readonly string UriSchemeHttp = "http";

		/// <summary>
		/// Value: 'amqps'.
		/// </summary>
		public static readonly string UriSchemeAmqps = "amqps";

		/// <summary>
		/// Value: 'amqp'.
		/// </summary>
		public static readonly string UriSchemeAmqp = "amqp";

		/// <summary>
		/// Value: 'file'.
		/// </summary>
		public static readonly string UriSchemeFile = "file";

		/// <summary>
		/// Value: 'resource'.
		/// </summary>
		public static readonly string UriSchemeResource = "resource";

		/// <summary>
		/// Value: 'pack'.
		/// </summary>
		public static readonly string UriSchemePack = "pack";

		/// <summary>
		/// Value: 'assembly'.
		/// </summary>
		public static readonly string UriSchemeAsemblyManifestResource = "assembly";

		/// <summary>
		/// Value: 'configuration'.
		/// </summary>
		public static readonly string UriSchemeConfiguration = "configuration";

		/// <summary>
		/// Value: 'urn:undefined'.
		/// </summary>
		public const string UndefinedUriString = "urn:undefined";

		/// <summary>
		/// Value: 'urn:undefined' (см. <seealso cref="UndefinedUriString"/>).
		/// </summary>
		public static readonly Uri UndefinedUri = new Uri(UndefinedUriString, UriKind.Absolute);

		public static bool? IsHttpsUri(this Uri uri)
			=> uri is null ? null : (uri.IsAbsoluteUri ? (bool?)string.Equals(UriSchemeHttps, uri.Scheme, StringComparison.OrdinalIgnoreCase) : false);

		public static bool? IsHttpUri(this Uri uri)
			=> uri is null ? null : (uri.IsAbsoluteUri ? (bool?)string.Equals(UriSchemeHttp, uri.Scheme, StringComparison.OrdinalIgnoreCase) : false);

		public static bool? IsFileUri(this Uri uri)
			=> uri is null ? null : (uri.IsAbsoluteUri ? (bool?)string.Equals(UriSchemeFile, uri.Scheme, StringComparison.OrdinalIgnoreCase) : false);

		public static bool? IsAssemblyManifestResourceUri(this Uri uri)
			=> uri is null ? null : (uri.IsAbsoluteUri ? (bool?)string.Equals(UriSchemeAsemblyManifestResource, uri.Scheme, StringComparison.OrdinalIgnoreCase) : false);

		public static bool? IsConfigurationUri(this Uri uri)
			=> uri is null ? null : (uri.IsAbsoluteUri ? (bool?)string.Equals(UriSchemeConfiguration, uri.Scheme, StringComparison.OrdinalIgnoreCase) : false);

		public static bool? IsAmqpsUri(this Uri uri)
			=> uri is null ? null : (uri.IsAbsoluteUri ? (bool?)string.Equals(UriSchemeAmqps, uri.Scheme, StringComparison.OrdinalIgnoreCase) : false);

		public static bool? IsAmqpUri(this Uri uri)
			=> uri is null ? null : (uri.IsAbsoluteUri ? (bool?)string.Equals(UriSchemeAmqp, uri.Scheme, StringComparison.OrdinalIgnoreCase) : false);

		/// <summary>
		/// Возвращает указанный URI в виде строки, соответствующей стандарту представления URI RFC-3986.
		/// <para>Если указанный URI есть <see langword="null"/>, то метод возвращает <see langword="null"/>.</para>
		/// </summary>
		/// <param name="uri">
		/// URI.
		/// </param>
		/// <returns>Объект <seealso cref="string"/>.</returns>
		public static string ToEscapedString(this Uri uri)
			=> uri is null ? null : Uri.EscapeUriString(stringToEscape: uri.ToString());

		public static string ExpandEnvironmentVariables(this Uri uri) {
			uri.EnsureNotNull(nameof(uri));
			//
			if (uri.IsAbsoluteUri)
				return uri.AbsoluteUri;
			else
				return Environment.ExpandEnvironmentVariables(name: Uri.UnescapeDataString(uri.OriginalString));
		}

		public static string GetHostAndPort(this Uri uri)
			=> GetHostAndPort(uri: uri, format: UriFormat.Unescaped);

		public static string GetHostAndPort(this Uri uri, UriFormat format) {
			uri.EnsureNotNull(nameof(uri)).EnsureAbsolute();
			//
			return uri.GetComponents(components: UriComponents.HostAndPort, format: format);
		}

		public static NameValueCollection GetQueryParameters(this ArgumentUtilitiesHandle<Uri> uri)
			=> HttpUtility.ParseQueryString(query: HttpUtility.UrlDecode(str: uri.EnsureNotNull().EnsureAbsolute().Value.Query));

		public static NameValueCollection GetQueryParameters(this Uri uri)
			=> GetQueryParameters(uri: uri.Arg(nameof(uri)));

		public static T RequireSingleQueryParameter<T>(this ArgumentUtilitiesHandle<Uri> uri, string name, Func<string, T> converter)
			=> GetQueryParameters(uri: uri).ArgProp($"{uri.Name}.{nameof(uri.Value.Query)}").RequireSingleParameter(name: name, converter: converter);

	}

}