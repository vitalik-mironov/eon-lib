using System.Collections.Generic;
using Eon.Reflection;

namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Определяет цель привязки функциональной зависимости как новый экземпляр типа.
	/// </summary>
	public interface INewInstanceDependencyTarget
		:ITypeDependencyTarget, IAsReadOnly<INewInstanceDependencyTarget> {

		/// <summary>
		/// Возвращает набор ссылок (<seealso cref="TypeNameReference"/>) на типы параметров конструктора, который будет использован для создания нового экземпляра функциональной зависимости.
		/// </summary>
		IEnumerable<TypeNameReference> ConstructorSignature { get; }

	}

}