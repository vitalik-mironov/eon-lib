using System.Runtime.Serialization;

namespace Eon.ComponentModel.Dependencies {

	[DataContract]
	public abstract class DependencyHandlerBase
#pragma warning disable CS0618 // Type or member is obsolete
		:Disposable, IDependencyHandler {
#pragma warning restore CS0618 // Type or member is obsolete

		protected DependencyHandlerBase() { }

		protected abstract DependencyResult ResolveDependency(IDependencyResolutionContext context);

		DependencyResult IDependencyHandler.ExecuteResolution(IDependencyResolutionContext context)
			=> ResolveDependency(context: context);

	}

}