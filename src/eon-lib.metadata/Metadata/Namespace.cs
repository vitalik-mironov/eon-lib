using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Metadata {

	[DataContract]
	public sealed class Namespace
		:MetadataBase, IMetadataNamespace {

		public Namespace(MetadataName name)
			: base(name) { }

		[JsonConstructor]
		Namespace(SerializationContext ctx)
			: base(ctx) { }

	}

}