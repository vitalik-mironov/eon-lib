using System.Runtime.Serialization;

namespace Eon.Threading {

	[DataContract]
	public enum LockAcquisitionFailReason {

		[EnumMember]
		Unknown = 0,

		[EnumMember]
		MaxSizeOfRecursiveLockReached,

		[EnumMember]
		InvalidHints,

		[EnumMember]
		InvalidTimeout,

		[EnumMember]
		MaxSizeOfWaitingQueueReached,

		[EnumMember]
		RepeatedUsageOfCookie,

		[EnumMember]
		TimeoutElapsed,

		[EnumMember]
		RaceCondition,

		[EnumMember]
		NullResource,

		[EnumMember]
		SpinningLimitReached

	}

}