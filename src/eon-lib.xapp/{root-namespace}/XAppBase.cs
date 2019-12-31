#region Compilation conditional symbols

#define DO_NOT_USE_OXY_LOGGING_API

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel;
using Eon.ComponentModel.Dependencies;
using Eon.Context;
using Eon.Description;
using Eon.Diagnostics;
using Eon.MessageFlow.Local;
using Eon.MessageFlow.Local.Description;
using Eon.Metadata;
using Eon.Runtime.Options;
using Eon.Text;
using Eon.Threading;
using Eon.Threading.Tasks;

using deputils = Eon.ComponentModel.Dependencies.DependencyUtilities;
using itrlck = Eon.Threading.InterlockedUtilities;
using vlt = Eon.Threading.VolatileUtilities;

namespace Eon {

	using IActivationList = IActivationList<IActivationListDescription>;
#if !DO_NOT_USE_OXY_LOGGING_API
	using ILoggingAutoSubscriptionXInstance = ILoggingAutoSubscription<ILoggingAutoSubscriptionDescription>;
#endif
	using IXAppInitializationList = IXAppInitializationList<IXAppInitializationListDescription>;
	using IXApp = IXApp<IXAppDescription>;
	using IXAppLocalPublisher = IXAppLocalPublisher<IXAppLocalPublisherDescription>;

	public abstract class XAppBase
		:XInstanceBase<IXAppDescription>, IXApp {

		#region Nested types

		sealed class P_XAppStartedEventArgs
			:IXAppStartedEventArgs {

			readonly IXApp _app;

			internal P_XAppStartedEventArgs(IXApp app) {
				app.EnsureNotNull(nameof(app));
				//
				_app = app;
			}

			public IXApp App
				=> _app;

		}

		#endregion

		#region Static members

		/// <summary>
		/// Value: '255'.
		/// </summary>
		public static readonly int DefaultOfMaxLengthOfAppInstanceId = 255;

		#endregion

		bool _hasAppShutdownRequested;

		CancellationTokenSource _appShutdownCts;

		bool _hasAppShutdownFinished;

		TaskCompletionSource<Nil> _appShutdownOp;

		EventHandler _eventHandler_AppShutdownFinished;

		readonly string _infoText;

		readonly string _appInstanceId;

		IXAppLocalPublisher _appMessageFlowPublisher;

#if !DO_NOT_USE_OXY_LOGGING_API
		ILoggingAutoSubscriptionXInstance _loggingAutoSubscription;
#endif

		RunControl<XAppBase> _runControl;

		IXAppInitializationList _appInitializationList;

		IActivationList _appStartActivationList;

		IXAppContainerControl _containerControl;

		IUnhandledExceptionObserver _unhandledExceptionObserver;

		protected XAppBase(IXAppCtorArgs<IXAppDescription> args)
			: base(description: args.EnsureNotNull(nameof(args)).Value.Description.ArgProp($"{nameof(args)}.{nameof(args.Description)}").EnsureNotNull().Value, outerServiceProvider: args.OuterServiceProvider, outerDependencies: args.OuterDependencies) {
			//
			var description = args.Description;
			var containerControl = args.ContainerControl;
			_hasAppShutdownRequested = false;
			_hasAppShutdownFinished = false;
			_containerControl = containerControl;
			_unhandledExceptionObserver = containerControl?.UnhandledExceptionObserver ?? UnhandledExceptionObserverOption.Require();
			_appInstanceId = CreateAppInstanceId(description: description, maxAllowedLength: DefaultOfMaxLengthOfAppInstanceId);
			_runControl =
				new RunControl<XAppBase>(
					options: RunControlOptions.SingleStart | ((containerControl?.Configuration.IsAppStartForbidden ?? false) ? RunControlOptions.ForbidStart : RunControlOptions.None),
					component: this,
					beforeStart: DoBeforeStartAsync,
					start: P_DoStartAsync,
					stop: DoStopAsync);
			_infoText = P_ToString();
		}

		public IUnhandledExceptionObserver UnhandledExceptionObserver
			=> ReadDA(location: ref _unhandledExceptionObserver);

		public bool HasContainerControl
			=> !(ReadDA(ref _containerControl) is null);

		// TODO: Put strings into the resources.
		//
		public IXAppContainerControl ContainerControl {
			get {
				var containerControl = ReadDA(ref _containerControl);
				if (containerControl is null)
					throw new EonException(message: $"This app doesn't have a container control.{Environment.NewLine}\tApp:{this.FmtStr().GNLI2()}");
				return containerControl;
			}
		}

		public string AppInstanceId
			=> _appInstanceId;

		public IRunControl<IXApp> RunControl
			=> ReadDA(ref _runControl);

		public IXAppLocalPublisher AppMessageFlow
			=> ReadIA(ref _appMessageFlowPublisher, locationName: nameof(_appMessageFlowPublisher));

		public IXAppInitializationList AppInitializationList
			=> ReadIA(ref _appInitializationList, locationName: nameof(_appInitializationList));

		public IActivationList AppStartActivationList
			=> ReadIA(ref _appStartActivationList, locationName: nameof(_appStartActivationList));

		IXApp<IXAppDescription> IXAppScopeInstance.App
			=> this;

		IXApp<IXAppDescription> IXAppScopeInstance.AppDisposeTolerant
			=> this;

		public CancellationToken AppShutdownToken {
			get {
				var cts = TryReadDA(ref _appShutdownCts);
				if (cts is null) {
					if (IsDisposeRequested || HasAppShutdownRequested)
						return CancellationTokenUtilities.Canceled;
					else {
						EnsureInitialized();
						throw ArgumentUtilitiesCoreL1.NewNullReferenceException(varName: nameof(_appShutdownCts), component: this);
					}
				}
				else
					try {
						return cts.Token;
					}
					catch (ObjectDisposedException) when (IsDisposeRequested || HasAppShutdownRequested) {
						return CancellationTokenUtilities.Canceled;
					}
			}
		}

		CancellationToken IXAppScopeInstance.ShutdownToken
			=> AppShutdownToken;

		public bool HasAppShutdownRequested
			=> vlt.Read(ref _hasAppShutdownRequested);

		bool IXAppScopeInstance.HasShutdownRequested
			=> vlt.Read(ref _hasAppShutdownRequested);

		public bool HasAppShutdownFinished
			=> vlt.Read(ref _hasAppShutdownFinished);

		public event EventHandler AppShutdownFinished {
			add { AddEventHandler(ref _eventHandler_AppShutdownFinished, value); }
			remove { RemoveEventHandler(ref _eventHandler_AppShutdownFinished, value); }
		}

		protected virtual string CreateAppInstanceId(IXAppDescription description, int maxAllowedLength) {
			description.EnsureNotNull(nameof(description));
			maxAllowedLength.Arg(nameof(maxAllowedLength)).EnsureNotLessThan(operand: 0);
			//
			if (maxAllowedLength == 0)
				return string.Empty;
			else {
				XAppInstanceIdTemplateSubstitution templateSubstitution;
				templateSubstitution =
					new XAppInstanceIdTemplateSubstitution(
						name: description.FullName.FmtStr().Short(),
						version: description.AppVersion.FmtStr().PrefixVInvariant(),
						pid: $"pid{ProcessUtilities.CurrentProcessId.ToString("d", CultureInfo.InvariantCulture)}");
				var guidPostfix = $"id-{GuidUtilities.NewGuidBase64String(count: 4)}";
				var appInstanceId =
					TextUtilities
					.SubstituteBraceTemplate(template: description.AppInstanceIdTemplate, values: templateSubstitution)
					.Truncate(maxTruncatedLength: Math.Max(0, maxAllowedLength - (guidPostfix.Length + 2)), mode: StringUtilities.TruncateMode.End);
				if (appInstanceId == string.Empty)
					return guidPostfix.Truncate(maxTruncatedLength: maxAllowedLength, mode: StringUtilities.TruncateMode.End);
				else
					return $"{appInstanceId}, {guidPostfix}";
			}
		}

		// TODO: Put strings into the resources.
		//
		protected sealed override async Task OnInitializeAsync(IContext ctx = default) {
			EnsureShutdownNotRequested();
			// Источник токена отмены, переходящего в сигнальное состояние при запросы "выключения" приложения (Shutdown).
			//
			var shutdownCts = default(CancellationTokenSource);
			try {
				shutdownCts = new CancellationTokenSource();
				if (!UpdDAIfNullBool(location: ref _appShutdownCts, value: shutdownCts))
					throw new EonException(message: $"App shutdown token has already set.{Environment.NewLine}\tApp:{this.FmtStr().GNLI2()}");
				EnsureShutdownNotRequested();
			}
			catch {
				itrlck.SetNullBool(location: ref _appShutdownCts, comparand: shutdownCts);
				shutdownCts?.Dispose();
				throw;
			}
			//
			var shutdownLinkedCts = default(CancellationTokenSource);
			var locCtx = default(IContext);
			try {
				var shutdownLinkedCt = CancellationTokenUtilities.SingleOrLinked(ct1: shutdownCts.Token, ct2: ctx.Ct(), linkedCts: out shutdownLinkedCts);
				locCtx = ContextUtilities.Create(outerCtx: ctx, ct: shutdownLinkedCt);
				// Издатель сообщений, используемый приложением (и его объектами).
				//
				var appMessageFlowPublisher = default(IXAppLocalPublisher);
				try {
					appMessageFlowPublisher = this.CreateAppScopeInstance<IXAppLocalPublisher>(description: Description.AppMessageFlowPublisher);
					await appMessageFlowPublisher.InitializeAsync(ctx: locCtx).ConfigureAwait(false);
					await appMessageFlowPublisher.StartAsync(ctx: locCtx).ConfigureAwait(false);
					WriteDA(ref _appMessageFlowPublisher, appMessageFlowPublisher);
					EnsureShutdownNotRequested();
				}
				catch (Exception exception) {
					appMessageFlowPublisher?.Dispose(exception);
					throw;
				}
				// Автоподписка логирования.
				//
#if !DO_NOT_USE_OXY_LOGGING_API
				var loggingAutoSubscriptionDescription = Description.LoggingAutoSubscription;
				if (loggingAutoSubscriptionDescription?.IsDisabled == false) {
					var loggingAutoSubscription = default(ILoggingAutoSubscriptionXInstance);
					try {
						loggingAutoSubscription =
							this
							.AppScopeInstanceOf<ILoggingAutoSubscriptionXInstance>(loggingAutoSubscriptionDescription)
							.Create(arg1: LoggingUtilities.Publisher);
						await loggingAutoSubscription.InitializeAsync(shutdownLinkedCt).ConfigureAwait(false);
						WriteDA(ref _loggingAutoSubscription, loggingAutoSubscription);
						EnsureShutdownNotRequested();
					}
					catch (Exception exception) {
						loggingAutoSubscription?.Dispose(exception);
						throw;
					}
				}
#endif
				//
				await OnInitializeAppAsync(ctx: locCtx).ConfigureAwait(false);
			}
			finally {
				locCtx?.Dispose();
				shutdownLinkedCts?.Dispose();
			}
		}

		protected virtual async Task OnInitializeAppAsync(IContext ctx = default) {
			EnsureShutdownNotRequested();
			//
			var appInitializationList = default(IXAppInitializationList);
			var appStartActivationList = default(IActivationList);
			try {
				// Инициализация списка компонентов, которые должны инициализироваться вместе с инициализацией приложения.
				//
				appInitializationList = await this.CreateInitializeAppScopeInstanceAsync<IXAppInitializationList>(description: Description.AppInitializationList).ConfigureAwait(false);
				WriteDA(ref _appInitializationList, appInitializationList);
				EnsureShutdownNotRequested();
				// Инициализация списка компонентов, активируемых при запуске приложения.
				//
				appStartActivationList = await this.CreateInitializeAppScopeInstanceAsync<IActivationList>(description: Description.AppStartActivationList).ConfigureAwait(false);
				WriteDA(ref _appStartActivationList, appStartActivationList);
				EnsureShutdownNotRequested();
			}
			catch (Exception exception) {
				appStartActivationList?.Dispose(exception);
				appInitializationList?.Dispose(exception);
				throw;
			}
		}

		protected virtual Task DoBeforeStartAsync(IRunControlAttemptState state) {
			try {
				state.EnsureNotNull(nameof(state));
				//
				EnsureShutdownNotRequested();
				EnsureInitialized();
				EnsureNotDisposeState(considerDisposeRequest: true);
				return TaskUtilities.FromVoidResult();
			}
			catch (Exception exception) {
				return TaskUtilities.FromError(error: exception);
			}
		}

		// TODO: Put strings into the resources.
		//
		async Task P_DoStartAsync(IRunControlAttemptState state) {
#if !DO_NOT_USE_OXY_LOGGING_API
			const string logMessagePrologue = "Запуск приложения.";
#endif
			state.EnsureNotNull(nameof(state));
			//
			var appMessageFlow = AppMessageFlow;
#if !DO_NOT_USE_OXY_LOGGING_API
			this.IssueInformation(messagePrologue: logMessagePrologue, message: "Начало запуска приложения.", severityLevel: SeverityLevel.Medium);
#endif
			try {
				await DoStartAsync(state: state).ConfigureAwait(false);
			}
			catch {
#if !DO_NOT_USE_OXY_LOGGING_API
				if (exception is OperationCanceledException)
					this.IssueWarning(messagePrologue: logMessagePrologue, message: "Запуск приложения прерван или отменен.", severityLevel: SeverityLevel.Medium);
				else
					this
						.IssueError(
							messagePrologue: logMessagePrologue,
							message: "В ходе запуска приложения возникла ошибка.",
							error: exception,
							includeErrorInIssueFaultException: true,
							severityLevel: SeverityLevel.Highest);
#endif
				throw;
			}
			if (HasAppShutdownRequested)
#if DO_NOT_USE_OXY_LOGGING_API
				return;
#else
				this.IssueInformation(messagePrologue: logMessagePrologue, message: "Запуск приложения завершён.", severityLevel: SeverityLevel.Medium);
#endif
			else {
				// Уведомление о запуске приложения.
				//
				try {
					await appMessageFlow.PublishAsync(payload: new P_XAppStartedEventArgs(app: this), disposePayloadAtEndOfPosting: true).ConfigureAwait(false);
				}
				catch (Exception exception) {
					var secondException = default(Exception);
					//
					try { await DoStopAsync().ConfigureAwait(false); }
					catch (Exception locSecondException) { secondException = locSecondException; }
					//
					throw new EonException(message: "Запуск приложения был успешно выполнен, но возник сбой публикации уведомления о запуске.", innerException: secondException is null ? exception : new AggregateException(exception, secondException));
				}
#if !DO_NOT_USE_OXY_LOGGING_API
				this.IssueInformation(messagePrologue: logMessagePrologue, message: "Запуск приложения успешно выполнен.", severityLevel: SeverityLevel.Medium);
#endif
			}
		}

		protected virtual async Task DoStartAsync(IRunControlAttemptState state) {
			state.EnsureNotNull(nameof(state));
			//
			await AppStartActivationList.ActivateAsync(ctx: state.Context).ConfigureAwait(false);
		}

		protected async virtual Task DoStopAsync()
			=> await AppStartActivationList.DeactivateAsync().ConfigureAwait(false);

		// TODO: Put strings into the resources.
		//
		public Task ShutdownAppAsync() {
			if (HasAppShutdownFinished)
				return Task.CompletedTask;
			else {
				vlt.Write(ref _hasAppShutdownRequested, true);
				//
				var existingOp = default(TaskCompletionSource<Nil>);
				var newOp = default(TaskCompletionSource<Nil>);
				try {
					existingOp = itrlck.Get(ref _appShutdownOp) ?? UpdDAIfNull(location: ref _appShutdownOp, value: newOp = new TaskCompletionSource<Nil>(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously));
					//
					if (ReferenceEquals(objA: newOp, objB: existingOp)) {
						if (HasAppShutdownFinished) {
							itrlck.SetNull(location: ref _appShutdownOp, comparand: newOp);
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
						return Task.FromException(exception: exception);
				}
				finally {
					if (!ReferenceEquals(objA: existingOp, objB: newOp))
						newOp?.TrySetCanceled();
				}
			}
			//
			async Task doShutdownAsync(TaskCompletionSource<Nil> locOp) {
				try {
#if !DO_NOT_USE_OXY_LOGGING_API
					this.IssueInformation(
						messagePrologue: "Выключение приложения.",
						message: "Получен управляющий элемент остановки и выгрузки приложения.",
						severityLevel: SeverityLevel.Medium);
#endif
					//
					var locShutdownCts = ReadDA(ref _appShutdownCts);
					if (locShutdownCts?.IsCancellationRequested == false)
						locShutdownCts.Cancel(throwOnFirstException: false);
					await BeforeShutdownAsync().ConfigureAwait(false);
					//
					await ReadDA(ref _runControl).StopAsync(finiteStop: true).ConfigureAwait(false);
#if !DO_NOT_USE_OXY_LOGGING_API
					this
						.IssueInformation(
							messagePrologue: "Выключение приложения.",
							message: "Приложение остановлено.",
							severityLevel: SeverityLevel.Medium);
#endif
					//
					var locUnhandledExceptionObserver = ReadDA(location: ref _unhandledExceptionObserver);
					var locShutdownFinishedEventHandler = ReadDA(location: ref _eventHandler_AppShutdownFinished);
					Dispose();
#if !DO_NOT_USE_OXY_LOGGING_API
					this
						.IssueInformation(
							messagePrologue: "Выключение приложения.",
							message: "Приложение выгружено.",
							severityLevel: SeverityLevel.Medium);
#endif
					//
					vlt.Write(location: ref _hasAppShutdownFinished, value: true);
					itrlck.SetNullBool(location: ref _appShutdownOp, comparand: locOp);
					//
					fireShutdownFinishedAsynchronously(locEventHandler: locShutdownFinishedEventHandler, locExceptionObserver: locUnhandledExceptionObserver);
#if !DO_NOT_USE_OXY_LOGGING_API
					this
						.IssueInformation(
							messagePrologue: "Выключение приложения.",
							message: $"Событие '{nameof(AppShutdownFinished)}' завершения выключения приложения обработано.",
							severityLevel: Diagnostics.SeverityLevel.Medium);
#endif
				}
				catch {
#if !DO_NOT_USE_OXY_LOGGING_API
					this
						.IssueError(
							messagePrologue: "Выключение приложения.",
							message: "Ошибка во время выполнения остановки и выгрузки приложения.",
							error: exception,
							includeErrorInIssueFaultException: true,
							severityLevel: SeverityLevel.Highest);
#endif
					throw;
				}
			}
			void fireShutdownFinishedAsynchronously(EventHandler locEventHandler, IUnhandledExceptionObserver locExceptionObserver) {
				if (!(locEventHandler is null))
					TaskUtilities.RunOnDefaultSchedulerOneWay(action: () => locEventHandler(sender: this, e: EventArgs.Empty), exceptionObserver: locExceptionObserver.DeriveObserver(component: this, message: $"'{nameof(AppShutdownFinished)}' event handling throws an exception."));
			}
		}

		protected virtual Task BeforeShutdownAsync()
			=> Task.CompletedTask;

		// TODO: Put strings into the resources.
		//
		public void EnsureShutdownNotRequested() {
			if (HasAppShutdownRequested)
				throw
					new EonException($"Запрошенная операция не может быть выполнена (или завершена), так как ранее приложением был получен управляющий элемент остановки и выгрузки приложения.{Environment.NewLine}\tПриложение:{this.FmtStr().GNLI2()}");
		}

		public override IEnumerable<IVh<IDependencyHandler2>> LocalDependencies() {
			yield return
				deputils.CreateHandlerForShared(() => AppMessageFlow.ToValueHolder(ownsValue: false));
			//
			foreach (var baseExecutor in base.LocalDependencies())
				yield return baseExecutor;
		}

		// TODO: Put strings into the resources.
		//
		string P_ToString() {
			DescriptionPackageIdentity packageIdentity;
			Uri siteOrigin;
			MetadataPathName fullName;
			try {
				var description = HasDescription ? Description : null;
				var package = description?.Package;
				//
				fullName = (description?.FullName);
				packageIdentity = package?.Identity;
				siteOrigin = package?.HasSiteOrigin == true ? package.SiteOrigin : null;

			}
			catch (ObjectDisposedException) {
				packageIdentity = null;
				siteOrigin = null;
				fullName = null;
			}
			var newLine = Environment.NewLine;
			return
				$"Name:{fullName.FmtStr().ShortNewLineIndented()}"
				+ $"{newLine}Type:{GetType().FmtStr().GNLI()}"
				+ (packageIdentity is null ? string.Empty : $"{newLine}Package (identity):{packageIdentity.FmtStr().GNLI()}")
				+ (siteOrigin is null ? string.Empty : $"{newLine}Site origin:{siteOrigin.ToEscapedString().FmtStr().GNLI()}");
		}

		public override string ToString()
			=> _infoText;

		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose)
				// Здесь вызывается именно RunControl.StopAsync(), но не ShutdownAsync(), так как ShutdownAsync() выполняет помимо остановки и выгрузку приложения.
				//
				TryReadDA(ref _runControl)?.StopAsync().WaitWithTimeout();
			//
			base.FireBeforeDispose(explicitDispose);
		}

		protected override void Dispose(bool explicitDispose) {
			// _appShutdownOp — очистка этой переменной не производится здесь (см. код ShutdownAsync).
			//
			if (explicitDispose) {
				_runControl?.Dispose();
				_appStartActivationList?.Dispose();
				_appInitializationList?.Dispose();
#if !DO_NOT_USE_OXY_LOGGING_API
				_loggingAutoSubscription?.Dispose();
#endif
				_appMessageFlowPublisher?.Dispose();
				//
				_appShutdownCts?.Dispose();
			}
			_appInitializationList = null;
			_appMessageFlowPublisher = null;
			_appShutdownCts = null;
			_appStartActivationList = null;
			_containerControl = null;
			_eventHandler_AppShutdownFinished = null;
#if !DO_NOT_USE_OXY_LOGGING_API
			_loggingAutoSubscription = null;
#endif
			_runControl = null;
			_unhandledExceptionObserver = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}
