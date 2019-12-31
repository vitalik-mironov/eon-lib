using System;
using System.Net.Http.Headers;
using System.Text;
using Eon.Runtime.Options;

namespace Eon.Net.Http.Headers {

	public static class HeaderUtilities {

		/// <summary>
		/// Value: '96'.
		/// </summary>
		public static readonly int ConventionalMaxLengthOfHeaderName = 96;

		/// <summary>
		/// Value: '16384'.
		/// </summary>
		public static readonly int ConventionalMaxLengthOfHeaderValue = 16384;

		/// <summary>
		/// Value: 'Authorization'.
		/// </summary>
		public const string AuthorizationHeaderName = "Authorization";

		/// <summary>
		/// Value: 'Content-Length'.
		/// </summary>
		public const string ContentLengthHeaderName = "Content-Length";

		/// <summary>
		/// Value: 'Content-Encoding'.
		/// </summary>
		public const string ContentEncodingHeaderName = "Content-Encoding";

		/// <summary>
		/// Value: 'Expires'.
		/// </summary>
		public const string ExpiresHeaderName = "Expires";

		/// <summary>
		/// Gets the default X-header suffix set by option <see cref="XHttpHeaderSuffixOption"/> (see <see cref="RuntimeOptions"/>).
		/// </summary>
		public static string DefaultXHeaderSuffix
			=> XHttpHeaderSuffixOption.Require();

		public static MediaTypeHeaderValue Copy(this MediaTypeHeaderValue source) {
			if (source is null)
				return null;
			else {
				var copy = new MediaTypeHeaderValue(mediaType: source.MediaType);
				foreach (var sourceParameter in source.Parameters) {
					if (sourceParameter != null)
						copy.Parameters.Add(new NameValueHeaderValue(name: sourceParameter.Name, value: sourceParameter.Value));
				}
				return copy;
			}
		}

		// TODO: Put strings into the resources.
		//
		public static Encoding RequireCharSetEncoding(this MediaTypeHeaderValue type, Encoding fallback = default) {
			var encoding = GetCharSetEncoding(type: type, fallback: fallback);
			if (encoding is null)
				throw new EonException(message: $"No text encoding set in HTTP content type header.{Environment.NewLine}\tContent type:{type.FmtStr().GNLI2()}");
			else
				return encoding;
		}

		public static Encoding GetCharSetEncoding(this MediaTypeHeaderValue type, Encoding fallback = default) {
			type.EnsureNotNull(nameof(type));
			//
			var charSet = type.CharSet;
			if (string.IsNullOrEmpty(charSet))
				return fallback;
			else
				return Encoding.GetEncoding(charSet);
		}

		public static void Set(this HttpHeaders headers, string name, string value) {
			headers.EnsureNotNull(nameof(headers));
			name.EnsureNotNull(nameof(name)).EnsureNotEmpty();
			//
			headers.Remove(name);
			if (!headers.TryAddWithoutValidation(name: name, value: value))
				headers.Add(name: name, value: value);
		}

	}

}