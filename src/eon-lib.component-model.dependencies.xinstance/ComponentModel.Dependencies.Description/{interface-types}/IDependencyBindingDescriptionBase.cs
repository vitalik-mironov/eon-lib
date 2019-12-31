using System.Collections.Generic;

using Eon.Description;

namespace Eon.ComponentModel.Dependencies.Description {

	/// <summary>
	/// Представляет базовое описание (конфигурацию) связи (байндинга) поставляемой функциональной зависимости (см. <seealso cref="IDependencyBinding"/>).
	/// </summary>
	public interface IDependencyBindingDescriptionBase
		:IDescription {

		/// <summary>
		/// Возвращает набор идентификаторов функциональной зависимости.
		/// <para>Не может быть null.</para>
		/// <para>Не может быть пустым набором.</para>
		/// </summary>
		IEnumerable<IDependencyId> DependencyIdSet { get; }

		IVh<IDependencyHandler2> GetDependencyHandler(object dependencyScopeContext = null);

	}

}