using System;
using System.Collections.Generic;

namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Defines a component exporting dependencies.
	/// </summary>
	public interface IDependencyExporter
		:IDisposable {

		/// <summary>
		/// Returns the chain of dependency resolution handlers (see <see cref="IDependencyHandler2"/>).
		/// <para>Each handler represents one or more exported dependencies (services).</para>
		/// <para>Note about handlers sequential order. First handler wins on requested dependency resolution.</para>
		/// <para>Chain can't contain <see langword="null"/>-item.</para>
		/// </summary>
		IEnumerable<IVh<IDependencyHandler2>> ExportDependencies();

	}

}