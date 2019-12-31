using System;
using Microsoft.Extensions.DependencyInjection;

namespace Eon.ComponentModel.Dependencies {

	public interface IServiceProviderHandler
		:IDependencyHandler2 {

		IServiceProvider ServiceProvider { get; }

		bool HasServiceScope { get; }

		IServiceScope ServiceScope { get; }

	}

}