using Eon;
using Eon.Description;

namespace Microsoft.Extensions.DependencyInjection {

	public static class DescriptionPackageServiceCollectionUtilities {

		/// <summary>
		/// Registers singleton implementation <see cref="DefaultDescriptionPackageService"/> for service <see cref="IDescriptionPackageService"/>.
		/// </summary>
		/// <param name="svcs">
		/// Service collection.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		public static IServiceCollection AddDefaultDescriptionPackageService(this IServiceCollection svcs) {
			svcs.EnsureNotNull(nameof(svcs));
			//
			svcs.AddSingleton<IDescriptionPackageService, DefaultDescriptionPackageService>(implementationFactory: locOuterSp => new DefaultDescriptionPackageService());
			return svcs;
		}

	}

}