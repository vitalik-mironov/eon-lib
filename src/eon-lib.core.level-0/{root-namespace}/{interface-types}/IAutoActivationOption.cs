
using System;

namespace Eon {

	/// <summary>
	/// Определяет свойство <seealso cref="IsAutoActivationEnabled"/>, декларирующее опцию включения/отключения авто-активации компонента.
	/// </summary>
	public interface IAutoActivationOption {

		/// <summary>
		/// Возвращает признак, указывающий включена ли автоматическая активация для компонента.
		/// </summary>
		bool IsAutoActivationEnabled { get; }

	}

}