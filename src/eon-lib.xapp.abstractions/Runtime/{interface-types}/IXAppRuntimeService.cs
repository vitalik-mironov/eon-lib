using System.Threading.Tasks;

using Eon.ComponentModel.Dependencies;
using Eon.Context;
using Eon.Description;
using Eon.Runtime.Description;
using Eon.Threading;

namespace Eon.Runtime {
	using IRuntimeApp = IRuntimeXApp<IRuntimeXAppDescription>;

	public interface IXAppRuntimeService
		:IDisposeNotifying, IDependencySupport {

		bool IsRuntimeAppSupplied { get; }

		IRuntimeApp RuntimeApp { get; }

		IRuntimeApp GetRuntimeAppIfSupplied();

		Task<UpdateResult<IRuntimeApp>> SupplyRuntimeAppAsync(IXAppStartupContext<IRuntimeXAppDescription> startupCtx, XAppStartupContextHostHints? overrideStartupHints = default);

		Task<UpdateResult<IRuntimeApp>> SupplyRuntimeAppAsync(IXAppStartupContext<ICustomXAppDescription> startupCtx, XAppStartupContextHostHints startupHints);

		Task<UpdateResult<IRuntimeApp>> TrySupplyRuntimeAppAsync(IXAppStartupContext<IRuntimeXAppDescription> startupCtx, XAppStartupContextHostHints? overrideStartupHints = default);

		Task<UpdateResult<IRuntimeApp>> TrySupplyRuntimeAppAsync(IXAppStartupContext<ICustomXAppDescription> startupCtx, XAppStartupContextHostHints startupHints);

		Task<TApp> CreateAppAsync<TDescription, TApp>(IXAppCtorArgs<TDescription> args, IContext ctx = default)
			where TDescription : class, IXAppDescription
			where TApp : class, IXApp<TDescription>;

		Task<IXAppStartupContext<TDescription>> CreateAppStartupContextAsync<TDescription>(
			DescriptionLocator locator = default,
			TDescription description = default,
			ArgumentPlaceholder<XAppStartupContextHostHints> hostHints = default,
			ArgumentPlaceholder<IXAppContainerControl> containerControl = default,
			IContext outerCtx = default)
			where TDescription : class, IXAppDescription;

		DescriptionLocator CreateRuntimeAppSatelliteDescriptionLocator(DescriptionPackageLocator locator);

	}

}