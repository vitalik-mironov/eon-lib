#region Compilation conditional symbols

#define DO_NOT_USE_EON_LOGGING_API

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel;
using Eon.Context;
using Eon.Description;
using Eon.MessageFlow.Local;
using Eon.Threading;
using Eon.Threading.Tasks;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon {

	public class ActivationServant<TDescription>
		:ActivatableXAppScopeInstanceBase<TDescription, ActivationServant<TDescription>>
		where TDescription : class, IActivationServantDescription {

		#region	Nested types

		sealed class P_ComponentActivationAttempt
			:Disposable {

			readonly int _retryIndex;

			readonly XFullCorrelationId _correlationId;

			RunControl<P_ComponentActivationAttempt> _activateControl;

			IActivatableXAppScopeInstance _component;

			internal P_ComponentActivationAttempt(int retryIndex, IActivatableXAppScopeInstance component, XFullCorrelationId correlationId, CancellationToken ct) {
				retryIndex
					.Arg(nameof(retryIndex))
					.EnsureNotLessThan(operand: -1);
				component.EnsureNotNull(nameof(component));
				correlationId.EnsureNotNull(nameof(correlationId));
				//
				_retryIndex = retryIndex;
				_correlationId = correlationId;
				_component = component;
				_activateControl =
					new RunControl<P_ComponentActivationAttempt>(
						options: RunControlOptions.SingleStart,
						component: this,
						start: P_DoActivateAsync,
						stop: P_DoDeactivateAsync,
						stopToken: ct);
			}

			/// <summary>
			/// Возвращает индекс повторной попытки активации.
			/// <para>Обращение к свойству не подвержено влиянию выгрузки.</para>
			/// </summary>
			public int RetryAttemptIndex
				=> _retryIndex;

			/// <summary>
			/// Возвращает ИД корреляции.
			/// <para>Не может быть <see langword="null"/>.</para>
			/// <para>Обращение к свойству не подвержено влиянию выгрузки.</para>
			/// </summary>
			public XFullCorrelationId CorrelationId
				=> _correlationId;

			public IActivatableXAppScopeInstance Component
				=> ReadDA(ref _component);

			public IRunControl<P_ComponentActivationAttempt> ActivateControl
				=> ReadDA(ref _activateControl);

			async Task P_DoActivateAsync(IRunControlAttemptState state) {
				state.EnsureNotNull(nameof(state));
				//
				var component = Component;
				state.Context.ThrowIfCancellationRequested();
				if (component.HasDeactivationRequested)
					throw new OperationCanceledException();
				else
					await component.InitializeAsync(ct: state.Context.Ct()).ConfigureAwait(false);
				//
				state.Context.ThrowIfCancellationRequested();
				if (component.HasDeactivationRequested)
					throw new OperationCanceledException();
				else
					await component.ActivateControl.StartAsync(ctx: state.Context).ConfigureAwait(false);
			}

			// TODO: Put strings into the resources.
			//
			async Task P_DoDeactivateAsync() {
				var component = Component;
				try {
					await component.ActivateControl.StopAsync().ConfigureAwait(false);
				}
				catch (Exception exception) {
					if (exception.HasSingleBaseExceptionOf<OperationCanceledException>())
						throw;
					else
						throw new EonException(message: $"Ошибка деактивации компонента.{Environment.NewLine}\tКомпонент:{component.FmtStr().GNLI2()}", innerException: exception);
				}
			}

			// TODO: Put strings into the resources.
			//
			public override string ToString()
				=> $"Попытка активации:{Environment.NewLine}\tИД корреляции попытки:{_correlationId.Value.FmtStr().GNLI2()}{Environment.NewLine}\tНомер попытки:{_retryIndex.ToString("d").FmtStr().GNLI2()}{Environment.NewLine}\tКомпонент:{_component.FmtStr().GNLI2()}";

			protected override void FireBeforeDispose(bool explicitDispose) {
				if (explicitDispose)
					TryReadDA(ref _activateControl)?.StopAsync().WaitWithTimeout();
				//
				base.FireBeforeDispose(explicitDispose);
			}

			protected override void Dispose(bool explicitDispose) {
				if (explicitDispose) {
					_activateControl?.Dispose();
				}
				_component = null;
				_activateControl = null;
				//
				base.Dispose(explicitDispose);
			}

		}

		#endregion

		RetrySettings _retryOptions;

		RunControlFactoryDelegate<ActivationServant<TDescription>> _activateControlFactory;

		ILocalSubscription _appStartedSubscription;

		P_ComponentActivationAttempt _lastComponentActivation;

		public ActivationServant(IXAppScopeInstance scope, TDescription description)
			: base(scope: scope, description: description) { }

		protected sealed override RunControlOptions DefineDefaultOfActivateControlOptions()
			/* Важно! Изменение возвращаемого значения обязательно требует ревью кода этого компонента. */
			=> RunControlOptions.SingleStart;

		// TODO: Put strings into the resources.
		//
		protected override async Task OnInitializeAsync(IContext ctx = default) {
			const string logMessagePrologue = "Инициализация авто-активации.";
			//
			WriteDA(location: ref _retryOptions, value: Description.RetryOptions);
			var componentDescription = Description.Component;
			// Определение условий и возможности активации элемента (компонента).
			//
			var notActivateComponent = false;
			if (componentDescription.IsDisabled) {
				this.LogDisabilityWarning(description: componentDescription, logMessagePrologue: logMessagePrologue);
				notActivateComponent = true;
			}
			else if (!componentDescription.IsAutoActivationEnabled()) {
				this.LogAutoActivationDisabilityWarning(description: componentDescription, logMessagePrologue: logMessagePrologue);
				notActivateComponent = true;
			}
			//
			if (notActivateComponent)
				WriteDA(
					location: ref _activateControlFactory,
					value:
						(locOptions, locComponent, locAttemptState, locBeforeActivate, locActivate, locDeactivate, locDeactivationToken) 
						=>
						new RunControl<ActivationServant<TDescription>>(
							options: locOptions,
							component: locComponent,
							attemptState: locAttemptState,
							beforeStart: locBeforeActivate,
							start: locState => P_DoActivateAsync(state: locState, continuation: locActivate, componentActivationProhibited: true),
							stop: () => P_DoDeactivateAsync(continuation: locDeactivate, componentActivationProhibited: true),
							stopToken: locDeactivationToken));
			else
				WriteDA(
					location: ref _activateControlFactory,
					value:
						(locOptions, locComponent, locAttemptState, locBeforeActivate, locActivate, locDeactivate, locDeactivationToken) 
						=>
						new RunControl<ActivationServant<TDescription>>(
							options: locOptions,
							component: locComponent,
							attemptState: locAttemptState,
							beforeStart: locBeforeActivate,
							start: locState => P_DoActivateAsync(state: locState, continuation: locActivate, componentActivationProhibited: false),
							stop: () => P_DoDeactivateAsync(continuation: locDeactivate, componentActivationProhibited: false),
							stopToken: locDeactivationToken));
			//
			await base.OnInitializeAsync(ctx: ctx).ConfigureAwait(false);
		}

		async Task P_DoActivateAsync(IRunControlAttemptState state, Func<IRunControlAttemptState, Task> continuation, bool componentActivationProhibited) {
			state.EnsureNotNull(nameof(state));
			continuation.EnsureNotNull(nameof(continuation));
			//
			await continuation(arg: state).ConfigureAwait(false);
			if (!componentActivationProhibited) {
				var app = App;
				var appShutdownCancellationToken = app.AppShutdownToken;
				var appMessageFlow = app.AppMessageFlow;
				var appStartedSubscription = default(ILocalSubscription);
				try {
					// Подписка на получение уведомления о запуске приложения.
					//
					appStartedSubscription =
						await
						appMessageFlow
						.SubscribeAsync<IXAppStartedEventArgs>(
							publicationFilter: locEventArgs => locEventArgs.Cancel = !ReferenceEquals(AppDisposeTolerant, locEventArgs.Message.Payload.App),
							process: P_HandleAppStartedNotificationAsync)
						.ConfigureAwait(false);
					if (app.RunControl.IsStarted) {
						// Поскольку приложение уже запущено.
						//
						await appStartedSubscription.DeactivateAsync().ConfigureAwait(false);
						appStartedSubscription.Dispose();
						//
						if (!HasDeactivationRequested)
							// Поскольку активация элемента выполняется асинхронно, то при наличии запроса остановки (HasDeactivationRequested) не генерируется исключение OperationCanceledException, а просто не выполняется оставшаяся логика.
							//
							P_StartComponentActivationAttempt(
								retryOptions: ReadIA(ref _retryOptions, locationName: nameof(_retryOptions)),
								rethrowException: true,
								retryAttemptIndex: -1,
								ct: appShutdownCancellationToken,
								correlationId: XCorrelationId.New(),
								breakCondition: () => HasDeactivationRequested);
					}
					else
						WriteDA(ref _appStartedSubscription, appStartedSubscription);
				}
				catch (Exception exception) {
					itrlck.SetNullBool(ref _appStartedSubscription, comparand: appStartedSubscription);
					appStartedSubscription?.Dispose(exception);
					throw;
				}
			}
		}

		protected override Task DoActivateAsync(IRunControlAttemptState state)
			=> TaskUtilities.FromAction(action: delegate () { state.EnsureNotNull(nameof(state)); });

		async Task P_DoDeactivateAsync(Func<Task> continuation, bool componentActivationProhibited) {
			continuation.EnsureNotNull(nameof(continuation));
			//
			var caughtExceptions = new List<Exception>();
			try { await continuation().ConfigureAwait(false); }
			catch (Exception exception) { caughtExceptions.Add(exception); }
			if (!componentActivationProhibited) {
				var appStartedSubscription = TryReadDA(ref _appStartedSubscription);
				var lastComponentActivation = TryReadDA(ref _lastComponentActivation);
				if (!(appStartedSubscription is null))
					try {
						await appStartedSubscription.DeactivateAsync().ConfigureAwait(false);
						appStartedSubscription.Dispose();
					}
					catch (Exception exception) { caughtExceptions.Add(exception); }
				//
				if (!(lastComponentActivation is null))
					try {
						await lastComponentActivation.ActivateControl.StopAsync().ConfigureAwait(false);
						lastComponentActivation.Dispose();
					}
					catch (Exception firstException) {
						caughtExceptions.Add(firstException);
					}
			}
			//
			if (caughtExceptions.Count > 0)
				throw new AggregateException(innerExceptions: caughtExceptions);
		}

		protected override Task DoDeactivateAsync()
			=> TaskUtilities.FromVoidResult();

		protected override void CreateActivateControl(
			RunControlOptions options,
			ActivationServant<TDescription> component,
			RunControlAttemptStateFactory attemptState,
			Func<IRunControlAttemptState, Task> beforeActivate,
			Func<IRunControlAttemptState, Task> activate,
			Func<Task> deactivate,
			CancellationToken deactivationToken,
			out IRunControl<ActivationServant<TDescription>> control)
			=>
			control =
			ReadIA(ref _activateControlFactory, locationName: nameof(_activateControlFactory))
			(options: options, component: component, attemptState: attemptState, beforeStart: beforeActivate, start: activate, stop: deactivate, stopToken: deactivationToken);

		// TODO: Put strings into the resources.
		//
		IActivatableXAppScopeInstance P_CreateComponent() {
			try {
				return this.CreateAppScopeInstance<IActivatableXAppScopeInstance>(description: Description.Component);
			}
			catch (Exception exception) {
				throw
					new EonException(message: "Ошибка создания экземпляра активируемого компонента.", innerException: exception);
			}
		}

		// TODO: Put strings into the resources.
		//
		void P_StartComponentActivationAttempt(
			RetrySettings retryOptions,
			bool rethrowException,
			int retryAttemptIndex,
			XFullCorrelationId correlationId,
			CancellationToken ct,
			Func<bool> breakCondition = default) {
			//
			retryOptions = retryOptions.EnsureNotNull(nameof(retryOptions)).AsReadOnly().EnsureValid();
			//
			if (breakCondition is null)
				breakCondition = () => ct.IsCancellationRequested || HasDeactivationRequested || !IsActive;
			if (!breakCondition()) {
				var logMessagePrologue = $"Отложенная активация. ИД корреляции: {correlationId.FmtStr().G()}.";
				try {
					IActivatableXAppScopeInstance componentNewInstance = default;
					P_ComponentActivationAttempt newActivation = default;
					P_ComponentActivationAttempt existingActivation = default;
					for (; ; ) {
						existingActivation = TryReadDA(ref _lastComponentActivation, considerDisposeRequest: true);
						if (existingActivation is null) {
							if (breakCondition()) {
								cleanup(ref componentNewInstance, ref newActivation);
								break;
							}
							else {
								newActivation =
									newActivation
									?? new P_ComponentActivationAttempt(
											retryIndex: retryAttemptIndex,
											component: componentNewInstance = P_CreateComponent(),
											correlationId: correlationId,
											ct: ct);
								if (itrlck.UpdateBool(location: ref _lastComponentActivation, value: newActivation, comparand: existingActivation)) {
									if (breakCondition()) {
										itrlck.SetNullBool(location: ref _lastComponentActivation, comparand: newActivation);
										cleanup(ref componentNewInstance, ref newActivation);
									}
									break;
								}
								else
									continue;
							}
						}
						else {
							cleanup(ref componentNewInstance, ref newActivation);
							break;
						}
					}
					//
					if (!(newActivation is null)) {
						newActivation
							.ActivateControl
							.StartAsync()
							.ContinueWith(
								 continuationAction:
									locTask => {
										var locCaughtException = default(Exception);
										try {
											if (locTask.IsCanceled || locTask.IsFaulted)
												itrlck.SetNullBool(location: ref _lastComponentActivation, comparand: newActivation);
											//
											P_HandleComponentActivationAttemptResult(
												retryOptions: retryOptions,
												component: componentNewInstance,
												retryAttemptIndex: newActivation.RetryAttemptIndex,
												activationTask: locTask,
												correlationId: correlationId,
												ct: ct);
										}
										catch (Exception locException) {
											locCaughtException = locException;
											throw;
										}
										finally {
											if (locTask.IsCanceled || locTask.IsFaulted) {
												try {
													cleanup(ref componentNewInstance, ref newActivation);
												}
												catch (Exception locException) {
													if (locCaughtException is null)
														throw;
													else
														throw new AggregateException(locCaughtException, locException);
												}
											}
										}
									},
								 continuationOptions: TaskContinuationOptions.PreferFairness);
					}
				}
				catch (Exception exception) {
					if (rethrowException)
						throw new EonException(message: $"Ошибка запуска попытки активации.{Environment.NewLine}\tКомпонент:{this.FmtStr().GNLI2()}", innerException: exception);
					else {
#if !DO_NOT_USE_EON_LOGGING_API
						this
							.IssueError(
								messagePrologue: logMessagePrologue,
								message: "Ошибка запуска попытки активации.",
								error: exception,
								includeErrorInIssueFaultException: true,
								severityLevel: exception.GetMostHighSeverityLevel(baseLevel: SeverityLevel.Medium));
#endif
					}
				}
			}
			//
			void cleanup(ref IActivatableXAppScopeInstance locTarget, ref P_ComponentActivationAttempt locActivation) {
				var locCaughtExceptions = new List<Exception>();
				try { locTarget?.Dispose(); locTarget = null; }
				catch (Exception locException) { locCaughtExceptions.Add(locException); }
				try { locActivation?.ActivateControl.StopAsync().WaitWithTimeout(); locActivation?.Dispose(); locActivation = null; }
				catch (Exception locException) { locCaughtExceptions.Add(locException); }
				if (locCaughtExceptions.Count > 0)
					throw new AggregateException(innerExceptions: locCaughtExceptions);
			}
		}

		// TODO: Put strings into the resources.
		//
		void P_HandleComponentActivationAttemptResult(
			RetrySettings retryOptions,
			IActivatableXAppScopeInstance component,
			int retryAttemptIndex,
			Task activationTask,
			XFullCorrelationId correlationId,
			CancellationToken ct) {
			//
			retryOptions = retryOptions.EnsureNotNull(nameof(retryOptions)).AsReadOnly().EnsureValid();
			component.EnsureNotNull(nameof(component));
			retryAttemptIndex.Arg(nameof(retryAttemptIndex)).EnsureNotLessThan(-1);
			activationTask.EnsureNotNull(nameof(activationTask));
			//
			var logMessagePrologue = $"Отложенная активация. ИД корреляции: {correlationId.FmtStr().G()}.";
			if (activationTask.IsFaulted || activationTask.IsCanceled) {
				if (activationTask.IsCanceled) {
#if !DO_NOT_USE_EON_LOGGING_API
					this
						.IssueWarning(
							messagePrologue: logMessagePrologue,
							message: $"Активация компонента была прервана или отменена.{Environment.NewLine}Повторная попытка не предусмотрена опциями активации компонента.{Environment.NewLine}\tКомпонент:{component.FmtStr().GNLI2()}",
							severityLevel: SeverityLevel.Medium);
#endif
				}
				else {
					string retryActivationLogMessage;
					bool doRetryActivation;
					if (retryOptions.IsDisabled) {
						doRetryActivation = false;
						retryActivationLogMessage = $"Повторная попытка активации компонента запрещена настройками активации (см. '{nameof(RetrySettings)}.{nameof(RetrySettings.IsDisabled)}').";
					}
					else if (retryOptions.MaxCount == -1) {
						doRetryActivation = true;
						retryActivationLogMessage = $"Повторная попытка активации компонента будет выполнена {(retryOptions.Interval == TimeoutDuration.Zero ? "немедленно" : $"через '{retryOptions.Interval.ToString()}'")}.";
					}
					else if (retryOptions.MaxCount == 0) {
						doRetryActivation = false;
						retryActivationLogMessage = $"Повторная попытка не предусмотрена настройками активации компонента (см. '{nameof(RetrySettings)}.{nameof(RetrySettings.MaxCount)}').";
					}
					else if ((retryAttemptIndex + 1) < retryOptions.MaxCount) {
						doRetryActivation = true;
						retryActivationLogMessage = $"Повторная попытка активации компонента ({retryAttemptIndex + 2:d} из {retryOptions.MaxCount:d}) будет выполнена {(retryOptions.Interval == TimeoutDuration.Zero ? "немедленно" : $"через '{retryOptions.Interval.ToString()}'")}.";
					}
					else {
						doRetryActivation = false;
						retryActivationLogMessage = $"Повторной попытки активации компонента не будет, так как все попытки исчерпаны (кол-во попыток {retryOptions.MaxCount:d}).";
					}
#if !DO_NOT_USE_EON_LOGGING_API
					this
						.IssueError(
							messagePrologue: logMessagePrologue,
							message: $"Ошибка активации компонента.{Environment.NewLine}{retryActivationLogMessage}{Environment.NewLine}\tКомпонент:{Environment.NewLine}{component.ToString().IndentLines2()}",
							error: activationTask.Exception,
							includeErrorInIssueFaultException: true,
							severityLevel: activationTask.Exception.GetMostHighSeverityLevel(baseLevel: SeverityLevel.Medium));
#endif
					//
					if (doRetryActivation) {
						if (retryOptions.Interval == TimeoutDuration.Zero)
							P_StartComponentActivationAttempt(retryOptions: retryOptions, rethrowException: false, retryAttemptIndex: retryAttemptIndex + 1, correlationId: correlationId, ct: ct);
						else
							TaskUtilities
								.Delay(duration: retryOptions.Interval)
								.ContinueWith(
									continuationAction:
										locDelayTask
										=>
										P_StartComponentActivationAttempt(
											retryOptions: retryOptions,
											rethrowException: false,
											retryAttemptIndex: retryAttemptIndex + 1,
											ct: ct,
											correlationId: correlationId),
									continuationOptions: TaskContinuationOptions.PreferFairness | TaskContinuationOptions.OnlyOnRanToCompletion,
									cancellationToken: ct,
									scheduler: TaskScheduler.Default);
					}
				}
			}
			else {
#if !DO_NOT_USE_EON_LOGGING_API
				this
					.IssueInformation(
						messagePrologue: logMessagePrologue,
						message: $"Активация компонента успешно выполнена.{Environment.NewLine}\tКомпонент:{component.FmtStr().GNLI2()}",
						severityLevel: SeverityLevel.Medium);
#endif
			}
		}

		// TODO: Put strings into the resources.
		//
		Task P_HandleAppStartedNotificationAsync(ILocalSubscription subscription, ILocalMessage<IXAppStartedEventArgs> message, IContext ctx) {
			try {
				subscription.EnsureNotNull(nameof(subscription));
				message.EnsureNotNull(nameof(message));
				ctx.EnsureNotNull(nameof(ctx));
				//
				if (ctx.IsCancellationRequested())
					return Task.FromCanceled(cancellationToken: ctx.Ct());
				else if (HasDeactivationRequested || !IsActive || !subscription.IsActive || !ReferenceEquals(subscription, TryReadDA(ref _appStartedSubscription)))
					return Task.CompletedTask;
				else {
					var retryOptions = TryReadDA(ref _retryOptions, considerDisposeRequest: true);
					if (retryOptions is null) {
						if (!IsDisposeRequested)
							EnsureInitialized();
						return TaskUtilities.FromCanceled();
					}
					else {
#if !DO_NOT_USE_EON_LOGGING_API
						var logMessagePrologue = $"Отложенная активация. ИД корреляции: {ctx.FullCorrelationId}.";
						this
							.IssueInformation(
								messagePrologue: logMessagePrologue,
								message: "Получено уведомление о запуске приложения. Начало активации.",
								severityLevel: SeverityLevel.Medium);
#endif
						P_StartComponentActivationAttempt(retryOptions: retryOptions, rethrowException: false, retryAttemptIndex: -1, correlationId: ctx.FullCorrelationId, ct: message.Payload.App.AppShutdownToken);
						return Task.CompletedTask;
					}
				}
			}
			catch (Exception exception) {
				return Task.FromException(exception);
			}
		}

		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose) {
				TryReadDA(ref _appStartedSubscription)?.DeactivateAsync().WaitWithTimeout();
			}
			//
			base.FireBeforeDispose(explicitDispose);
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_appStartedSubscription?.Dispose();
				_lastComponentActivation?.Dispose();
			}
			_appStartedSubscription = null;
			_retryOptions = null;
			_lastComponentActivation = null;
			_activateControlFactory = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}