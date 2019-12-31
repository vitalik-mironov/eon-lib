namespace Eon.Runtime.Options {

	public sealed class XAppRuntimeServiceFactoryOption
		:RuntimeOptionBase {

		#region Static & constant members

		public static IDependencyScopedComponentFactory<IXAppRuntimeService> Require()
			=> RuntimeOptions.Option<XAppRuntimeServiceFactoryOption>.Require().Factory;

		#endregion

		readonly IDependencyScopedComponentFactory<IXAppRuntimeService> _factory;

		public XAppRuntimeServiceFactoryOption(IDependencyScopedComponentFactory<IXAppRuntimeService> factory) {
			factory.EnsureNotNull(nameof(factory));
			//
			_factory = factory;
		}

		public IDependencyScopedComponentFactory<IXAppRuntimeService> Factory
			=> _factory;

	}

}