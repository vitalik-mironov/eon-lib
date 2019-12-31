using System;

using Eon.ComponentModel.Dependencies;
using Eon.Description;

namespace Eon {

	public interface IXAppCtorArgs<out TDescription>
		where TDescription : class, IXAppDescription {

		TDescription Description { get; }

		IXAppContainerControl ContainerControl { get; }

		IServiceProvider OuterServiceProvider { get; }

		IDependencySupport OuterDependencies { get; }

	}

}