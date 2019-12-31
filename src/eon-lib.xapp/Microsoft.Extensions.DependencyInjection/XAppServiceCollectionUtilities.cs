using Eon;
using Eon.Runtime;

namespace Microsoft.Extensions.DependencyInjection {

	public static class XAppServiceCollectionUtilities {

		/// <summary>
		/// Registers singleton factory (see <see cref="IDependencyScopedComponentFactory{TComponent}"/>) for <see cref="DefaultXAppRuntimeService"/> service.
		/// </summary>
		/// <param name="svcs">
		/// Service collection.
		/// <para>Can't be <see langword="null"/></para>
		/// </param>
		public static IServiceCollection AddDefaultXAppRuntimeServiceFactory(this IServiceCollection svcs) {
			svcs.EnsureNotNull(nameof(svcs));
			//
			svcs.AddSingleton(implementationInstance: new DependencyScopedComponentFactory<IXAppRuntimeService>((locSp, locDependencies) => new DefaultXAppRuntimeService(outerServiceProvider: locSp, outerDependencies: locDependencies)));
			svcs.AddSingleton<IDependencyScopedComponentFactory<IXAppRuntimeService>>(implementationInstance: new DelegatedDependencyScopedComponentFactory<IXAppRuntimeService>((locSp, locDependencies) => new DefaultXAppRuntimeService(outerServiceProvider: locSp, outerDependencies: locDependencies)));
			return svcs;
		}

	}

}