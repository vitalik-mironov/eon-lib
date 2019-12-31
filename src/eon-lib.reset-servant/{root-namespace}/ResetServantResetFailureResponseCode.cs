using System.Runtime.Serialization;

namespace Eon {

	[DataContract]
	public enum ResetServantResetFailureResponseCode {

		[EnumMember]
		None = 0,

		[EnumMember]
		RunProgram,

		[EnumMember]
		RunProgramThenShutdownApp

	}

}