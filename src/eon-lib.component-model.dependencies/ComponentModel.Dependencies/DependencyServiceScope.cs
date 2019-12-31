using Microsoft.Extensions.DependencyInjection;

namespace Eon.ComponentModel.Dependencies {

	public sealed class DependencyServiceScope
		:DependencyScope, IDependencyServiceScope {

		public DependencyServiceScope(IServiceScope serviceScope, bool ownsServiceScope = default)
			: base(outerServiceScope: serviceScope, ownsOuterServiceScope: ownsServiceScope) { }

		public DependencyServiceScope(ArgumentUtilitiesHandle<IServiceScope> serviceScope, bool ownsServiceScope = default)
			: base(outerServiceScope: serviceScope, ownsOuterServiceScope: ownsServiceScope) { }

	}

}