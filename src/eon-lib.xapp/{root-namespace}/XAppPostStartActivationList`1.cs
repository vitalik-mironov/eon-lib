#region Compilation conditional symbols

#define DO_NOT_USE_EON_LOGGING_API

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Description;
using Eon.MessageFlow.Local;
using Eon.Threading.Tasks;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon {

	public class XAppPostStartActivationList<TDescription>
		:ActivationList<TDescription>
		where TDescription : class, IActivationListDescription {

		ILocalSubscription _appStartedSubscription;

		public XAppPostStartActivationList(IXAppScopeInstance scope, TDescription description)
			: base(scope: scope, description: description) { }

		protected override async Task OnInitializeAsync(IContext ctx = default) {
			var subscription = default(ILocalSubscription);
			try {
				if (!Description.OmitItemsActivation) {
					// Подписка на получение уведомления о запуске приложения.
					//
					subscription = await App.AppMessageFlow.SubscribeAsync<IXAppStartedEventArgs>(publicationFilter: locEventArgs => locEventArgs.Cancel = !ReferenceEquals(AppDisposeTolerant, locEventArgs.Message.Payload.App), process: P_HandleAppStartedNotificationAsync).ConfigureAwait(false);
					WriteDA(ref _appStartedSubscription, subscription);
				}
				//
				await base.OnInitializeAsync(ctx: ctx).ConfigureAwait(false);
			}
			catch (Exception exception) {
				itrlck.SetNullBool(ref _appStartedSubscription, subscription);
				subscription?.Dispose(exception);
				throw;
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
				else if (HasDeactivationRequested || !subscription.IsActive || !ReferenceEquals(subscription, TryReadDA(ref _appStartedSubscription)))
					return TaskUtilities.FromCanceled();
				else {
#if !DO_NOT_USE_EON_LOGGING_API
					var logMessagePrologue = $"Отложенная активация. ИД корреляции: {context.FullCorrelationId}.";
					this
						.IssueInformation(
							messagePrologue: logMessagePrologue,
							message: "Получено уведомление о запуске приложения. Начало активации.",
							severityLevel: SeverityLevel.Lowest);
#endif
#if DO_NOT_USE_EON_LOGGING_API
					TaskUtilities.RunOnDefaultScheduler(factory: () => ActivateAsync(ctx: null));
#else
					var doActivationTask = TaskUtilities.RunOnDefaultScheduler(factory: () => ActivateAsync(ctx: null));
					doActivationTask
						.ContinueWith(
							continuationAction:
								locTask => {
									if (locTask.IsFaulted) {
										var locTaskException = locTask.Exception.Flatten();
										this
											.IssueError(
												messagePrologue: logMessagePrologue,
												message: "Сбой активации.",
												error: locTaskException,
												severityLevel: locTaskException.GetMostHighSeverityLevel(baseLevel: SeverityLevel.Lowest),
												includeErrorInIssueFaultException: true);
									}
									else if (locTask.IsCanceled)
										this.IssueWarning(messagePrologue: logMessagePrologue, message: "Активация была прервана или отменена.", severityLevel: SeverityLevel.Lowest);
									else
										this.IssueInformation(messagePrologue: logMessagePrologue, message: "Активация успешно выполнена.", severityLevel: SeverityLevel.Lowest);
								},
							continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
#endif
					return Task.CompletedTask;
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
			}
			_appStartedSubscription = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}