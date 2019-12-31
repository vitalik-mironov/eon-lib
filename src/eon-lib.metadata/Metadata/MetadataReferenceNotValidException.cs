using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Metadata {

	public class MetadataReferenceNotValidException
		:MetadataReferenceException {

		public MetadataReferenceNotValidException(MetadataReference metadataRef)
			: this(metadataRef, (string)null, (Exception)null) { }

		public MetadataReferenceNotValidException(MetadataReference metadataRef, string message)
			: this(metadataRef, message, (Exception)null) { }

		public MetadataReferenceNotValidException(MetadataReference metadataRef, string message, Exception innerException)
			: base(metadataRef, (MetadataBase)null, string.IsNullOrEmpty(message) ? FormatXResource(locator: typeof(MetadataReferenceNotValidException), subpath: null) : FormatXResource(typeof(MetadataReferenceNotValidException), "WithMessage", message), innerException) { }

	}

}