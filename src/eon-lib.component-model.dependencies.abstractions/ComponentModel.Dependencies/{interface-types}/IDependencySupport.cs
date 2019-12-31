using System.Collections.Generic;

namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Defines a component having own dependency scope (see <see cref="IDependencyScope"/>, <see cref="GetDependencyScope"/>).
	/// </summary>
	public interface IDependencySupport {

		/// <summary>
		/// Gets dependency scope of this component.
		/// <para>Result is immutable. Every method call returns same <see cref="IDependencyScope"/>.</para>
		/// <para>Result can't be <see langword="null"/>.</para>
		/// </summary>
		IDependencyScope GetDependencyScope();

		/// <summary>
		/// Gets set of local dependencies exported by this component.
		/// <para>Set can't contain <see langword="null"/>-item.</para>
		/// </summary>
		IEnumerable<IVh<IDependencyHandler2>> LocalDependencies();

	}

}