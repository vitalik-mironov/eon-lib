using System.Runtime.Serialization;

using Newtonsoft.Json;

using Eon.Description.Annotations;
using Eon.Metadata;
using Eon.Runtime.Serialization;

namespace Eon.Description {

	[DataContract]
	[XInstanceContract(ContractType = typeof(BlankXApp<ICustomXAppDescription>), Sealed = false)]
	public class BlankXAppDescription
		:CustomXAppDescriptionBase {

		public BlankXAppDescription(MetadataName name)
			: base(name: name) { }

		[JsonConstructor]
		protected BlankXAppDescription(SerializationContext ctx)
			: base(ctx: ctx) { }

	}

}