using System.Threading.Tasks;

using Eon.Context;
using Eon.Description;
using Eon.Runtime.Description;
using Eon.Threading;

namespace Eon.Runtime {
	using ICustomXApp = IXApp<ICustomXAppDescription>;

	public interface IRuntimeXApp<out TDescription>
		:IXApp<TDescription>
		where TDescription : class, IRuntimeXAppDescription {

		IXAppRuntimeService Runtime { get; }

		bool IsHostsApp { get; }

		ICustomXApp HostedApp { get; }

		ICustomXApp GetHostedAppIfExists();

		Task<UpdateResult<TApp>> HostAppAsync<TAppDescription, TApp>(IXAppStartupContext<TAppDescription> startupCtx, XAppStartupContextHostHints? overrideStartupHints = default)
			where TAppDescription : class, ICustomXAppDescription
			where TApp : class, IXApp<TAppDescription>;

	}

}