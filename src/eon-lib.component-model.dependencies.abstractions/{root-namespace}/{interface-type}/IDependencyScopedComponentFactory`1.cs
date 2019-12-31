using System;
using Eon.ComponentModel.Dependencies;

namespace Eon {

	public interface IDependencyScopedComponentFactory<out TComponent>
		:IFactory<IServiceProvider, IDependencySupport, TComponent>
		where TComponent : class { }

}