#region Compilation conditional symbols

#define DO_NOT_USE_EON_LOGGING_API

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel;
using Eon.Context;
using Eon.Description;
using Eon.Linq;
using Eon.Threading.Tasks;

namespace Eon {

	public class ActivationList<TDescription>
		:ActivatableXAppScopeInstanceBase<TDescription, ActivationList<TDescription>>, IActivationList<TDescription>
		where TDescription : class, IActivationListDescription {

		IActivatableXAppScopeInstance[ ] _activatableItems;

		IActivatableXAppScopeInstance[ ] _activatedItems;

		public ActivationList(IXAppScopeInstance scope, TDescription description)
			: base(scope: scope, description: description) { }

		public IEnumerable<IActivatableXAppScopeInstance> ActivatableItems
			=> EnumerateDA(ReadIA(location: ref _activatableItems, locationName: nameof(_activatableItems)));

		IRunControl<IActivationList<TDescription>> IActivatableXAppScopeInstance<IActivationList<TDescription>>.ActivateControl
			=> ActivateControl;

		protected sealed override RunControlOptions DefineDefaultOfActivateControlOptions()
			/* Важно! Изменение возвращаемого значения обязательно требует ревью кода этого компонента. */
			=> RunControlOptions.SingleStart;

		// TODO: Put strings into the resources.
		//
		protected override async Task OnInitializeAsync(IContext ctx = default) {
			const string logMessagePrologue = "Инициализация активации.";
			//
			List<IActivatableXAppScopeInstance> activatableItems = default;
			try {
				if (Description.OmitItemsActivation) {
#if !DO_NOT_USE_EON_LOGGING_API
					this
						.IssueWarning(
							message: $"Описанием (конфигурацией) данного списка указано не производить активацию входящих в этот список компонентов (см. '{nameof(Description)}.{nameof(Description.OmitItemsActivation)}')",
							severityLevel: SeverityLevel.Lowest);
#endif
					WriteDA(location: ref _activatableItems, value: new IActivatableXAppScopeInstance[ ] { });
				}
				else {
					var activatableItemsDescriptions = Description.ActivatableItems.ToArray();
					activatableItems = new List<IActivatableXAppScopeInstance>();
					for (var i = 0; i < activatableItemsDescriptions.Length; i++) {
						var activatableItemDescription = activatableItemsDescriptions[ i ];
						if (activatableItemDescription.IsDisabled())
							this.LogDisabilityWarning(description: activatableItemDescription, logMessagePrologue: logMessagePrologue);
						else if (!activatableItemDescription.IsAutoActivationEnabled())
							this.LogAutoActivationDisabilityWarning(description: activatableItemDescription, logMessagePrologue: logMessagePrologue);
						else
							activatableItems.Add(await this.CreateInitializeAppScopeInstanceAsync<IActivatableXAppScopeInstance>(description: activatableItemDescription, ctx: ctx));
					}
					WriteDA(location: ref _activatableItems, value: activatableItems.ToArray());
				}
				//
				await base.OnInitializeAsync(ctx: ctx).ConfigureAwait(false);
			}
			catch (Exception exception) {
				activatableItems.DisposeMany(exception);
				throw;
			}
		}

		// TODO: Put strings into the resources.
		//
		protected override Task DoActivateAsync(IRunControlAttemptState state) {
#if !DO_NOT_USE_EON_LOGGING_API
			const string logMessagePrologue = "Активация.";
#endif
			//
			try {
				state.EnsureNotNull(nameof(state));
				//
				if (HasDeactivationRequested)
					return TaskUtilities.FromCanceled();
				else
					return TaskUtilities.RunOnDefaultScheduler(action: doActivateParallel, cancellationToken: CancellationToken.None, state: state);
			}
			catch (Exception exception) {
				return TaskUtilities.FromError(error: exception);
			}
			//
			void doActivateParallel(IRunControlAttemptState locState) {
				locState.EnsureNotNull(nameof(locState));
				//
				var locActivatedItems = new List<IActivatableXAppScopeInstance>();
				try {
					foreach (var locActivatable in ActivatableItems) {
						// Мониторинг запроса отмены операции.
						//
						if (HasDeactivationRequested)
							throw new OperationCanceledException();
						// Активация компонента.
						//
						try {
#if !DO_NOT_USE_EON_LOGGING_API
							this
								.IssueInformation(
									messagePrologue: logMessagePrologue,
									message: $"Активация компонента.{Environment.NewLine}\tКомпонент:{Environment.NewLine}{locActivatable.ToString().IndentLines2()}",
									severityLevel: SeverityLevel.Lowest);
#endif
							if (!locActivatable.IsAutoActivationEnabled) {
#if !DO_NOT_USE_EON_LOGGING_API
								this
									.IssueWarning(
										messagePrologue: logMessagePrologue,
										message: $"Активация компонента не выполнена, так как активация отключена самим компонентом (см. '{nameof(locActivatable.IsAutoActivationEnabled)}').{Environment.NewLine}\tОбъект:{locActivatable.FmtStr().GNLI2()}",
										severityLevel: SeverityLevel.Lowest);
#endif
								continue;
							}
							locActivatable
								.ActivateControl
								.StartAsync(options: TaskCreationOptions.AttachedToParent)
#if !DO_NOT_USE_EON_LOGGING_API
								.ContinueWith(
									continuationAction:
										locItemActivationTask => {
											if (locItemActivationTask.IsFaulted)
												this
													.IssueError(
														messagePrologue: logMessagePrologue,
														message: $"Ошибка активации компонента.{Environment.NewLine}\tКомпонент:{Environment.NewLine}{locActivatable.ToString().IndentLines2()}",
														error: locItemActivationTask.Exception,
														includeErrorInIssueFaultException: true,
														severityLevel: locItemActivationTask.Exception.GetMostHighSeverityLevel(baseLevel: SeverityLevel.Lowest));
											else if (locItemActivationTask.IsCanceled)
												this
												.IssueWarning(
													messagePrologue: logMessagePrologue,
													message: $"Активация компонента была прервана или отменена.{Environment.NewLine}\tКомпонент:{locActivatable.FmtStr().GNLI2()}",
													severityLevel: SeverityLevel.Lowest);
											else
												this
													.IssueInformation(
														messagePrologue: logMessagePrologue,
														message: $"Активация компонента успешно выполнена.{Environment.NewLine}\tКомпонент:{locActivatable.FmtStr().GNLI2()}",
														severityLevel: SeverityLevel.Lowest);
										},
									continuationOptions: TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously)
#endif
							;
							//
							locActivatedItems.Add(locActivatable);
						}
						catch {
#if !DO_NOT_USE_EON_LOGGING_API
							this
								.IssueError(
									messagePrologue: logMessagePrologue,
									message: $"Ошибка активации компонента.{Environment.NewLine}\tКомпонент:{locActivatable.FmtStr().GNLI2()}",
									error: exception,
									severityLevel: exception.GetMostHighSeverityLevel(baseLevel: SeverityLevel.Lowest),
									includeErrorInIssueFaultException: true);
#endif
							throw;
						}
					}
					WriteDA(ref _activatedItems, locActivatedItems.ToArray());
				}
				catch (Exception exception) {
					var caughtExceptions = new List<Exception>();
					foreach (var activatedItem in locActivatedItems)
						try { activatedItem.ActivateControl.StopAsync(TaskCreationOptions.AttachedToParent); }
						catch (Exception secondException) { caughtExceptions.Add(secondException); }
					//
					if (caughtExceptions.Count > 0)
						throw new AggregateException(exception.Sequence().Concat(caughtExceptions));
					else
						throw;
				}
			}
		}

		// TODO: Put strings into the resources.
		//
		protected override Task DoDeactivateAsync() {
#if !DO_NOT_USE_EON_LOGGING_API
			const string logMessagePrologue = "Деактивация.";
#endif
			//
			return TaskUtilities.RunOnDefaultScheduler(action: doDeactivate, ct: CancellationToken.None);
			//
			void doDeactivate() {
				foreach (var activatedItem in EnumerateDA(location: ref _activatedItems)) {
					try {
#if !DO_NOT_USE_EON_LOGGING_API
						this
							.IssueInformation(
								messagePrologue: logMessagePrologue,
								message: $"Деактивация компонента.{Environment.NewLine}\tКомпонент:{activatedItem.FmtStr().GNLI2()}",
								severityLevel: SeverityLevel.Lowest);
#endif
						activatedItem
							.ActivateControl
							.StopAsync(options: TaskCreationOptions.AttachedToParent)
#if !DO_NOT_USE_EON_LOGGING_API
							.ContinueWith(
								continuationAction:
									locDeactivationTask => {
										if (locDeactivationTask.IsFaulted)
											this
												.IssueError(
													messagePrologue: logMessagePrologue,
													message: $"Ошибка деактивации компонента.{Environment.NewLine}\tКомпонент:{activatedItem.FmtStr().GNLI2()}",
													error: locDeactivationTask.Exception,
													includeErrorInIssueFaultException: true,
													severityLevel: locDeactivationTask.Exception.GetMostHighSeverityLevel(baseLevel: SeverityLevel.Lowest));
										else if (locDeactivationTask.IsCanceled)
											this
												.IssueWarning(
													messagePrologue: logMessagePrologue,
													message: $"Деактивация компонента была прервана или отменена.{Environment.NewLine}\tКомпонент:{activatedItem.FmtStr().GNLI2()}",
													severityLevel: SeverityLevel.Lowest);
										else
											this
												.IssueInformation(
													messagePrologue: logMessagePrologue,
													message: $"Деактивация компонента успешно выполнена.{Environment.NewLine}\tКомпонент:{activatedItem.FmtStr().GNLI2()}",
													severityLevel: SeverityLevel.Lowest);
									},
								continuationOptions: TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously)
#endif
							;
					}
					catch {
#if !DO_NOT_USE_EON_LOGGING_API
						this
							.IssueError(
								messagePrologue: logMessagePrologue,
								message: $"Ошибка деактивации компонента.{Environment.NewLine}\tКомпонент:{activatedItem.FmtStr().GNLI2()}",
								error: exception,
								severityLevel: exception.GetMostHighSeverityLevel(baseLevel: SeverityLevel.Lowest),
								includeErrorInIssueFaultException: true);
#endif
						throw;
					}
				}
			}
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_activatableItems.DisposeAndClearArray();
			}
			_activatableItems = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}