using System.Runtime.Serialization;

namespace Eon {

	[DataContract]
	public enum ComparisonOperator {

		[EnumMember]
		MoreThan = 0,

		[EnumMember]
		MoreThanOrEqualTo,

		[EnumMember]
		EqualTo,

		[EnumMember]
		LessThanOrEqualTo,

		[EnumMember]
		LessThan

	}

}