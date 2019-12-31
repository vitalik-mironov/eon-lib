namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Представляет цель байндинга (связи) функциональной зависимости (см. <seealso cref="IDependencyBinding"/>).
	/// <para>Цель привязки декларирует как именно (или во что) должна быть разрешена та или иная функциональная зависимость.</para>
	/// </summary>
	public interface IDependencyTarget
		:IAsReadOnly<IDependencyTarget>, IValidatable {

		/// <summary>
		/// Возвращает обработчик функциональной зависимости для данной цели привязки.
		/// <para>В зависимости от реализации, данный метод может возвращать либо новый экземпляр обработчика, либо существующий (или общий). В случае нового экземпляра выгрузка объекта <see cref="IVh{TValue}"/> приведет к выгрузке исполнителя, в ином случае выгрузка исполнителя при выгрузке объекта <see cref="IVh{TValue}"/> не производится.</para>
		/// </summary>
		/// <returns>Объект <see cref="IVh{TValue}"/>.</returns>
		IVh<IDependencyHandler2> GetDependencyHandler(object dependencyScopeContext = null);

	}

}