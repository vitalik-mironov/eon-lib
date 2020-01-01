using System.Collections.Generic;

namespace Eon.Description {

	/// <summary>
	/// Представляет описание (конфигурацию) списка инициализации  Eon-приложения (см. <seealso cref="IXApp{TDescription}"/>).
	/// </summary>
	public interface IXAppInitializationListDescription
		:IDescription, IAbilityOption {

		/// <summary>
		/// Возвращает набор описаний (конфигураций) инициализируемых компонентов.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </summary>
		IEnumerable<IDescription> InitializableItems { get; set; }

	}

}