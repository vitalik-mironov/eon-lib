namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Построитель экспорта функциональных зависимостей компонента <seealso cref="IXInstance"/>.
	/// </summary>
	public interface IXInstanceDependencyExporterBuilder {

		/// <summary>
		/// Строит и возвращает экспортер функциональных зависимостей (см. <seealso cref="IDependencyExporter"/>) для указанного объекта <seealso cref="IXInstance"/>.
		/// </summary>
		/// <param name="instance">
		/// Объект <seealso cref="IXInstance" />, для которого строится экспортер функциональных зависимостей.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <returns>Объект <seealso cref="IVh{TValue}"/>.</returns>
		IVh<IDependencyExporter> BuildFor(IXInstance instance);

	}

}