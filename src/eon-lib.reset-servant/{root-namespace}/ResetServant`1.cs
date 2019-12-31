#region Compilation conditional symbols

#if DEBUG

#define SIMULATE_RESET_FAILURE
#undef SIMULATE_RESET_FAILURE

#endif

#define DO_NOT_USE_OXY_LOGGING_API

#endregion
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel;
using Eon.Context;
using Eon.Description;
using Eon.Diagnostics;
using Eon.Threading;
using Eon.Threading.Tasks;
using Eon.Triggers;
using Eon.Triggers.Description;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon {
	using ITriggerXInstance = ITrigger<ITriggerDescription>;

	public class ResetServant<TDescription>
		:ActivatableXAppScopeInstanceBase<TDescription, ResetServant<TDescription>>, IResetServant<TDescription>
		where TDescription : class, IResetServantDescription {

		#region Nested types

		sealed class P_ResetState {

			internal P_ResetState() { }

			public IXAppScopeInstance PreviousComponent { get; set; }

		}

		#endregion

		SemaphoreSlim _resetLock;

		long _resetCounter;

		Func<Exception, IContext, Task> _resetFailureResponseDelegate;

		ITriggerXInstance _resetTrigger;

		RunControlFactoryDelegate<ResetServant<TDescription>> _activateControlFactory;

		IDisposable _resetTriggerSignalSubscription;

		IXAppScopeInstance _component;

		public ResetServant(IXAppScopeInstance scope, TDescription description)
			: base(scope: scope, description: description) {
			// TODO_HIGH: Implement lock naming — $"{nameof(ResetServant<TDescription>)}.{nameof(_resetLock)}".
			//
			_resetLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
			_resetCounter = -1L;
		}

		SemaphoreSlim P_ResetLock
			=> ReadDA(ref _resetLock);

		protected sealed override RunControlOptions DefineDefaultOfActivateControlOptions()
			/* Важно! Изменение возвращаемого значения обязательно требует ревью кода этого компонента. */
			=> RunControlOptions.SingleStart;

		// TODO: Put strings into the resources.
		//
		protected override async Task OnInitializeAsync(IContext ctx = default) {
			RunControl<ResetServant<TDescription>> activateControl = default;
			ITriggerXInstance resetTrigger = default;
			try {
				var componentDescription = Description.Component;
				var componentDisability = componentDescription.IsDisabled();
				if (componentDisability) {
					this.LogDisabilityWarning(description: componentDescription);
					WriteDA(
						location: ref _activateControlFactory,
						value:
							(locOptions, locComponent, locAttemptState, locBeforeActivate, locActivate, locDeactivate, locDeactivateToken)
							=>
							new RunControl<ResetServant<TDescription>>(
								options: locOptions,
								component: locComponent,
								attemptState: locAttemptState,
								beforeStart: locBeforeActivate,
								start: locState => P_DoActivateAsync(state: locState, continuation: locActivate, componentDisability: true),
								stop: () => P_DoDeactivateAsync(continuation: locDeactivate, componentDisability: true),
								stopToken: locDeactivateToken));
					resetTrigger = null;
				}
				else {
					switch (Description.ResetFailureResponseCode) {
						case ResetServantResetFailureResponseCode.None:
							WriteDA(location: ref _resetFailureResponseDelegate, value: null);
							break;
						default:
							RequireComponentResetFailureResponseDelegate(dlg: out var failureResponseDlg);
							if (failureResponseDlg is null)
								throw new EonException(message: $"Method '{nameof(RequireComponentResetFailureResponseDelegate)}' has returned invalid result '{failureResponseDlg.FmtStr().G()}'.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
							WriteDA(ref _resetFailureResponseDelegate, failureResponseDlg);
							break;
					}
					//
					WriteDA(
						location: ref _activateControlFactory,
						value:
							(locOptions, locComponent, locAttemptState, locBeforeActivate, locActivate, locDeactivate, locDeactivateToken) 
							=>
							new RunControl<ResetServant<TDescription>>(
								options: locOptions,
								component: locComponent,
								attemptState: locAttemptState,
								beforeStart: locBeforeActivate,
								start: locState => P_DoActivateAsync(state: locState, continuation: locActivate, componentDisability: false),
								stop: () => P_DoDeactivateAsync(continuation: locDeactivate, componentDisability: false),
								stopToken: locDeactivateToken));
					if (!Description.ResetTrigger.IsDisabled)
						resetTrigger = await this.CreateInitializeAppScopeInstanceAsync<ITriggerXInstance>(description: Description.ResetTrigger, ctx: ctx).ConfigureAwait(false);
					else
						resetTrigger = null;
				}
				WriteDA(ref _resetTrigger, resetTrigger);
				//
				await base.OnInitializeAsync(ctx: ctx).ConfigureAwait(false);
			}
			catch (Exception exception) {
				resetTrigger?.Dispose(exception);
				activateControl?.Dispose(exception);
				throw;
			}
		}

		protected override void CreateActivateControl(
			RunControlOptions options,
			ResetServant<TDescription> component,
			RunControlAttemptStateFactory attemptState,
			Func<IRunControlAttemptState, Task> beforeActivate,
			Func<IRunControlAttemptState, Task> activate,
			Func<Task> deactivate,
			CancellationToken deactivationToken,
			out IRunControl<ResetServant<TDescription>> control)
			=>
			control =
			ReadIA(ref _activateControlFactory, locationName: nameof(_activateControlFactory))
			(options: options, component: component, attemptState: attemptState, beforeStart: beforeActivate, start: activate, stop: deactivate, stopToken: deactivationToken);

		async Task P_DoActivateAsync(IRunControlAttemptState state, Func<IRunControlAttemptState, Task> continuation, bool componentDisability) {
			state.EnsureNotNull(nameof(state));
			continuation.EnsureNotNull(nameof(continuation));
			//
			if (componentDisability)
				await continuation(arg: state).ConfigureAwait(false);
			else {
				var resetTrigger = ReadIA(ref _resetTrigger, isNotRequired: true, locationName: nameof(_resetTrigger));
				IXAppScopeInstance component = default;
				IDisposable resetTriggerSubscription = default;
				try {
					component =
						await
						P_ResetComponentAsync(breakCondition: breakCondition, doFailureResponse: false)
						.ConfigureAwait(false);
					await continuation(arg: state).ConfigureAwait(false);
					if (resetTrigger != null) {
						WriteDA(
							location: ref _resetTriggerSignalSubscription,
							value: resetTriggerSubscription = TriggerUtilities.SubscribeToSignal(trigger: resetTrigger, onSignal: P_ResetTrigger_OnSignal));
						await resetTrigger.ActivateControl.StartAsync().ConfigureAwait(false);
					}
				}
				catch (Exception exception) {
					var caughtExceptions = new List<Exception>() { exception };
					if (resetTriggerSubscription != null) {
						itrlck.SetNullBool(ref _resetTriggerSignalSubscription, resetTriggerSubscription);
						try { resetTriggerSubscription.Dispose(); }
						catch (Exception secondException) { caughtExceptions.Add(secondException); }
					}
					if (component != null)
						try {
							await OnFreeComponentAsync(component: component).ConfigureAwait(false);
							itrlck.SetNullBool(ref _component, component);
						}
						catch (Exception secondException) {
							caughtExceptions.Add(secondException);
						}
					if (caughtExceptions.Count > 1)
						throw new AggregateException(innerExceptions: caughtExceptions);
					else
						throw;
				}
			}
			//
			bool breakCondition(IContext locCtx) {
				locCtx.ThrowIfCancellationRequested();
				return state.RunControl.HasStopRequested || HasDeactivationRequested;
			}
		}

		protected override Task DoActivateAsync(IRunControlAttemptState state)
			=> TaskUtilities.FromAction(action: delegate () { state.EnsureNotNull(nameof(state)); });

		async Task P_DoDeactivateAsync(Func<Task> continuation, bool componentDisability) {
			continuation.EnsureNotNull(nameof(continuation));
			//
			var caughtExceptions = new List<Exception>();
			try { await continuation().ConfigureAwait(false); }
			catch (Exception exception) { caughtExceptions.Add(exception); }
			if (!componentDisability) {
				var resetTriggerSignalSubscription = ReadDA(ref _resetTriggerSignalSubscription);
				if (resetTriggerSignalSubscription != null)
					try { resetTriggerSignalSubscription.Dispose(); }
					catch (Exception exception) { caughtExceptions.Add(exception); }
				var resetTrigger = ReadDA(ref _resetTrigger);
				if (resetTrigger != null)
					try { await resetTrigger.ActivateControl.StopAsync().ConfigureAwait(false); }
					catch (Exception exception) { caughtExceptions.Add(exception); }
				var component = ReadDA(ref _component);
				var resetLock = P_ResetLock;
				if (component != null)
					try {
						var lckAcquired = false;
						try {
							lckAcquired = await resetLock.WaitAsync(millisecondsTimeout: TaskUtilities.DefaultAsyncTimeoutMilliseconds).ConfigureAwait(false);
							if (!lckAcquired)
								throw new LockAcquisitionFailException(reason: LockAcquisitionFailReason.TimeoutElapsed);
							//
							component = ReadDA(ref _component);
							if (component != null) {
								await OnFreeComponentAsync(component: component).ConfigureAwait(false);
								itrlck.SetNullBool(location: ref _component, comparand: component);
							}
						}
						finally {
							if (lckAcquired)
								try { resetLock.Release(); }
								catch (ObjectDisposedException) { }
						}
					}
					catch (Exception exception) {
						caughtExceptions.Add(exception);
					}
			}
			if (caughtExceptions.Count > 0)
				throw new AggregateException(innerExceptions: caughtExceptions);
		}

		protected override Task DoDeactivateAsync()
			=> TaskUtilities.FromVoidResult();

		// TODO: Put strings into the resources.
		//
		void P_ResetTrigger_OnSignal(ITriggerSignalProperties signalProps, IDisposable subscription) {
			signalProps.EnsureNotNull(nameof(signalProps));
			subscription.EnsureNotNull(nameof(subscription));
			//
			var resetTrigger = TryReadDA(ref _resetTrigger);
			var resetTriggerSubscription = TryReadDA(ref _resetTriggerSignalSubscription, considerDisposeRequest: true);
			if (ReferenceEquals(resetTrigger, signalProps.Trigger) && ReferenceEquals(resetTriggerSubscription, subscription)) {
				try { if (signalProps.Trigger.IsDisabled) return; }
				catch (ObjectDisposedException) { return; }
				if (IsActive)
					TaskUtilities.RunOnDefaultScheduler(factory: () => resetComponentAsync(locTriggerSignalProps: signalProps));
			}
			//
			async Task resetComponentAsync(ITriggerSignalProperties locTriggerSignalProps) {
				try {
					using (var localCtx = ContextUtilities.Create(fullCorrelationId: signalProps.CorrelationId))
						await P_ResetComponentAsync(triggerSignalProps: locTriggerSignalProps, doFailureResponse: true, ctx: localCtx).ConfigureAwait(false);
				}
				catch (Exception exception) {
					if (!((exception is ObjectDisposedException && IsDisposeRequested) || exception.HasSingleBaseExceptionOf<OperationCanceledException>() || exception.IsObserved())) {
#if !DO_NOT_USE_OXY_LOGGING_API
						this
							.IssueError(
								message: $"Сбой установки (замены) компонента, инициированной триггером.{Environment.NewLine}\tСобытие-инициатор:{locTriggerSignalProps.FmtStr().GNLI2()}",
								error: exception,
								includeErrorInIssueFaultException: true,
								severityLevel: exception.GetMostHighSeverityLevel(baseLevel: SeverityLevel.Medium));
#endif
					}
				}
			}
		}

		// TODO: Put strings into the resources.
		//
		Task<IXAppScopeInstance> P_ResetComponentAsync(Func<IContext, bool> breakCondition = default, ITriggerSignalProperties triggerSignalProps = default, bool doFailureResponse = default, IContext ctx = default) {
			try {
				if (breakCondition is null)
					breakCondition =
						(locContext) => {
							locContext.ThrowIfCancellationRequested();
							return (!IsActive || HasDeactivationRequested);
						};
				//
				var startTimestamp = StopwatchUtilities.GetTimestampAsTimeSpan();
				IContext locCtx = default;
				try {
					var resetNumber = Interlocked.Increment(ref _resetCounter);
					var failureResponseDlg = ReadIA(ref _resetFailureResponseDelegate, isNotRequired: true, locationName: nameof(_resetFailureResponseDelegate));
					var state = new P_ResetState();
					locCtx = CreateScopedContext(outerCtx: ctx, localTag: state);
					var logMessagePrologue = $"{(resetNumber == 0 ? "Установка компонента." : $"Замена (переустановка) компонента. Порядковый номер замены: {resetNumber:d}.")} ИД корреляции: {locCtx.FullCorrelationId}.{(triggerSignalProps is null ? string.Empty : $"{Environment.NewLine}\tСобытие-инициатор:{triggerSignalProps.FmtStr().GNLI2()}")}";
					if (locCtx.IsCancellationRequested())
						return Task.FromCanceled<IXAppScopeInstance>(cancellationToken: locCtx.Ct());
					var resetTask = TaskUtilities.RunOnDefaultScheduler(factory: async () => await doResetAsync(locCtx: locCtx).ConfigureAwait(false));
					// Вывод результатов ресета в лог.
					//
#if !DO_NOT_USE_OXY_LOGGING_API
					resetTask
						.ContinueWith(
							continuationAction:
								locTask => {
									var locDuration = StopwatchUtilities.GetTimestampAsTimeSpan().Subtract(ts: startTimestamp);
									if (locTask.IsFaulted) {
										this
											.IssueError(
												messagePrologue: logMessagePrologue,
												message: $"Длительность:{locDuration.ToString("c").FmtStr().GNLI()}{Environment.NewLine}Сбой.",
												error: locTask.Exception,
												includeErrorInIssueFaultException: true,
												severityLevel: locTask.Exception.GetMostHighSeverityLevel(baseLevel: SeverityLevel.Medium));
										locTask.Exception.MarkAsObserved();
									}
									else if (locTask.IsCanceled)
										this
											.IssueWarning(
												messagePrologue: logMessagePrologue,
												message: $"Длительность:{locDuration.ToString("c").FmtStr().GNLI()}{Environment.NewLine}Отменено или прервано.",
												severityLevel: SeverityLevel.Medium);
									else
										this
											.IssueInformation(
												messagePrologue: logMessagePrologue,
												message: $"Длительность:{locDuration.ToString("c").FmtStr().GNLI()}{Environment.NewLine}Успешно выполнено.{Environment.NewLine}\tНовый экземпляр компонента:{locTask.Result.FmtStr().GNLI2()}",
												severityLevel: SeverityLevel.Medium);
								},
							continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
#endif
					// Обработка сбоя ресета.
					//
					Task failureResponseTask;
					if (doFailureResponse && !(failureResponseDlg is null)) {
						failureResponseTask =
							resetTask
							.ContinueWith(
								continuationFunction:
									locTask => {
										if (!(state.PreviousComponent is null))
											// Корректирующие действие сбоя ресета выполняется только в случае именно замены компонента.
											//
											return
												doFailureCorrectiveAsync(
													locFailure: locTask.Exception,
													responseDlg: failureResponseDlg,
													locCtx: locCtx,
													logMessagePrologue: logMessagePrologue);
										else
											return TaskUtilities.FromCanceled();
									},
								continuationOptions: TaskContinuationOptions.PreferFairness | TaskContinuationOptions.OnlyOnFaulted)
							.Unwrap();
						// Вывод результатов обработки сбоя в лог.
						//
#if !DO_NOT_USE_OXY_LOGGING_API
						failureResponseTask
							.ContinueWith(
								continuationAction:
									locTask => {
										if (locTask.IsFaulted)
											this
												.IssueError(
													messagePrologue: logMessagePrologue,
													message: $"Корректирующее действие при сбое замены (переустановки) компонента.{Environment.NewLine}Сбой корректирующего действия.",
													error: locTask.Exception,
													includeErrorInIssueFaultException: true,
													severityLevel: locTask.Exception.GetMostHighSeverityLevel(baseLevel: SeverityLevel.High));
										else
											this
												.IssueInformation(
													messagePrologue: logMessagePrologue,
													message: $"Корректирующее действие при сбое замены (переустановки) компонента.{Environment.NewLine}Корректирующее действие успешно выполнено.",
													severityLevel: SeverityLevel.Medium);
									},
								continuationOptions: TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.NotOnCanceled);
#endif
					}
					else
						failureResponseTask = null;
					// Очистка.
					//
					if (failureResponseTask is null)
						resetTask.ContinueWith(continuationAction: locTask => locCtx.Dispose(), continuationOptions: TaskContinuationOptions.PreferFairness);
					else
						failureResponseTask.ContinueWith(continuationAction: locTask => locCtx.Dispose(), continuationOptions: TaskContinuationOptions.PreferFairness);
					return resetTask;
				}
				catch (Exception exception) {
					locCtx?.Dispose(exception);
					throw;
				}
			}
			catch (Exception exception) {
				return TaskUtilities.FromError<IXAppScopeInstance>(error: exception);
			}
			//
			async Task<IXAppScopeInstance> doResetAsync(IContext locCtx) {
				locCtx.EnsureNotNull(nameof(locCtx));
				var resetState =
					locCtx
					.LocalTag
					.ArgProp($"{nameof(locCtx)}.{nameof(locCtx.LocalTag)}")
					.EnsureNotNull()
					.EnsureOfType<P_ResetState>()
					.Value;
				//
				if (breakCondition(locCtx))
					throw new OperationCanceledException();
				else {
					IXAppScopeInstance newComponent = default;
					try {
						var resetLock = P_ResetLock;
						var lckAcquired = false;
						var componentBefore = ReadDA(ref _component);
						try {
							lckAcquired = await resetLock.WaitAsync(millisecondsTimeout: TaskUtilities.DefaultAsyncTimeoutMilliseconds).ConfigureAwait(false);
							if (!lckAcquired)
								throw new LockAcquisitionFailException(reason: LockAcquisitionFailReason.TimeoutElapsed);
							//
							if (breakCondition(locCtx))
								throw new OperationCanceledException();
							else {
								EnsureNotDisposeState(considerDisposeRequest: true);
								var component = ReadDA(ref _component);
								if (component is null || ReferenceEquals(component, componentBefore)) {
									if (!(component is null)) {
										resetState.PreviousComponent = component;
#if SIMULATE_RESET_FAILURE
										throw new DigitalFlareApplicationException(message: "Dummy reset failure.");
#else
										await OnFreeComponentAsync(component: component, ctx: locCtx).ConfigureAwait(false);
										itrlck.SetNullBool(location: ref _component, comparand: component);
#endif
									}
									if (breakCondition(locCtx))
										throw new OperationCanceledException();
									else {
										EnsureNotDisposeState(considerDisposeRequest: true);
										CreateComponent(component: out newComponent, ctx: locCtx);
										try {
											await OnSetupComponentAsync(component: newComponent, ctx: locCtx).ConfigureAwait(false);
											if (!UpdDAIfNullBool(ref _component, newComponent)) {
												await OnFreeComponentAsync(component: newComponent, ctx: locCtx).ConfigureAwait(false);
												throw
													new InvalidOperationException(
														message: "Завершающий этап выполнения данной операциии предполагает отсутствие экземпляра компонента. Однако обнарежено, что другой экземпляр компонента уже установлен.");
											}
											return newComponent;
										}
										catch {
											itrlck.UpdateBool(location: ref _component, value: null, comparand: newComponent);
											throw;
										}
									}
								}
								else
									return component;
							}
						}
						finally {
							if (lckAcquired)
								try { resetLock.Release(); }
								catch (ObjectDisposedException) { }
						}
					}
					catch (Exception exception) {
						newComponent?.Dispose(exception);
						throw;
					}
				}
			}
			async Task doFailureCorrectiveAsync(Exception locFailure, Func<Exception, IContext, Task> responseDlg, IContext locCtx, string logMessagePrologue) {
				locFailure.EnsureNotNull(nameof(locFailure));
				responseDlg.EnsureNotNull(nameof(responseDlg));
				locCtx.EnsureNotNull(nameof(locCtx));
				//
				if (breakCondition(locCtx))
					throw new OperationCanceledException();
				else {

					var lckAcquired = false;
					var resetLock = P_ResetLock;
					try {
						lckAcquired = await resetLock.WaitAsync(millisecondsTimeout: TaskUtilities.DefaultAsyncTimeoutMilliseconds).ConfigureAwait(false);
						if (!lckAcquired)
							throw new LockAcquisitionFailException(reason: LockAcquisitionFailReason.TimeoutElapsed);
						//
						if (breakCondition(locCtx))
							throw new OperationCanceledException();
						else
							await responseDlg(arg1: locFailure, arg2: locCtx).ConfigureAwait(false);
					}
					finally {
						if (lckAcquired)
							try { resetLock.Release(); }
							catch (ObjectDisposedException) { }
					}
				}
			}
		}

		public Task<IXAppScopeInstance> ResetComponentAsync(ITriggerSignalProperties triggerSignalProps = default, bool doFailureResponse = default, IContext ctx = default)
			=> P_ResetComponentAsync(triggerSignalProps: triggerSignalProps, doFailureResponse: doFailureResponse, ctx: ctx);

		// TODO: Put strings into the resources.
		//
		protected virtual void CreateComponent(out IXAppScopeInstance component, IContext ctx = default) {
			try {
				component = this.CreateAppScopeInstance<IXAppScopeInstance>(description: Description.Component);
			}
			catch (Exception exception) {
				throw new EonException(message: "Ошибка создания экземпляра компонента.", innerException: exception);
			}
		}

		protected virtual async Task OnSetupComponentAsync(IXAppScopeInstance component, IContext ctx = default) {
			component.EnsureNotNull(nameof(component));
			//
			await component.InitializeAsync(ct: ctx.Ct()).ConfigureAwait(false);
			if (component is IActivatableXAppScopeInstance activatableComponent && activatableComponent.IsAutoActivationEnabled) {
				ctx.ThrowIfCancellationRequested();
				await activatableComponent.ActivateAsync().ConfigureAwait(false);
			}
		}

		protected virtual async Task OnFreeComponentAsync(IXAppScopeInstance component, IContext ctx = default) {
			component.EnsureNotNull(nameof(component));
			//
			if (component is IActivatableXAppScopeInstance activatableComponent) {
				IRunControl control;
				try { control = activatableComponent.ActivateControl; }
				catch (ObjectDisposedException) { if (activatableComponent.IsDisposeRequested) control = null; else throw; }
				if (control != null)
					await control.StopAsync().ConfigureAwait(false);
			}
			component.Dispose();
		}

		// TODO: Put strings into the resources.
		//
		protected virtual void RequireComponentResetFailureResponseDelegate(out Func<Exception, IContext, Task> dlg) {
			switch (Description.ResetFailureResponseCode) {
				case ResetServantResetFailureResponseCode.None:
					dlg = null;
					break;
				case ResetServantResetFailureResponseCode.RunProgram:
					dlg = P_ResetFailure_RunProgramAsync;
					break;
				case ResetServantResetFailureResponseCode.RunProgramThenShutdownApp:
					dlg = P_ResetFailure_RunProgramThenShutdownAppAsync;
					break;
				default:
					throw
						new NotSupportedException(
							message: $"Указанный описанием код корректирующего действия в случае сбоя замены (переустановки) компонента не поддерживается (см. '{nameof(Description)}.{nameof(Description.ResetFailureResponseCode)}').{Environment.NewLine}\tКод:{Description.ResetFailureResponseCode.FmtStr().GNLI2()}{Environment.NewLine}\tКомпонент:{this.FmtStr().GNLI2()}");
			}
		}

		async Task P_ResetFailure_RunProgramThenShutdownAppAsync(Exception failure, IContext ctx = default) {
			Func<Task> appShutdownDelegate;
			CancellationToken appShutdownToken;
			IXApp<IXAppDescription> app;
			IXAppContainerControl appContainer;
			try {
				app = App;
				appShutdownToken = app.ShutdownToken;
				appContainer = app.HasContainerControl ? app.ContainerControl : null;
			}
			catch (ObjectDisposedException) {
				ctx.ThrowIfCancellationRequested();
				throw;
			}
			if (appContainer is null)
				appShutdownDelegate = app.ShutdownAppAsync;
			else
				appShutdownDelegate = appContainer.ShutdownAsync;
			//
			await P_ResetFailure_RunProgramAsync(failure: failure, ctx: ctx).ConfigureAwait(false);
			await
				TaskUtilities
				.RunOnDefaultScheduler(factory: appShutdownDelegate)
				.WaitCompletionAsync(ct: appShutdownToken, cancellationBreak: true)
				.ConfigureAwait(false);
		}

		async Task P_ResetFailure_RunProgramAsync(Exception failure, IContext ctx = default) {
			var programStartInfo = new ProcessStartInfo();
			programStartInfo.Arguments = Description.ResetFailureProgramArgs ?? string.Empty;
			programStartInfo.CreateNoWindow = true;
			programStartInfo.FileName = Description.ResetFailureProgram;
			programStartInfo.UseShellExecute = true;
			programStartInfo.ErrorDialog = false;
			programStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			using (var program = new Process()) {
				program.StartInfo = programStartInfo;
				program.Start();
			}
			await Task.CompletedTask;
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_resetLock?.Dispose();
				_component?.Dispose();
				_resetTriggerSignalSubscription?.Dispose();
				_resetTrigger?.Dispose();
			}
			_resetLock = null;
			_activateControlFactory = null;
			_component = null;
			_resetTriggerSignalSubscription = null;
			_resetTrigger = null;
			_resetFailureResponseDelegate = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}