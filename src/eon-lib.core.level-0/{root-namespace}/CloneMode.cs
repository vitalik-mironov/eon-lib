using System.Runtime.Serialization;

namespace Eon {

	[DataContract]
	public enum CloneMode {

		[EnumMember]
		Original = 0,

		[EnumMember]
		Root,

		[EnumMember]
		Deep

	}

}