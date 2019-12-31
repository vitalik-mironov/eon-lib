using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Eon.ComponentModel;
using Eon.Context;
using Eon.Description;
using Eon.Diagnostics;
using Eon.Runtime;
using Eon.Runtime.Description;
using Eon.Runtime.Options;
using Eon.Threading;
using Eon.Threading.Tasks;

using itrlck = Eon.Threading.InterlockedUtilities;
using vlt = Eon.Threading.VolatileUtilities;

namespace Eon {
	using ICustomXApp = IXApp<ICustomXAppDescription>;
	using IRuntimeApp = IRuntimeXApp<IRuntimeXAppDescription>;

	public class XAppContainerControl
		:DisposeNotifying, IXAppContainerControl {

		#region Static & constant members

		/// <summary>
		/// Value: 'default'.
		/// </summary>
		public static readonly string DefaultLimeProviderTypeToken = "default";

		/// <summary>
		/// Value: 'default'.
		/// </summary>
		public static readonly string DefaultAppDescriptionLocatorToken = "default";

		#endregion

		bool _hasShutdownRequested;

		bool _hasShutdownFinished;

		TaskCompletionSource<Nil> _shutdownOp;

		EventHandler _eventHandler_ShutdownFinished;

		DisposableLazy<IXAppContainerControlConfiguration> _configuration;

		RunControl<XAppContainerControl> _runControl;

		XAppContainerControlRunState _runState;

		IUnhandledExceptionObserver _unhandledExceptionObserver;

		public XAppContainerControl(IUnhandledExceptionObserver unhandledExceptionObserver = default) {
			_configuration =
				new DisposableLazy<IXAppContainerControlConfiguration>(
					factory:
						() => {
							// TODO: Put strings into the resources.
							//
							RequireConfiguration(configuration: out var locConfiguration);
							if (locConfiguration is null)
								throw new EonException(message: $"No configuration provided for this component (see method '{nameof(RequireConfiguration)}').{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
							return locConfiguration;
						},
					ownsValue: false);
			P_CtorInitializer(unhandledExceptionObserver: unhandledExceptionObserver);
		}

		public XAppContainerControl(IXAppContainerControlConfiguration configuration, bool ownsConfiguration = false, IUnhandledExceptionObserver unhandledExceptionObserver = default) {
			configuration.EnsureNotNull(nameof(configuration));
			//
			_configuration = new DisposableLazy<IXAppContainerControlConfiguration>(value: configuration, ownsValue: ownsConfiguration);
			P_CtorInitializer(unhandledExceptionObserver: unhandledExceptionObserver);
		}

		void P_CtorInitializer(IUnhandledExceptionObserver unhandledExceptionObserver = default) {
			_unhandledExceptionObserver = unhandledExceptionObserver ?? UnhandledExceptionObserverOption.Require();
			_runControl = new RunControl<XAppContainerControl>(options: RunControlOptions.SingleStart, component: this, start: DoStartAsync, stop: DoShutdownAsync);
		}

		public bool HasShutdownRequested
			=> vlt.Read(ref _hasShutdownRequested);

		public bool HasShutdownFinished
			=> vlt.Read(ref _hasShutdownFinished);

		public IXAppContainerControlConfiguration Configuration
			=> ReadDA(ref _configuration).Value;

		// TODO: Put strings into the resources.
		//
		public XAppContainerControlRunState RunState {
			get {
				var runState = ReadDA(ref _runState);
				if (runState is null)
					throw new EonException(message: $"Run state object is missing for this component. Start of this component has either not been completed yet or has not been successfully completed.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
				else
					return runState;
			}
		}

		public IUnhandledExceptionObserver UnhandledExceptionObserver
			=> ReadDA(location: ref _unhandledExceptionObserver);

		public event EventHandler ShutdownFinished {
			add { AddEventHandler(ref _eventHandler_ShutdownFinished, value); }
			remove { RemoveEventHandler(ref _eventHandler_ShutdownFinished, value); }
		}

		protected virtual void RequireConfiguration(out IXAppContainerControlConfiguration configuration)
			=> throw new NotImplementedException().SetErrorCode(code: GeneralErrorCodes.Operation.NotImplemented);

		protected virtual async Task DoStartAsync(IRunControlAttemptState state) {
			state.EnsureNotNull(nameof(state));
			//
			var config = Configuration;
			var runtimeSvc = default(IXAppRuntimeService);
			var runtimeApp = default(IRuntimeApp);
			var runtimeAppSupplyResult = UpdateResult<IRuntimeApp>.Default;
			var appStartupContext = default(IXAppStartupContext<ICustomXAppDescription>);
			var appSupplyResult = UpdateResult<ICustomXApp>.Default;
			var app = default(ICustomXApp);
			var newRunState = default(XAppContainerControlRunState);
			var caughtException = default(Exception);
			try {
				var progress = state.Context.Progress();
				var isAppStartForbidden = config.IsAppStartForbidden;
				// Setup Oxy XApp runtime service.
				//
				observeRunControlStopRequest();
				await progress.ReportAsync("Setup app runtime service…").ConfigureAwait(false);
				runtimeSvc = await config.CreateAppRuntime.ArgProp($"{nameof(Configuration)}.{nameof(Configuration.CreateAppRuntime)}").EnsureNotNull().Value(arg: state.Context).ConfigureAwait(false);
				// Create Создание контекста запуска приложения пользователя (custom app).
				//
				observeRunControlStopRequest();
				await progress.ReportAsync("Create app startup context…").ConfigureAwait(false);
				appStartupContext =
					await
					config
					.CreateAppStartupContext
					.ArgProp($"{nameof(Configuration)}.{nameof(Configuration.CreateAppStartupContext)}")
					.EnsureNotNull()
					.Value
					(runtime: runtimeSvc, locator: null, description: null, startupHints: default, containerControl: this, outerCtx: state.Context).ConfigureAwait(false);
				if (appStartupContext is null)
					throw new EonException(message: $"No app startup context created.{Environment.NewLine}\tConfiguration:{config.FmtStr().GNLI2()}{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
				else if (!ReferenceEquals(objA: appStartupContext.ContainerControl, objB: this))
					throw new EonException(message: $"Created app startup context not linked with this component (see property '{nameof(IXAppStartupContext)}.{nameof(IXAppStartupContext.ContainerControl)}').{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
				// Setup Oxy runtime XApp.
				//
				observeRunControlStopRequest();
				await progress.ReportAsync("Setup runtime app…").ConfigureAwait(false);
				runtimeAppSupplyResult = await runtimeSvc.SupplyRuntimeAppAsync(startupCtx: appStartupContext, startupHints: XAppStartupContextHostHints.None).ConfigureAwait(false);
				if (!runtimeAppSupplyResult.IsUpdated)
					throw new EonException(message: $"Runtime app has already been set.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
				runtimeApp = runtimeAppSupplyResult.Current;
				if (!runtimeApp.HasContainerControl || !ReferenceEquals(objA: runtimeApp.ContainerControl, objB: this))
					throw new EonException(message: $"Runtime app not linked with this component (see property '{nameof(IRuntimeApp)}.{nameof(IRuntimeApp.ContainerControl)}').{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
				//
				observeRunControlStopRequest();
				await runtimeApp.InitializeAsync(ctx: appStartupContext).ConfigureAwait(false);
				observeRunControlStopRequest();
				if (!isAppStartForbidden)
					await runtimeApp.RunControl.StartAsync(ctx: appStartupContext).ConfigureAwait(false);
				// Load description of custom XApp.
				//
				observeRunControlStopRequest();
				await progress.ReportAsync("Load app description (configuration)…").ConfigureAwait(false);
				var appDescription = await appStartupContext.LoadDescriptionAsync(ctx: appStartupContext).Unwrap().ConfigureAwait(false);
				observeRunControlStopRequest();
				await config.OnAppDescriptionLoadedAsync.Fluent().NullCondInvoke(arg1: appDescription, arg2: appStartupContext).ConfigureAwait(false);
				// Setup custom XApp.
				//
				observeRunControlStopRequest();
				await progress.ReportAsync("Setup app…").ConfigureAwait(false);
				appSupplyResult = await runtimeApp.HostAppAsync<ICustomXAppDescription, ICustomXApp>(startupCtx: appStartupContext, overrideStartupHints: XAppStartupContextHostHints.None).ConfigureAwait(false);
				if (!appSupplyResult.IsUpdated)
					throw new EonException(message: $"App has already been set.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}{Environment.NewLine}\tApp description:{appDescription.FmtStr().GNLI2()}");
				app = appSupplyResult.Current;
				if (!app.HasContainerControl || !ReferenceEquals(objA: app.ContainerControl, objB: this))
					throw new EonException(message: $"App not linked with this component (see property '{nameof(ICustomXApp)}.{nameof(ICustomXApp.ContainerControl)}').{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
				//
				observeRunControlStopRequest();
				await config.OnAppCreatedAsync.Fluent().NullCondInvoke(arg1: app, arg2: appStartupContext).ConfigureAwait(false);
				// Initialize custom XApp.
				//
				observeRunControlStopRequest();
				await progress.ReportAsync("App initialization…").ConfigureAwait(false);
				await app.InitializeAsync(ctx: appStartupContext).ConfigureAwait(false);
				observeRunControlStopRequest();
				await config.OnAppInitializedAsync.Fluent().NullCondInvoke(arg1: app, arg2: appStartupContext).ConfigureAwait(false);
				// Start custom XApp.
				//
				if (isAppStartForbidden)
					observeRunControlStopRequest();
				else {
					observeRunControlStopRequest();
					await progress.ReportAsync("App start…".ToFormattableString()).ConfigureAwait(false);
					await app.RunControl.StartAsync(ctx: appStartupContext).ConfigureAwait(false);
					observeRunControlStopRequest();
					await config.OnAppStartedAsync.Fluent().NullCondInvoke(arg1: app, arg2: appStartupContext).ConfigureAwait(false);
					observeRunControlStopRequest();
				}
				// Run state.
				//
				newRunState = new XAppContainerControlRunState(runtime: runtimeSvc, runtimeApp: runtimeAppSupplyResult.Current, app: appSupplyResult.Current);
				if (!UpdDAIfNullBool(location: ref _runState, value: newRunState))
					throw new EonException(message: $"Can't change run state of this component. Run state can be set once.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
			}
			catch (Exception exception) {
				caughtException = exception;
				throw;
			}
			finally {
				if (!(caughtException is null)) {
					if (appSupplyResult.IsUpdated)
						await appSupplyResult.Current.ShutdownAppAsync(exception: caughtException).ConfigureAwait(false);
					if (runtimeAppSupplyResult.IsUpdated)
						await runtimeAppSupplyResult.Current.ShutdownAppAsync(caughtException).ConfigureAwait(false);
					runtimeSvc?.Dispose(caughtException);
					itrlck.SetNull(location: ref _runState, comparand: newRunState);
				}
				appStartupContext?.Dispose(caughtException);
			}
			//
			void observeRunControlStopRequest() {
				if (state.RunControl.HasStopRequested || state.Context.IsCancellationRequested())
					throw new OperationCanceledException(message: $"Stop has been requested.{Environment.NewLine}\rComponent:{state.RunControl.Component.FmtStr().GNLI2()}");
			}
		}

		protected virtual async Task DoShutdownAsync() {
			var runState = ReadDA(ref _runState);
			if (!(runState is null)) {
				var caughtExceptions = new List<Exception>();
				// Shutdown custom app.
				//
				try {
					await runState.App.ShutdownAppAsync().ConfigureAwait(false);
				}
				catch (Exception exception) {
					caughtExceptions.Add(exception);
				}
				// Shutdon runtime app.
				//
				try {
					await runState.RuntimeApp.ShutdownAppAsync().ConfigureAwait(false);
				}
				catch (Exception exception) {
					caughtExceptions.Add(exception);
				}
				// Dispose XApp runtime service.
				//
				try {
					runState.Runtime.Dispose();
				}
				catch (Exception exception) {
					caughtExceptions.Add(exception);
				}
				//
				if (caughtExceptions.Count > 0)
					throw new AggregateException(caughtExceptions);
			}
		}

		public async Task StartAsync(IAsyncProgress<IFormattable> progress = default) {
			using (var ctx = ContextUtilities.Create(progress: progress is null ? default : progress.ToValueHolder(ownsValue: false).ArgPlaceholder()))
				await StartAsync(ctx: ctx).ConfigureAwait(false);
		}

		public async Task StartAsync(IContext ctx = default) {
			var runControl = ReadDA(ref _runControl, considerDisposeRequest: true);
			await runControl.StartAsync(ctx: ctx).ConfigureAwait(false);
		}

		public Task ShutdownAsync() {
			if (HasShutdownFinished)
				return Task.CompletedTask;
			else {
				vlt.Write(ref _hasShutdownRequested, true);
				//
				var existingOp = default(TaskCompletionSource<Nil>);
				var newOp = default(TaskCompletionSource<Nil>);
				try {
					existingOp = itrlck.Get(ref _shutdownOp) ?? UpdDAIfNull(location: ref _shutdownOp, value: newOp = new TaskCompletionSource<Nil>(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously));
					//
					if (ReferenceEquals(objA: newOp, objB: existingOp)) {
						if (HasShutdownFinished) {
							itrlck.SetNull(location: ref _shutdownOp, comparand: newOp);
							newOp.SetResult(result: Nil.Value);
						}
						else
							TaskUtilities.RunOnDefaultScheduler(factory: doShutdownAsync, state: newOp).ContinueWithTryApplyResultTo(taskProxy: newOp);
					}
					return existingOp.Task;
				}
				catch (Exception exception) {
					if (newOp?.TrySetException(exception: exception) == true)
						return newOp.Task;
					else
						return Task.FromException(exception);
				}
				finally {
					if (!ReferenceEquals(objA: existingOp, objB: newOp))
						newOp?.TrySetCanceled();
				}
			}
			//
			async Task doShutdownAsync(TaskCompletionSource<Nil> locOp) {
				await BeforeShutdownAsync().ConfigureAwait(false);
				await ReadDA(ref _runControl).StopAsync(finiteStop: true).ConfigureAwait(false);
				var locUnhandledExceptionObserver = ReadDA(location: ref _unhandledExceptionObserver);
				var locShutdownFinishedEventHandler = ReadDA(ref _eventHandler_ShutdownFinished);
				Dispose();
				vlt.Write(location: ref _hasShutdownFinished, value: true);
				itrlck.SetNullBool(location: ref _shutdownOp, comparand: locOp);
				//
				fireShutdownFinishedAsynchronously(locEventHandler: locShutdownFinishedEventHandler, locExceptionObserver: locUnhandledExceptionObserver);
			}
			void fireShutdownFinishedAsynchronously(EventHandler locEventHandler, IUnhandledExceptionObserver locExceptionObserver) {
				if (!(locEventHandler is null))
					TaskUtilities.RunOnDefaultSchedulerOneWay(action: () => locEventHandler(sender: this, e: EventArgs.Empty), exceptionObserver: locExceptionObserver.DeriveObserver(component: this, message: $"'{nameof(ShutdownFinished)}' event handling throws an exception."));
			}
		}

		protected virtual Task BeforeShutdownAsync()
			=> Task.CompletedTask;

		protected override void FireBeforeDispose(bool explicitDispose) {
			base.FireBeforeDispose(explicitDispose);
			//
			if (explicitDispose)
				TryReadDA(ref _runControl)?.StopAsync().WaitWithTimeout();
		}

		protected override void Dispose(bool explicitDispose) {
			// _shutdownOp — очистка этой переменной не производится здесь (см. код ShutdownAsync).
			//
			if (explicitDispose) {
				_runState?.App.Dispose();
				_runState?.Runtime.Dispose();
				_runState?.Runtime.Dispose();
				_runControl?.Dispose();
				_configuration?.Dispose();
			}
			_runState = null;
			_runControl = null;
			_configuration = null;
			//
			_eventHandler_ShutdownFinished = null;
			_unhandledExceptionObserver = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}