using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Metadata {

	public class MetadataNotValidatedYetException
		:MetadataException {

		readonly IMetadata _metadata;

		public MetadataNotValidatedYetException()
			: this(message: null, metadata: null, innerException: null) { }

		public MetadataNotValidatedYetException(IMetadata metadata)
			: this(message: null, metadata: metadata, innerException: null) { }

		public MetadataNotValidatedYetException(string message, IMetadata metadata)
			: this(message, metadata, (Exception)null) { }

		public MetadataNotValidatedYetException(IMetadata metadata, Exception innerException)
			: this(message: null, metadata: metadata, innerException: innerException) { }

		public MetadataNotValidatedYetException(string message, IMetadata metadata, Exception innerException)
			: base(message: string.IsNullOrEmpty(message) ? FormatXResource(typeof(MetadataNotValidatedYetException), "DefaultMessage", metadata) : FormatXResource(typeof(MetadataNotValidatedYetException), "UserMessage", message, metadata), innerException: innerException) {
			_metadata = metadata;
		}

		public IMetadata Metadata => _metadata;

	}

}