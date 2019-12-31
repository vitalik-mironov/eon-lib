using System.Runtime.Serialization;

namespace Eon.Net.Http {

	[DataContract]
	public enum HandlerMode {

		[EnumMember]
		ClientSide = 0,

		[EnumMember]
		HostSide

	}

}