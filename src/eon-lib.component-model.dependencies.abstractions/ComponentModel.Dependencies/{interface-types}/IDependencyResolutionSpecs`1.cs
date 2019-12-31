namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Определяет спецификацию разрешения функциональной зависимости.
	/// </summary>
	/// <typeparam name="TDependency">Тип функциональной зависимости.</typeparam>
	public interface IDependencyResolutionSpecs<TDependency>
		:IDependencyResolutionSpecs
		where TDependency : class {

		/// <summary>
		/// Возвращает критерий выборки функциональной зависимости.
		/// <para>Критерий выборки используется для согласования результата разрешения зависимости, полученного от одного из исполнителей разрешения (<see cref="IDependencyHandler2"/>), с клиентом зависимости.</para>
		/// <para>Критерий выборки может быть использован, например, когда зависимость должна обладать некоторыми свойствами, известными клиенту.</para>
		/// </summary>
		new IDependencyResolutionSelectCriterion<TDependency> SelectCriterion { get; }

	}

}