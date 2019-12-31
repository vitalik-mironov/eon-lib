using System;

namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Обработчик запроса функциональной зависимости.
	/// </summary>
	[Obsolete(error: false, message: "Use 'DigitalFlare.ComponentModel.Dependencies.IDependencyHandler2' instead.")]
	public interface IDependencyHandler
		:IDisposable {

		/// <summary>
		/// Выполняет разрешение функциональной зависимости в соответствии с критериями, определенными контекстом разрешения <paramref name="resolutionCtx"/> (<see cref="IDependencyResolutionContext"/>).
		/// <para>Результат разрешения определяется возвратом объекта <see cref="DependencyResult"/>.</para>
		/// </summary>
		/// <param name="resolutionCtx">Контекст разрешения функциональной зависимости.</param>
		/// <returns>Значение <see cref="DependencyResult"/>.</returns>
		DependencyResult ExecuteResolution(IDependencyResolutionContext resolutionCtx);

	}

}