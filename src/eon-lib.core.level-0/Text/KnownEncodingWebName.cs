using System.Runtime.Serialization;

namespace Eon.Text {

	[DataContract]
	public enum KnownEncodingWebName {

		[EnumMember]
		UTF8 = 0,

		[EnumMember]
		Cyrillic_Windows,

		[EnumMember]
		Cyrillic_Dos

	}

}