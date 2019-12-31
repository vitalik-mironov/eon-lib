using Eon.Reflection;

namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Определяет идентификатор функциональной зависимости посредством именной ссылки на тип (<seealso cref="TypeNameReference"/>).
	/// </summary>
	public interface ITypeDependencyId
		:IDependencyId,
		IAsReadOnly<ITypeDependencyId> {

		/// <summary>
		/// Возвращает или устанавливает именную ссылку на тип (<seealso cref="TypeNameReference"/>), идентифицирующий функциональную зависимость.
		/// </summary>
		TypeNameReference Type { get; set; }

	}

}