using System.Runtime.Serialization;

namespace Eon.Interaction {

	[DataContract]
	public enum InteractionRole {

		[EnumMember]
		Client = 0,

		[EnumMember]
		Host = 1,

		[EnumMember]
		Proxy = 2,

		[EnumMember]
		Monitor = 3,

		[EnumMember]
		Other = int.MaxValue

	}

}