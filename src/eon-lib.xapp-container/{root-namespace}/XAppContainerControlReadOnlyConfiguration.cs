using System;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Description;
using Eon.Runtime;

namespace Eon {
	using ICustomXApp = IXApp<ICustomXAppDescription>;
	using ICustomXAppStartupContext = IXAppStartupContext<ICustomXAppDescription>;

	public class XAppContainerControlReadOnlyConfiguration
		:Disposable, IXAppContainerControlConfiguration {

		Func<IContext, Task<IXAppRuntimeService>> _createAppRuntime;

		XAppStartupContextFactory<ICustomXAppDescription> _createAppStartupContext;

		Func<ICustomXAppDescription, ICustomXAppStartupContext, Task> _onAppDescriptionLoaded;

		Func<ICustomXApp, ICustomXAppStartupContext, Task> _onAppCreated;

		Func<ICustomXApp, ICustomXAppStartupContext, Task> _onAppInitialized;

		readonly bool _isAppStartForbidden;

		Func<ICustomXApp, ICustomXAppStartupContext, Task> _onAppStarted;

		public XAppContainerControlReadOnlyConfiguration(
			Func<IContext, Task<IXAppRuntimeService>> createAppRuntime,
			XAppStartupContextFactory<ICustomXAppDescription> createAppStartupContext,
			Func<ICustomXAppDescription, ICustomXAppStartupContext, Task> onAppDescriptionLoaded = default,
			Func<ICustomXApp, ICustomXAppStartupContext, Task> onAppCreated = default,
			Func<ICustomXApp, ICustomXAppStartupContext, Task> onAppInitialized = default,
			bool isAppStartForbidden = default,
			Func<ICustomXApp, ICustomXAppStartupContext, Task> onAppStarted = default) {
			//
			createAppRuntime.EnsureNotNull(nameof(createAppRuntime));
			createAppStartupContext.EnsureNotNull(nameof(createAppStartupContext));
			//
			_createAppRuntime = createAppRuntime;
			_createAppStartupContext = createAppStartupContext;
			_onAppDescriptionLoaded = onAppDescriptionLoaded;
			_onAppCreated = onAppCreated;
			_onAppInitialized = onAppInitialized;
			_isAppStartForbidden = isAppStartForbidden;
			_onAppStarted = onAppStarted;
		}

		public Func<IContext, Task<IXAppRuntimeService>> CreateAppRuntime
			=> ReadDA(ref _createAppRuntime);

		public XAppStartupContextFactory<ICustomXAppDescription> CreateAppStartupContext
			=> ReadDA(ref _createAppStartupContext);

		public virtual Func<ICustomXAppDescription, ICustomXAppStartupContext, Task> OnAppDescriptionLoadedAsync
			=> ReadDA(ref _onAppDescriptionLoaded);

		public virtual Func<ICustomXApp, ICustomXAppStartupContext, Task> OnAppCreatedAsync
			=> ReadDA(ref _onAppCreated);

		public virtual Func<ICustomXApp, ICustomXAppStartupContext, Task> OnAppInitializedAsync
			=> ReadDA(ref _onAppInitialized);

		public virtual bool IsAppStartForbidden {
			get {
				EnsureNotDisposeState();
				return _isAppStartForbidden;
			}
		}

		public virtual Func<ICustomXApp, ICustomXAppStartupContext, Task> OnAppStartedAsync
			=> ReadDA(ref _onAppStarted);

		protected override void Dispose(bool explicitDispose) {
			_createAppRuntime = null;
			_createAppStartupContext = null;
			_onAppDescriptionLoaded = null;
			_onAppCreated = null;
			_onAppInitialized = null;
			_onAppStarted = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}