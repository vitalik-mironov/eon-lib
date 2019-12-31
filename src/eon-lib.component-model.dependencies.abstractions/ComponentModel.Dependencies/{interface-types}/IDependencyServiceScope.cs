using Microsoft.Extensions.DependencyInjection;

namespace Eon.ComponentModel.Dependencies {

	public interface IDependencyServiceScope
		:IDependencyScope, IServiceScope { }

}