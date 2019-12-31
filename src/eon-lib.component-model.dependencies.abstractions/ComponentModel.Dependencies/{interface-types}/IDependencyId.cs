using System;

namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Определяет идентификатор функциональной зависимости.
	/// </summary>
	public interface IDependencyId
		:IAsReadOnly<IDependencyId>, IValidatable, IEquatable<IDependencyId> {

		/// <summary>
		/// Проверяет, соответствует ли указанная функциональная зависимость цели привязки <see cref="IDependencyTarget"/>.
		/// </summary>
		/// <param name="target">Цель привязки функциональной зависимости.</param>
		/// <returns>Значение <see cref="bool"/>.</returns>
		bool IsTargetMatch(IDependencyTarget target);

	}

}