using System;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Description;
using Eon.Runtime;

namespace Eon {
	using ICustomXApp = IXApp<ICustomXAppDescription>;
	using ICustomXAppStartupContext = IXAppStartupContext<ICustomXAppDescription>;

	public interface IXAppContainerControlConfiguration
		:IDisposable {

		Func<IContext, Task<IXAppRuntimeService>> CreateAppRuntime { get; }

		XAppStartupContextFactory<ICustomXAppDescription> CreateAppStartupContext { get; }

		Func<ICustomXAppDescription, ICustomXAppStartupContext, Task> OnAppDescriptionLoadedAsync { get; }

		Func<ICustomXApp, ICustomXAppStartupContext, Task> OnAppCreatedAsync { get; }

		Func<ICustomXApp, ICustomXAppStartupContext, Task> OnAppInitializedAsync { get; }

		Func<ICustomXApp, ICustomXAppStartupContext, Task> OnAppStartedAsync { get; }

		bool IsAppStartForbidden { get; }

	}

}