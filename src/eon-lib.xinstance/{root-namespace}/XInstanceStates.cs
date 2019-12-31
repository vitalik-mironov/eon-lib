using System;
using System.Runtime.Serialization;
using Eon.Description;

namespace Eon {

	/// <summary>
	/// Определяет состояния компонента <see cref="IXInstance"/>.
	/// </summary>
	[Flags]
	[DataContract]
	public enum XInstanceStates {

		/// <summary>
		/// Дефолтное значение состояния.
		/// </summary>
		[EnumMember]
		None = 0x0,

		/// <summary>
		/// Компонент создан.
		/// </summary>
		[EnumMember]
		Created = 0x1,

		/// <summary>
		/// Была начата инициализация компонента.
		/// </summary>
		[EnumMember]
		InitializeSent = 0x2,

		/// <summary>
		/// Компонент инициализирован.
		/// </summary>
		[EnumMember]
		Initialized = 0x4,

		/// <summary>
		/// Компоненты выгружен.
		/// </summary>
		[EnumMember]
		Disposed = 0x8,

		/// <summary>
		/// Компонент находится в состоянии сбоя и не должен использоваться. Фактически означает, что компонент не операбелен.
		/// </summary>
		[EnumMember]
		Faulted = 0x10,

		/// <summary>
		/// Компонент не имеет описания (конфигурации) <see cref="IDescription"/>.
		/// </summary>
		[EnumMember]
		NoDescription = 0x20

	}

}