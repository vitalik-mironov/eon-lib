using System.Runtime.Serialization;

using Eon.Description.Annotations;
using Eon.Runtime.Serialization;
using Newtonsoft.Json;

namespace Eon.Description {

	[DataContract]
	[XInstanceContract(ContractType = typeof(XAppPostStartActivationList<IActivationListDescription>), Sealed = false)]
	public class XAppPostStartActivationListDescription
		:ActivationListDescription {

		public XAppPostStartActivationListDescription(Metadata.MetadataName name)
			: base(name: name) { }

		[JsonConstructor]
		protected XAppPostStartActivationListDescription(SerializationContext ctx)
			: base(ctx: ctx) { }

	}

}