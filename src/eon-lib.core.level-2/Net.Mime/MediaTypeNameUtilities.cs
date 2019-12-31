namespace Eon.Net.Mime {

	public static class MediaTypeNameUtilities {

		/// <summary>
		/// Значение: 'application/octet-stream'.
		/// </summary>
		public const string AppOctetStream = "application/octet-stream";

		/// <summary>
		/// Значение: 'application/json'.
		/// </summary>
		public const string AppJson = "application/json";

		/// <summary>
		/// Значение: 'application/xml'.
		/// </summary>
		public const string AppXml = "application/xml";

		/// <summary>
		/// Значение: 'application/gzip'.
		/// </summary>
		public const string AppGZip = "application/gzip";

		/// <summary>
		/// Значение: 'application/zip'.
		/// </summary>
		public const string AppZip = "application/zip";

		/// <summary>
		/// Значение: 'text/xml'.
		/// </summary>
		public const string TextXml = "text/xml";

		/// <summary>
		/// Значение: 'text/csv'.
		/// </summary>
		public const string TextCsv = "text/csv";

		/// <summary>
		/// Значение: 'text/plain'.
		/// </summary>
		public const string TextPlain = "text/plain";

		/// <summary>
		/// Значение: 'application/x-www-form-urlencoded'.
		/// </summary>
		public const string AppFormData = "application/x-www-form-urlencoded";

		/// <summary>
		/// Значение: '160'.
		/// </summary>
		public static readonly int MediaTypeNameDefaultMaxLength = 160;

		/// <summary>
		/// Значение: <see cref="StringCompareOp.OrdinalCI"/>.
		/// </summary>
		public static readonly StringCompareOp Comparison = StringCompareOp.OrdinalCI;

		public static bool IsJsonMediaType(string mediaTypeName)
			=>
			mediaTypeName
			.Arg(nameof(mediaTypeName))
			.EnsureHasMaxLength(MediaTypeNameDefaultMaxLength)
			.EnsureNotNullOrWhiteSpace()
			.Value
			.EqualsOrdinalCI(AppJson);

		public static bool IsXmlMediaType(string mediaTypeName) {
			mediaTypeName
				.Arg(nameof(mediaTypeName))
				.EnsureHasMaxLength(MediaTypeNameDefaultMaxLength)
				.EnsureNotNullOrWhiteSpace();
			//
			return mediaTypeName.EqualsOrdinalCI(AppXml) || mediaTypeName.EqualsOrdinalCI(TextXml);
		}

		/// <summary>
		/// Проверяет, соответствует ли указанный медиа-тип универсальному медиа-типу <see cref="AppOctetStream"/>.
		/// </summary>
		/// <param name="mediaTypeName">
		/// Медиа-тип.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <returns>Значение <see cref="bool"/>.</returns>
		public static bool IsGenericMediaType(string mediaTypeName)
			=> mediaTypeName
			.Arg(nameof(mediaTypeName))
			.EnsureHasMaxLength(MediaTypeNameDefaultMaxLength)
			.EnsureNotNullOrWhiteSpace()
			.Value
			.EqualsOrdinalCI(AppOctetStream);

		public static MediaTypeNameContext MediaTypeName(this string mediaTypeName)
			=> new MediaTypeNameContext(mediaTypeName);

		public static bool IsGeneric(this MediaTypeNameContext mediaTypeName)
			=> mediaTypeName.MediaTypeName.EqualsOrdinalCI(AppOctetStream);

		public static bool IsXml(this MediaTypeNameContext mediaTypeName)
			=> mediaTypeName.MediaTypeName.EqualsOrdinalCI(AppXml) || mediaTypeName.MediaTypeName.EqualsOrdinalCI(TextXml);

		public static bool IsJson(this MediaTypeNameContext mediaTypeName)
			=> mediaTypeName.MediaTypeName.EqualsOrdinalCI(AppJson);

		public static bool Is(this MediaTypeNameContext mediaTypeName, string otherMediaTypeName) {
			otherMediaTypeName
				.EnsureNotNull(nameof(otherMediaTypeName))
				.EnsureHasMaxLength(maxLength: MediaTypeNameDefaultMaxLength);
			//
			return mediaTypeName.MediaTypeName.EqualsOrdinalCI(otherMediaTypeName);
		}

	}

}