using System.Runtime.Serialization;

namespace Eon {

	[DataContract]
	public enum TimeUnit
		:int {

		[EnumMember]
		Tick = 0,

		[EnumMember]
		Millisecond,

		[EnumMember]
		Second,

		[EnumMember]
		Minute,

		[EnumMember]
		Hour,

		[EnumMember]
		Day

	}


}