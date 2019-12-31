using System;
using Eon.ComponentModel.Dependencies;

namespace Eon {

	public delegate TComponent DependencyScopedComponentFactory<out TComponent>(IServiceProvider serviceProvider = default, IDependencySupport dependencies = default);

}