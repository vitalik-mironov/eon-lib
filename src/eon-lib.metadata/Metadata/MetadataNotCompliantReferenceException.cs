using System;

using xres = Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Metadata {

	public class MetadataNotCompliantReferenceException
		:MetadataReferenceException {

		readonly string _metadataAsString;

		readonly IMetadata _metadata;

		public MetadataNotCompliantReferenceException(IMetadataReference reference, IMetadata metadata)
			: this(reference, metadata, null, null) { }

		public MetadataNotCompliantReferenceException(IMetadataReference reference, IMetadata metadata, string message, Exception innerException)
			: base(reference, null, string.IsNullOrEmpty(message) ? xres.FormatXResource(typeof(MetadataNotCompliantReferenceException), null, metadata.Fluent().NullCond(i => i.GetType()), reference.Fluent().NullCond(i => i.TargetMetadataType)) : xres.FormatXResource(typeof(MetadataNotCompliantReferenceException), "WithMessage", metadata.Fluent().NullCond(i => i.GetType()), reference.Fluent().NullCond(i => i.TargetMetadataType), message), innerException) {
			_metadata = metadata;
			_metadataAsString = metadata?.ToString();
		}

		public IMetadata Metadata
			=> _metadata;

		public string MetadataAsString
			=> _metadataAsString;

	}

}