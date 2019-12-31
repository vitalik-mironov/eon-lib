using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Metadata {

	[DataContract]
	public abstract class MetadataInclusion
		:MetadataBase {

		[DataMember(Name = nameof(MetadataReference), IsRequired = true, Order = 0)]
		MetadataReference _metadataReference;

		protected MetadataInclusion(MetadataReference metadataReference)
			: base(name: metadataReference.EnsureNotNull(nameof(metadataReference)).Value.TargetMetadataName) {
			_metadataReference = metadataReference;
		}

		[JsonConstructor]
		protected MetadataInclusion(SerializationContext ctx)
			: base(ctx: ctx) { }

		public MetadataReference MetadataReference
			=> ReadDA(ref _metadataReference);

		protected override void Dispose(bool explicitDispose) {
			_metadataReference = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}