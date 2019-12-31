using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Metadata {

	public class MetadataReferenceNotResolvedException
		:MetadataReferenceException {

		public MetadataReferenceNotResolvedException(IMetadataReference reference, IMetadata @base)
			: this(reference, @base, (string)null, (Exception)null) { }

		public MetadataReferenceNotResolvedException(IMetadataReference reference, IMetadata @base, Exception innerException)
			: this(reference, @base, (string)null, innerException) { }

		public MetadataReferenceNotResolvedException(IMetadataReference reference, IMetadata @base, string message, Exception innerException)
			: base(reference, @base, string.IsNullOrEmpty(message) ? FormatXResource(typeof(MetadataReferenceNotResolvedException), null) : message, innerException) {
			this.SetErrorCode(MetadataErrorCodes.Reference.NotResolved);
		}

	}

}