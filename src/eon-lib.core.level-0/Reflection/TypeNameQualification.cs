using System.Runtime.Serialization;

namespace Eon.Reflection {

	[DataContract]
	public enum TypeNameQualification {

		[EnumMember]
		Type = 0,

		[EnumMember]
		Namespace,

		[EnumMember]
		Assembly

	}

}