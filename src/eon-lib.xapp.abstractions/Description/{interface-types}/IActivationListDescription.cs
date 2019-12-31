using System.Collections.Generic;

namespace Eon.Description {

	/// <summary>
	/// Определяет описание (конфигурацию) списка авто-активации компонентов <seealso cref="IActivatableXAppScopeInstance"/>.
	/// </summary>
	public interface IActivationListDescription
		:IDescription {

		bool OmitItemsActivation { get; set; }

		/// <summary>
		/// Возвращает набор описаний (конфигураций) активируемых компонентов.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </summary>
		IEnumerable<IDescription> ActivatableItems { get; set; }

	}

}