using System;
using System.Collections.Generic;

namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Defines a dependency scope.
	/// </summary>
	public interface IDependencyScope
		:IDisposable {

		IEnumerable<IDependencyResolutionContext> RunningResolutions { get; }

		int MaxCountOfRunningResolutions { get; }

		bool ProhibitNewInstanceRequest { get; }

		IDependencyExporter Exporter { get; }

		IServiceProvider ServiceProvider { get; }

		/// <summary>
		/// Gets an owner of this dependency scope.
		/// <para>Can be <see langword="null"/>.</para>
		/// </summary>
		object Owner { get; }

		bool ResolveDependency<TDependency>(IDependencyResolutionContext context, out TDependency dependency)
			where TDependency : class;

		TDependency ResolveDependency<TDependency>(IDependencyResolutionContext context)
			where TDependency : class;

		/// <summary>
		/// Gets an outer dependency scope, if that exists.
		/// <para>Can return <see langword="null"/>.</para>
		/// </summary>
		IDependencyScope GetOuterScope();

		/// <summary>
		/// Gets resolution model for this dependency scope.
		/// <para>Idempotent. The resolution model creates once. All subsequent calls of this method returns result of the first call.</para>
		/// <para>Can't return <see langword="null"/>.</para>
		/// </summary>
		IDependencyResolutionModel<IDependencyScope> GetResolutionModel();

		IDependencyScope CreateScopeCopy(object copyOwner = default);

		IDependencyScope CreateChildScope(object childOwner = default);

	}

}