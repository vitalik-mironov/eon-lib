using System.Runtime.Serialization;

namespace Eon.Diagnostics {

	[DataContract]
	public enum OperationCompletionStatusCode {

		[EnumMember]
		Undefined = 0,

		[EnumMember]
		Cancel,

		[EnumMember]
		Fault,

		[EnumMember]
		Timeout,

		[EnumMember]
		Success,

		[EnumMember]
		QueueWaitTimeout

	}

}