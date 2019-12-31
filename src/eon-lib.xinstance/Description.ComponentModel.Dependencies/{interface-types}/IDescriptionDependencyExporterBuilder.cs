using System;

using Eon.ComponentModel.Dependencies;

namespace Eon.Description.ComponentModel.Dependencies {

	/// <summary>
	/// Представляет построитель экспорта функциональных зависимостей описания (конфигурации) (см. <seealso cref="IDescription"/>).
	/// </summary>
	public interface IDescriptionDependencyExporterBuilder
		:IDisposable {

		/// <summary>
		/// Строит и возвращает экспортер функциональных зависимостей (см. <seealso cref="IDependencyExporter"/>) для указанного объекта описания (конфигурации).
		/// </summary>
		/// <param name="description">
		/// Объект описания, для которого строится экспортер функциональных зависимостей.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <returns>Объект <seealso cref="IVh{TValue}"/>.</returns>
		IVh<IDependencyExporter> BuildFor(IDescription description);

	}

}