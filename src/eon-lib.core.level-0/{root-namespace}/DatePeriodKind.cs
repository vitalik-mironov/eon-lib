using System.Runtime.Serialization;

namespace Eon {

	[DataContract]
	public enum DatePeriodKind {

		[EnumMember]
		Any = 0,

		[EnumMember]
		CurrentDay,

		[EnumMember]
		CurrentWeek,

		[EnumMember]
		CurrentMonth,

		[EnumMember]
		CurrentQuarter,

		[EnumMember]
		CurrentHalfOfYear,

		[EnumMember]
		CurrentYear,

		[EnumMember]
		BeginOfWeek,

		[EnumMember]
		BeginOfMonth,

		[EnumMember]
		BeginOfQuarter,

		[EnumMember]
		BeginOfHalfOfYear,

		[EnumMember]
		BeginOfYear,

		[EnumMember]
		PreviousDay,

		[EnumMember]
		PreviousWeek,

		[EnumMember]
		PreviousMonth,

		[EnumMember]
		PreviousQuarter,

		[EnumMember]
		PreviousHalfOfYear,

		[EnumMember]
		PreviousYear,

		[EnumMember]
		NextDay,

		[EnumMember]
		NextWeek,

		[EnumMember]
		NextMonth,

		[EnumMember]
		NextQuarter,

		[EnumMember]
		NextHalfOfYear,

		[EnumMember]
		NextYear

	}

}