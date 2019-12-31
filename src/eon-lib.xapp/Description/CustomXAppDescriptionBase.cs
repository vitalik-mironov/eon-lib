using System.Runtime.Serialization;
using Eon.Metadata;
using Eon.Runtime.Serialization;

namespace Eon.Description {

	[DataContract]
	public abstract class CustomXAppDescriptionBase
		:XAppDescriptionBase, ICustomXAppDescription {

		protected CustomXAppDescriptionBase(MetadataName name)
			: base(name: name) { }

		protected CustomXAppDescriptionBase(SerializationContext ctx)
			: base(ctx: ctx) { }

	}

}