using Eon.Description;

namespace Eon.ComponentModel.Dependencies.Description {

	/// <summary>
	/// Представляет описание (конфигурацию) компонента, поддерживающего определение функциональных зависимостей посредством описания <see cref="IDependenciesDescription"/> через свойство описания <see cref="Dependencies"/>.
	/// </summary>
	public interface IDependencySupportDescription
		:IDescription {

		/// <summary>
		/// Возвращает описание поставляемых функциональных зависимостей компонентом, который создан на основе данного описания (или сконфигурирован данным описанием).
		/// <para>Может быть <see langword="null"/>.</para>
		/// </summary>
		IDependenciesDescription Dependencies { get; }

	}

}