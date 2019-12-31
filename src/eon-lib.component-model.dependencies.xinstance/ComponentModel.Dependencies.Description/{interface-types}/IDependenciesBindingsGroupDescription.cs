using System.Collections.Generic;

using Eon.Description;

namespace Eon.ComponentModel.Dependencies.Description {

	/// <summary>
	/// Представляет описание (конфигурацию) группы байндингов (связей) функциональных зависимостей.
	/// </summary>
	public interface IDependenciesBindingsGroupDescription
		:IDescription, IAbilityOption {

		/// <summary>
		/// Возвращает набор байндингов поставляемых функциональных зависимостей (см. <seealso cref="IDependencyBinding"/>), входящих в данную группу.
		/// <para>Не может быть null.</para>
		/// </summary>
		IEnumerable<IDependencyBinding> Bindings { get; }

		/// <summary>
		/// Возвращает набор описаний байндингов поставляемых функциональных зависимостей (см. <seealso cref="IDependencyBindingDescriptionBase"/>), входящих в данную группу.
		/// <para>Не может быть null.</para>
		/// </summary>
		IEnumerable<IDependencyBindingDescriptionBase> BindingsDescriptions { get; }

	}

}