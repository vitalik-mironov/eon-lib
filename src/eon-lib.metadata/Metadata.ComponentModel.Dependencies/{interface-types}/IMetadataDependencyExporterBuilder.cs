using System;

using Eon.ComponentModel.Dependencies;

namespace Eon.Metadata.ComponentModel.Dependencies {

	/// <summary>
	/// Построитель экспорта функциональных зависимостей объекта метаданных (см. <seealso cref="IMetadata"/>).
	/// </summary>
	public interface IMetadataDependencyExporterBuilder
		:IDisposable {

		/// <summary>
		/// Строит и возвращает экспортер функциональных зависимостей (см. <seealso cref="IDependencyExporter"/>) для указанного объекта метаданных.
		/// </summary>
		/// <param name="metadata">
		/// Объект метаданных, для которого строится экспортер функциональных зависимостей.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <returns>Объект <seealso cref="IVh{TValue}"/>.</returns>
		IVh<IDependencyExporter> BuildFor(IMetadata metadata);

	}

}