using Eon.Description;

namespace Eon.Runtime.Options {

	public sealed class DescriptionPackageServiceFactoryOption
		:RuntimeOptionBase {

		#region Static & constant members

		public static IDependencyScopedComponentFactory<IDescriptionPackageService> Require()
			=> RuntimeOptions.Option<DescriptionPackageServiceFactoryOption>.Require().Factory;

		#endregion

		readonly IDependencyScopedComponentFactory<IDescriptionPackageService> _factory;

		public DescriptionPackageServiceFactoryOption(IDependencyScopedComponentFactory<IDescriptionPackageService> factory) {
			factory.EnsureNotNull(nameof(factory));
			//
			_factory = factory;
		}

		public IDependencyScopedComponentFactory<IDescriptionPackageService> Factory
			=> _factory;

	}

}