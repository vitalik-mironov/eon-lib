using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel;
using Eon.ComponentModel.Dependencies;
using Eon.Context;
using Eon.Description;
using Eon.Runtime;
using Eon.Runtime.Options;
using Eon.Threading;
using Eon.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Hosting {

	using ICustomXAppStartupContext = IXAppStartupContext<ICustomXAppDescription>;

	/// <summary>
	/// Default implementation of hosted service, running XApp. See also <see cref="IXAppHostedService"/>.
	/// </summary>
	public partial class DefaultXAppHostedService
		:DisposeNotifying, IXAppHostedService {

		#region Nested types

		sealed class P_RunStateObject {

			public readonly XAppContainerControl ContainerControl;

			internal P_RunStateObject(XAppContainerControl containerControl) {
				containerControl.EnsureNotNull(nameof(containerControl));
				//
				ContainerControl = containerControl;
			}

		}

		#endregion

		IServiceProvider _serviceProvider;

		DescriptionLocator _locator;

		IDependencyServiceScope _dependencyScope;

		RunControl<DefaultXAppHostedService> _runControl;

		IHostApplicationLifetime _applicationLifetime;

		P_RunStateObject _runState;

		public DefaultXAppHostedService(IServiceProvider serviceProvider, DescriptionLocator locator = default) {
			serviceProvider.EnsureNotNull(nameof(serviceProvider));
			//
			_serviceProvider = serviceProvider;
			_locator = locator;
			_runControl = new RunControl<DefaultXAppHostedService>(component: this, options: RunControlOptions.SingleStart, attemptState: null, beforeStart: null, start: DoStartAsync, stop: DoStopAsync, stopToken: default);
		}

		protected IServiceProvider ServiceProvider
			=> ReadDA(location: ref _serviceProvider);

		public IRunControl<DefaultXAppHostedService> RunControl
			=> ReadDA(location: ref _runControl);

		// TODO: Put strings into the resources.
		//
		P_RunStateObject P_RunState {
			get {
				var state = ReadDA(ref _runState);
				if (state is null)
					throw new EonException(message: $"Сomponent has not yet been started or has already been stopped.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
				else
					return state;
			}
		}

		protected IDependencyServiceScope RequireDependencyScope()
			=> InitDA(location: ref _dependencyScope, factory: CreateDependencyScope);

		protected IServiceScope RequireServiceScope()
			=> RequireDependencyScope();

		protected IDependencyServiceScope CreateDependencyScope() {
			CreateDependencyScope(scope: out var scope);
			return scope;
		}

		protected virtual void CreateDependencyScope(out IDependencyServiceScope scope) {
			var serviceScope = default(IServiceScope);
			try {
				serviceScope = ServiceProvider.CreateScope();
				scope = new DependencyServiceScope(serviceScope: serviceScope, ownsServiceScope: true);
			}
			catch {
				serviceScope?.Dispose();
				throw;
			}
		}

		protected IHostApplicationLifetime RequireHostAppLifetime()
			=> InitDA(location: ref _applicationLifetime, factory: () => RequireServiceScope().ServiceProvider.GetRequiredService<IHostApplicationLifetime>());

		protected virtual async Task<IXAppRuntimeService> CreateXAppRuntimeServiceAsync(IContext ctx = default) {
			var sp = RequireServiceScope().ServiceProvider;
			var factory1 = sp.GetService<IDependencyScopedComponentFactory<IXAppRuntimeService>>();
			if (factory1 is null) {
				var factory2 = sp.GetService<DependencyScopedComponentFactory<IXAppRuntimeService>>();
				if (factory2 is null) {
					var factory3 = sp.GetService<IFactory<IXAppRuntimeService>>();
					if (factory3 is null)
						return await XAppRuntimeServiceFactoryOption.Require().CreateAsync(arg1: sp, arg2: null, ctx: ctx).Unwrap().ConfigureAwait(false);
					else
						return await factory3.CreateAsync(ctx: ctx).Unwrap().ConfigureAwait(false);
				}
				else
					return factory2(serviceProvider: sp, dependencies: null);
			}
			else
				return await factory1.CreateAsync(arg1: sp, arg2: null, ctx: ctx).Unwrap().ConfigureAwait(false);
		}

		// TODO: Put strings into the resources.
		//
		protected virtual async Task<ICustomXAppStartupContext> CreateXAppStartupContextAsync(
			IXAppRuntimeService runtime,
			DescriptionLocator locator = default,
			ICustomXAppDescription description = default,
			ArgumentPlaceholder<XAppStartupContextHostHints> hostHints = default,
			ArgumentPlaceholder<IXAppContainerControl> containerControl = default,
			IContext outerCtx = default) {
			//
			runtime.EnsureNotNull(nameof(runtime));
			//
			if (locator is null && description is null) {
				locator = ReadDA(location: ref _locator);
				if (locator is null) {
					var sp = RequireServiceScope().ServiceProvider;
					var settingsOptions = sp.GetService<IOptions<CustomXAppDescriptionLocatorSettings>>();
					var settings = settingsOptions?.Value;
					if (!(settings is null || settings.IsDisabled)) {
						try {
							settings.Validate();
						}
						catch (Exception exception) {
							throw new EonException(message: $"Settings defined by the options (type '{typeof(IOptions<CustomXAppDescriptionLocatorSettings>)}') is not valid.{Environment.NewLine}\tOptions:{settingsOptions.FmtStr().GNLI2()}{Environment.NewLine}\tSettings:{settings.FmtStr().GNLI2()}", innerException: exception);
						}
						locator = settings.GetLocator();
					}
					locator = locator ?? CustomXAppDescriptionLocatorOption.Require();
				}
			}
			return await runtime.CreateAppStartupContextAsync(locator: locator, description: description, hostHints: hostHints, containerControl: containerControl, outerCtx: outerCtx).ConfigureAwait(false);
		}

		async Task IHostedService.StartAsync(CancellationToken ct) {
			if (ct.CanBeCanceled) {
				ct.ThrowExceptionIfCancellationRequested();
				using (var ctx = ContextUtilities.Create(ct: ct))
					await RunControl.StartAsync(ctx: ctx).ConfigureAwait(false);
			}
			else
				await RunControl.StartAsync(ctx: null).ConfigureAwait(false);
		}

		async Task IHostedService.StopAsync(CancellationToken ct)
			=> await RunControl.StopAsync(ctx: null, finiteStop: true).WaitResultAsync(ct: ct).ConfigureAwait(false);

		// TODO: Put strings into the resources.
		//
		protected virtual async Task DoStartAsync(IRunControlAttemptState state) {
			state.EnsureNotNull(nameof(state));
			//
#if DEBUG
			var sp = RequireServiceScope().ServiceProvider;
			var config = sp.GetRequiredService<IConfiguration>();
			Debugger.Break();
#endif
			var container = default(XAppContainerControl);
			var containerConfig = default(XAppContainerControlReadOnlyConfiguration);
			var runState = default(P_RunStateObject);
			try {
				containerConfig = new XAppContainerControlReadOnlyConfiguration(createAppRuntime: CreateXAppRuntimeServiceAsync, createAppStartupContext: CreateXAppStartupContextAsync);
				container = new XAppContainerControl(configuration: containerConfig, ownsConfiguration: true);
				runState = new P_RunStateObject(containerControl: container);
				if (UpdDAIfNullBool(location: ref _runState, value: runState))
					await runState.ContainerControl.StartAsync(ctx: state.Context).ConfigureAwait(false);
				else
					throw new EonException(message: $"Invalid operation. Component start has previously been completed.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
			}
			catch (Exception exception) {
				itrlck.SetNull(location: ref _runState, comparand: runState);
				container?.Dispose(exception);
				containerConfig?.Dispose(exception);
				throw;
			}
		}

		protected virtual async Task DoStopAsync(IRunControlAttemptState state) {
			state.EnsureNotNull(nameof(state));
			//
			var runState = ReadDA(location: ref _runState);
			if (!(runState is null)) {
				var caughtExceptions = new List<Exception>();
				try {
					await runState.ContainerControl.ShutdownAsync().ConfigureAwait(false);
				}
				catch (Exception exception) {
					caughtExceptions.Add(exception);
				}
				itrlck.SetNullBool(location: ref _runState, comparand: runState);
				if (caughtExceptions.Count > 0)
					throw new AggregateException(innerExceptions: caughtExceptions);
			}
		}

		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose)
				TryReadDA(location: ref _runControl)?.StopAsync(finiteStop: true).WaitWithTimeout();
			//
			base.FireBeforeDispose(explicitDispose);
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_runControl?.Dispose();
			}
			_runState = null;
			_runControl = null;
			_dependencyScope = null;
			_applicationLifetime = null;
			_serviceProvider = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}