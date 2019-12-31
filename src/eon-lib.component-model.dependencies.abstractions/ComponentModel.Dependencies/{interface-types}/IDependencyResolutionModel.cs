using System;
using System.Collections.Generic;

namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Определяет программный интерфейс зафиксированной и неизменной модели разрешения функциональных зависимостей.
	/// <para>Модель определяет собой порядок (последовательность) задействования исполнителей разрешения функциональных зависмостей (см. <seealso cref="IDependencyHandler2"/>).</para>
	/// </summary>
	public interface IDependencyResolutionModel
		:IDisposable, IReadOnlyList<IDependencyHandler2> { }

}