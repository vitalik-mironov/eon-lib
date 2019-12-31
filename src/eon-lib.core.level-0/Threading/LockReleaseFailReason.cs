using System.Runtime.Serialization;

namespace Eon.Threading {

	[DataContract]
	public enum LockReleaseFailReason {

		[EnumMember]
		Unknown = 0,

		[EnumMember]
		InvalidThread,

		[EnumMember]
		InvalidCookie,

		[EnumMember]
		InconsequentRelease

	}

}