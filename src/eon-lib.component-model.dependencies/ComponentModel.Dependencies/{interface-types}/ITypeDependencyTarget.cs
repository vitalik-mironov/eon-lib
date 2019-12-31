using Eon.Reflection;

namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Определяет цель привязки функциональной зависимости как экземпляр типа. Тип указывается именной ссылкой (<seealso cref="TypeNameReference"/>).
	/// </summary>
	public interface ITypeDependencyTarget
		:IDependencyTarget {

		/// <summary>
		/// Возвращает или устанавливает ссылку на тип (см. <seealso cref="TypeNameReference"/>), экземпляр которого определяет цель привязки функциональной зависимости.
		/// </summary>
		TypeNameReference TargetType { get; set; }

	}

}