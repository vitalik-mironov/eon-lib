using System.Runtime.Serialization;

namespace Eon.Diagnostics {

	/// <summary>
	/// Определяет уровни детализации.
	/// </summary>
	[DataContract]
	public enum VerbosityLevel {

		/// <summary>
		/// Соответствует наименьшему уровню детализации.
		/// <para>Данный уровень харктерен для "продакшн"-экплуатации.</para>
		/// <para>Используется по умолчанию.</para>
		/// </summary>
		[EnumMember]
		Low = 0,

		/// <summary>
		/// Соответствует среднему уровню детализации.
		/// </summary>
		[EnumMember]
		Medium,

		/// <summary>
		/// Соответствует наивысшему уровню детализации.
		/// </summary>
		[EnumMember]
		High,

	}

}