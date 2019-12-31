namespace Eon.Net.Mime {

	public readonly struct MediaTypeNameContext {

		public readonly string MediaTypeName;

		internal MediaTypeNameContext(string mediaTypeName) {
			mediaTypeName
				.Arg(nameof(mediaTypeName))
				.EnsureHasMaxLength(MediaTypeNameUtilities.MediaTypeNameDefaultMaxLength)
				.EnsureNotEmptyOrWhiteSpace();
			//
			MediaTypeName = mediaTypeName;
		}

	}

}