using System.Runtime.Serialization;

namespace Eon.Diagnostics {

	/// <summary>
	/// Определяет степени серьёзности.
	/// </summary>
	[DataContract]
	public enum SeverityLevel {

		/// <summary>
		/// Наименьшая.
		/// </summary>
		[EnumMember]
		Lowest = 0,

		/// <summary>
		/// Низкая.
		/// </summary>
		[EnumMember]
		Low,

		/// <summary>
		/// Средняя.
		/// </summary>
		[EnumMember]
		Medium,

		/// <summary>
		/// Высокая.
		/// </summary>
		[EnumMember]
		High,

		/// <summary>
		/// Наивысшая.
		/// </summary>
		[EnumMember]
		Highest

	}

}