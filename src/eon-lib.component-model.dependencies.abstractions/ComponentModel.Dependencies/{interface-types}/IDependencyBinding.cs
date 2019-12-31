namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Представляет связь (байндинг), сопоставляющей идентификатор функциональной зависимости конкретной цели (см. <seealso cref="IDependencyTarget"/>), реализующей функциональную зависимость.
	/// </summary>
	public interface IDependencyBinding
		:IValidatable, IAsReadOnly<IDependencyBinding> {

		/// <summary>
		/// Возвращает идентификатор функциональной зависимости.
		/// </summary>
		IDependencyId DependencyId { get; set; }

		/// <summary>
		/// Возвращает цель, реализующую функциональную зависимость.
		/// </summary>
		IDependencyTarget Target { get; set; }

		IVh<IDependencyHandler2> GetDependencyHandler(object dependencyScopeContext = default);

	}

}