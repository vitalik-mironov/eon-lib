using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel;
using Eon.Context;
using Eon.Linq;
using Eon.MessageFlow.Local.Internal;
using Eon.Threading;
using Eon.Threading.Tasks;

using static Eon.DisposableUtilities;
using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.MessageFlow.Local {

#pragma warning disable IDE0034 // Simplify 'default' expression

	public partial class LocalPublisher
		:DisposeNotifying, ILocalPublisher {

#if TRG_NETFRAMEWORK
		const int __PostingQueueLengthWarning = 4096;
#endif

		#region Nested types

		sealed class P_NewSubscriptionRegistrationState {

			public readonly ILocalSubscription Subscription;

			public readonly TaskCompletionSource<ILocalSubscription> SubscribeTaskProxy;

			internal P_NewSubscriptionRegistrationState(ILocalSubscription subscription) {
				subscription.EnsureNotNull(nameof(subscription));
				//
				Subscription = subscription;
				SubscribeTaskProxy = new TaskCompletionSource<ILocalSubscription>(TaskContinuationOptions.None);
			}

		}

		#endregion

		#region Static members

#if DEBUG
		static readonly TimeSpan __PostingOperationDurationWarning = TimeSpan.MaxValue;
#else
		static readonly TimeSpan __PostingOperationDurationWarning = TimeSpan.FromSeconds(60.0);
#endif

		#endregion

		ILocalPublisherSettings _runningSettings;

		PrimitiveSpinLock _subscriptionsSpinLock;

		List<ILocalSubscription> _subscriptionsList;

		IDictionary<ILocalSubscription, P_NewSubscriptionRegistrationState> _subscriptionsDictionary;

		Queue<LocalPostingQueueEntry> _postingQueue;

		PrimitiveSpinLock _postingQueueSpinLock;

		LocalPostingWorker[ ] _postingWorkers;

		RunControl<LocalPublisher> _runControl;

		CancellationTokenSource _postingCts;

		public LocalPublisher(ILocalPublisherSettings settings) {
			settings.EnsureNotNull(nameof(settings)).EnsureReadOnly().EnsureValid().EnsureNotDisabled();
			//
			_runningSettings = settings;
			_subscriptionsSpinLock = new PrimitiveSpinLock();
			_subscriptionsList = new List<ILocalSubscription>();
			_subscriptionsDictionary = new Dictionary<ILocalSubscription, P_NewSubscriptionRegistrationState>();
			_postingQueue = new Queue<LocalPostingQueueEntry>();
			_postingQueueSpinLock = new PrimitiveSpinLock();
			_postingCts = new CancellationTokenSource();
			_postingWorkers =
				EnumerableUtilities
				.CreateSequence(
					factory: locIndex => new LocalPostingWorker(queue: _postingQueue, queueSpinLock: _postingQueueSpinLock, cancellationToken: _postingCts.Token),
					length: _runningSettings.PostingDop)
				.ToArray();
			_runControl = new RunControl<LocalPublisher>(options: RunControlOptions.SingleStart, component: this, start: P_DoStartAsync, stop: P_DoStopAsync);
		}

		async Task<ILocalMessagePostingToken> P_PublishMessageAsync(ILocalMessage msg, bool disposeMessageAtEndOfPosting) {
			msg.EnsureNotNull(nameof(msg));
			//
			ILocalMessagePostingToken postingToken;
			IRunControl runControl = ReadDA(ref _runControl);
			IList<ILocalSubscription> subscriptions = ReadDA(ref _subscriptionsSpinLock).Invoke(() => ReadDA(ref _subscriptionsList).Where(i => i.IsActive).ToArray());
			subscriptions = await P_PublicationFilterAsync(publisherRunControl: runControl, msg: msg, sourceSubscriptions: subscriptions).ConfigureAwait(false);
			if (subscriptions.Count > 0 && !runControl.HasStopRequested) {
				var postingTokenStricted = new LocalPostingToken(message: msg, postingCount: subscriptions.Count, disposeMessageAtEndOfPosting: disposeMessageAtEndOfPosting);
				postingToken = postingTokenStricted;
				var postingQueueEntry = default(LocalPostingQueueEntry);
				try {
					postingQueueEntry = new LocalPostingQueueEntry(postingToken: postingTokenStricted, postingSubscriptions: subscriptions);
					var postingQueueLength =
						ReadDA(ref _postingQueueSpinLock)
						.Invoke(
							() => {
								var locPostingQueue = ReadDA(ref _postingQueue);
								locPostingQueue.Enqueue(postingQueueEntry);
								return locPostingQueue.Count;
							});
#if TRG_NETFRAMEWORK
					if (postingQueueLength >= __PostingQueueLengthWarning) {
						try {
							throw
								new InvalidOperationException(
									message: $"Длина очереди доставки сообщений достигла предела предупреждения '{__PostingQueueLengthWarning:d}' и составляет '{postingQueueLength:d}'.{Environment.NewLine}Слишком большая очередь доставки сообщений свидетельствует о некорректной работе компонентов, выполняющих доставку сообщений, либо о слишком большом потоке сообщений.{Environment.NewLine}Данное событие не прерывает работу, если оно успешно записано в системый журнал событий.");
						}
						catch (Exception firstException) {
							WindowsEventLogUtilities.WriteFaultToEventLog(fault: firstException, failFastOnError: true, faultFormattingOptions: ExceptionInfoFormattingOptions.Full);
						}
					}
#else
					// TODO_HIGH: Implement logging (as for TRG_NETFRAMEWORK).
					//
					// ...
#endif
					// Уведомление воркеров (рабочих элементов, обрабатывающих доставку сообщений).
					//
					var postingWorkers = ReadDA(ref _postingWorkers);
					for (var i = 0; i < postingWorkers.Length; i++) {
						ILocalPostingWorker postingWorker = ReadDA(ref postingWorkers[ i ]);
						postingWorker.BreakLoopIdle();
#if TRG_NETFRAMEWORK
						if (!postingWorker.IsLoopAlive) {
							if (!runControl.HasStopRequested)
								try {
									throw
										new InvalidOperationException(
											message: $"Один из рабочих элементов (#{(i + 1):d} из {postingWorkers.Length:d}), выполняющий доставку сообщений, находится в нерабочем состоянии.{Environment.NewLine}Данное событие не прерывает работу, если оно успешно записано в системый журнал событий.");
								}
								catch (Exception firstException) {
									WindowsEventLogUtilities.WriteFaultToEventLog(fault: firstException, failFastOnError: true, faultFormattingOptions: ExceptionInfoFormattingOptions.Full);
								}
						}
						//
						TimeSpan? currentPostingDuration;
						if ((currentPostingDuration = postingWorker.CurrentlyPostingDuration) >= __PostingOperationDurationWarning) {
							var currentPostingSubscription = postingWorker.CurrentlyPostingSubscription;
							try {
								throw
									new InvalidOperationException(
										message: $"Время выполнения операции доставки, выполняемой одним из рабочих элементов (#{(i + 1):d} из {postingWorkers.Length:d}), достигло предела предупреждения '{__PostingOperationDurationWarning.ToString("c")}' и составляет '{currentPostingDuration.Value.ToString("c")}'.{Environment.NewLine}\tПодписка, согласно которой выполняется доставка:{currentPostingSubscription.FmtStr().GNLI2()}{Environment.NewLine}Данное событие не прерывает работу, если оно успешно записано в системый журнал событий.{Environment.NewLine}Указанное событие сообщает о том, что один из рабочих элементов уже слишком долго выполняет операцию доставки, что не является нормальным.");
							}
							catch (Exception firstException) {
								WindowsEventLogUtilities.WriteFaultToEventLog(fault: firstException, failFastOnError: true, faultFormattingOptions: ExceptionInfoFormattingOptions.Full);
							}
						}
#else
						// TODO_HIGH: Implement logging (as for TRG_NETFRAMEWORK).
						//
						// ...
#endif
					}
				}
				catch (Exception exception) {
					postingQueueEntry?.Dispose(exception);
					throw;
				}
			}
			else {
				postingToken = new LocalPostingToken(message: msg, disposeMessageAtEndOfPosting: disposeMessageAtEndOfPosting);
				if (disposeMessageAtEndOfPosting)
					msg.Dispose();
			}
			return postingToken;
		}

		public async Task<ILocalMessagePostingToken> PublishMessageAsync(ILocalMessage message, bool disposeMessageAtEndOfPosting)
			// TODO_HIGH: To implement the forcing of new task for publication.
			//
			=> await P_PublishMessageAsync(message, disposeMessageAtEndOfPosting).ConfigureAwait(false);

		public virtual Task<ILocalMessagePostingToken> PublishAsync<TPayload>(TPayload payload, bool disposePayloadAtEndOfPosting)
			=> PublishMessageAsync(message: new LocalMessage<TPayload>(payload, ownsPayload: disposePayloadAtEndOfPosting), disposeMessageAtEndOfPosting: true);

		public virtual Task<ILocalMessagePostingToken> PublishAsync<TPayload>(TPayload payload)
			=> PublishAsync(payload: payload, disposePayloadAtEndOfPosting: false);

		// TODO: Put strings into the resources.
		//
		public Task<ILocalSubscription> SubscribeAsync(ILocalSubscription subscription) {
			try {
				subscription.EnsureNotNull(nameof(subscription));
				if (!ReferenceEquals(subscription.Publisher, this))
					throw
						new ArgumentOutOfRangeException(
							paramName: nameof(subscription),
							message: $"Subscription is not associated with this publisher.{Environment.NewLine}\tSubscription:{subscription.FmtStr().GNLI2()}{Environment.NewLine}\tPublisher:{this.FmtStr().GNLI2()}");
				//
				var spinLock = ReadDA(ref _subscriptionsSpinLock);
				var subscriptionsDictionary = ReadDA(ref _subscriptionsDictionary);
				var subscriptionsList = ReadDA(ref _subscriptionsList);
				var existingSubscriptionRegistration = default(P_NewSubscriptionRegistrationState);
				//
				if (!spinLock.Invoke(() => subscriptionsDictionary.TryGetValue(subscription, out existingSubscriptionRegistration))) {
					var newSubscriptionRegistration = default(P_NewSubscriptionRegistrationState);
					try {
						newSubscriptionRegistration = new P_NewSubscriptionRegistrationState(subscription);
						spinLock
							.Invoke(
								() => {
									EnsureNotDisposeState();
									//
									if (subscriptionsDictionary.ContainsKey(subscription))
										existingSubscriptionRegistration = subscriptionsDictionary[ subscription ];
									else {
										subscriptionsDictionary.Add(subscription, newSubscriptionRegistration);
										existingSubscriptionRegistration = newSubscriptionRegistration;
										subscriptionsList.Add(newSubscriptionRegistration.Subscription);
									}
								});
						if (existingSubscriptionRegistration == newSubscriptionRegistration)
							subscription
								.ActivateAsync()
								.ContinueWith(
									locActivateTask => {
										if (locActivateTask.IsCanceled)
											newSubscriptionRegistration.SubscribeTaskProxy.TrySetCanceled();
										else if (locActivateTask.IsFaulted) {
											if (!newSubscriptionRegistration.SubscribeTaskProxy.TrySetException(locActivateTask.Exception))
												throw new AggregateException(locActivateTask.Exception);
										}
										else
											newSubscriptionRegistration.SubscribeTaskProxy.TrySetResult(newSubscriptionRegistration.Subscription);
									},
									TaskContinuationOptions.ExecuteSynchronously);
					}
					catch (Exception firstException) {
						newSubscriptionRegistration?.SubscribeTaskProxy.TrySetException(firstException);
						throw;
					}
					finally {
						if (newSubscriptionRegistration != existingSubscriptionRegistration)
							newSubscriptionRegistration?.SubscribeTaskProxy.TrySetCanceled();
					}
				}
				//
				return existingSubscriptionRegistration.SubscribeTaskProxy.Task;
			}
			catch (Exception firstException) {
				return TaskUtilities.FromError<ILocalSubscription>(firstException);
			}
		}

		public virtual async Task<ILocalSubscription> SubscribeAsync<TData>(ProcessLocalMessage<TData> process, LocalPublicationFilter<TData> publicationFilter = null, LocalSubscriptionStates subscriptionInitialState = default) {
			var subscription = default(ILocalSubscription);
			var subscriber = default(ILocalSubscriber);
			try {
				subscriber = new DelegateLocalSubscriber<TData>(process: process, publicationFilter: publicationFilter);
				subscription = new LocalSubscription(publisher: this, subscriber: subscriber, initialState: LocalSubscriptionStates.OwnsSubscriber | LocalSubscriptionStates.PublisherOwnsSubscription | subscriptionInitialState);
				return await SubscribeAsync(subscription: subscription).ConfigureAwait(false);
			}
			catch (Exception exception) {
				DisposeMany(exception, subscriber, subscription);
				throw;
			}
		}

		public Task<ILocalSubscription> SubscribeAsync<TData>(ProcessLocalMessage<TData> processMessage, LocalPublicationFilter<TData> publicationFilter)
			=> SubscribeAsync(process: processMessage, publicationFilter: publicationFilter, subscriptionInitialState: LocalSubscriptionStates.None);

		public Task<ILocalSubscription> SubscribeAsync<TData>(ProcessLocalMessage<TData> processMessage)
			=> SubscribeAsync(process: processMessage, publicationFilter: null, subscriptionInitialState: LocalSubscriptionStates.None);

		public Task<ILocalSubscription> SubscribeAsync<TData>(ProcessLocalMessage<TData> processMessage, LocalSubscriptionStates state = default(LocalSubscriptionStates))
			=> SubscribeAsync(process: processMessage, publicationFilter: null, subscriptionInitialState: state);

		Task P_DoStartAsync(IRunControlAttemptState state) {
			try {
				state.EnsureNotNull(nameof(state));
				//
				void doStart() {
					var locPostingCt = ReadDA(ref _postingCts).Token;
					var locPostingWorkers = ReadDA(ref _postingWorkers);
					var locStartedPostingWorkers = new List<LocalPostingWorker>();
					try {
						for (var i = 0; i < locPostingWorkers.Length; i++) {
							var postingWorker = ReadDA(ref locPostingWorkers[ i ]);
							//
							postingWorker.RunControl.StartAsync(options: TaskCreationOptions.AttachedToParent, ctx: state.Context);
							locStartedPostingWorkers.Add(postingWorker);
						}
					}
					catch (Exception locEception) {
						var caughtExceptions = new List<Exception>();
						caughtExceptions.Add(locEception);
						foreach (var worker in locStartedPostingWorkers) {
							try {
								worker.RunControl.StopAsync(options: TaskCreationOptions.AttachedToParent);
							}
							catch (Exception locSecondException) {
								caughtExceptions.Add(locSecondException);
							}
						}
						if (caughtExceptions.Count == 1)
							throw;
						else
							throw new AggregateException(caughtExceptions);
					}
				}
				return TaskUtilities.RunOnDefaultScheduler(action: doStart);
			}
			catch (Exception exception) {
				return Task.FromException(exception);
			}
		}

		async Task P_DoStopAsync() {
			ReadDA(ref _postingCts).Cancel(throwOnFirstException: false);
			await
				TaskUtilities
				.RunOnDefaultScheduler(
					factory:
						async () => {
							var locWorkersStopTasks = new List<Task>();
							//
							foreach (var postingWorker in EnumerateDA(ReadDA(ref _postingWorkers)))
								locWorkersStopTasks.Add(item: postingWorker.RunControl.StopAsync());
							//
							if (locWorkersStopTasks.Count > 0)
								await
									Task
									.Factory
									.ContinueWhenAll(
										tasks: locWorkersStopTasks.ToArray(),
										continuationAction:
											locTasks => {
												var locTaskExceptions = locTasks.Where(locTask => locTask.IsFaulted).Select(locTask => locTask.Exception.Flatten()).ToArray();
												if (locTaskExceptions.Length > 0)
													throw new AggregateException(innerExceptions: locTaskExceptions);
											})
									.ConfigureAwait(false);
						})
				.ConfigureAwait(false);
		}

		public Task StartAsync(IContext ctx = default)
			=> StartAsync(options: TaskCreationOptions.None, ctx: ctx);

		public Task StartAsync(TaskCreationOptions options, IContext ctx = default) {
			try {
				return ReadDA(ref _runControl).StartAsync(options: options, ctx: ctx);
			}
			catch (Exception exception) {
				return Task.FromException(exception: exception);
			}
		}

		public Task StopAsync() {
			try {
				return ReadDA(ref _runControl).StopAsync();
			}
			catch (Exception firstException) {
				return TaskUtilities.FromError(error: firstException);
			}
		}

		public ILocalPublisherSettings RunningSettings
			=> ReadDA(ref _runningSettings);

		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose)
				TryReadDA(ref _runControl)?.StopAsync().WaitWithTimeout();
			//
			base.FireBeforeDispose(explicitDispose);
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_runControl?.Dispose();
				_postingCts?.Dispose();
				var postingQueueSpinLock = _postingQueueSpinLock;
				if (postingQueueSpinLock != null) {
					postingQueueSpinLock.Invoke(() => _postingQueue.Fluent().NullCond(x => x.ToList()))
						.Fluent().NullCond(postingQueueCopy => postingQueueCopy.DisposeMany());
					postingQueueSpinLock.Invoke(() => _postingQueue.Fluent().NullCond(x => x.Clear()));
				}
				var subscriptionsSpinLock = _subscriptionsSpinLock;
				if (subscriptionsSpinLock != null) {
					subscriptionsSpinLock.Invoke(() => _subscriptionsList.Fluent().NullCond(x => x.ToList()))
						.Fluent().NullCond(subscriptionsListCopy => subscriptionsListCopy.Where(i => (i.State & LocalSubscriptionStates.PublisherOwnsSubscription) == LocalSubscriptionStates.PublisherOwnsSubscription).DisposeMany());
					subscriptionsSpinLock.Invoke(() => _subscriptionsList.Fluent().NullCond(x => x.Clear()));
					subscriptionsSpinLock.Invoke(() => _subscriptionsDictionary.Fluent().NullCond(x => x.Clear()));
				}
				_postingWorkers.DisposeAndClearArray();
			}
			_postingQueue = null;
			_postingQueueSpinLock = null;
			_postingWorkers = null;
			_runningSettings = null;
			_subscriptionsDictionary = null;
			_subscriptionsList = null;
			_subscriptionsSpinLock = null;
			_runControl = null;
			_postingCts = null;
			//
			base.Dispose(explicitDispose);
		}

	}

#pragma warning restore IDE0034 // Simplify 'default' expression

}