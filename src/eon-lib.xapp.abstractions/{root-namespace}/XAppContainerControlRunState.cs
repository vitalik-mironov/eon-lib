using Eon.Description;
using Eon.Runtime;
using Eon.Runtime.Description;

namespace Eon {

	using ICustomXApp = IXApp<ICustomXAppDescription>;
	using IEonRuntimeApp = IRuntimeXApp<IRuntimeXAppDescription>;

	public sealed class XAppContainerControlRunState {

		readonly IXAppRuntimeService _runtime;

		readonly IEonRuntimeApp _runtimeApp;

		readonly ICustomXApp _app;

		public XAppContainerControlRunState(IXAppRuntimeService runtime, IEonRuntimeApp runtimeApp, ICustomXApp app) {
			runtime.EnsureNotNull(nameof(runtime));
			runtimeApp.EnsureNotNull(nameof(runtimeApp));
			app.EnsureNotNull(nameof(app));
			//
			_runtime = runtime;
			_runtimeApp = runtimeApp;
			_app = app;
		}

		public IXAppRuntimeService Runtime
			=> _runtime;

		public IEonRuntimeApp RuntimeApp
			=> _runtimeApp;

		public ICustomXApp App
			=> _app;

	}

}