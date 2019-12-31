using System.Collections.Generic;

using Eon.Description;

namespace Eon.ComponentModel.Dependencies.Description {

	/// <summary>
	/// Представляет описание (конфигурацию) (см. <seealso cref="IDescription"/>) функциональных зависимостей.
	/// </summary>
	public interface IDependenciesDescription
		:IDescription {

		/// <summary>
		/// Возвращает набор описаний групп байндингов функциональных зависимостей (см. <seealso cref="IDependenciesBindingsGroupDescription"/>), входящих в данное описание.
		/// <para>Не может быть null.</para>
		/// </summary>
		IEnumerable<IDependenciesBindingsGroupDescription> BindingsGroups { get; }

	}

}