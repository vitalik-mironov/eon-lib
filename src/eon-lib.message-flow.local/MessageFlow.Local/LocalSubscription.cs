using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Eon.ComponentModel;
using Eon.Context;
using Eon.Threading;
using Eon.Threading.Tasks;

namespace Eon.MessageFlow.Local {

	public class LocalSubscription
		:Disposable, ILocalSubscription {

		#region Static & constant members

		static readonly IEqualityComparer<ILocalPublisher> __PublisherEqComparer = EqualityComparer<ILocalPublisher>.Default;

		static readonly IEqualityComparer<ILocalSubscriber> __SubscriberEqComparer = EqualityComparer<ILocalSubscriber>.Default;

		#endregion

		ILocalPublisher _publisher;

		readonly string _publisherInfoText;

		ILocalPublicationFilterListener _publisherAsIPublicationFilterListener;

		ILocalSubscriber _subscriber;

		readonly string _subscriberInfoText;

		ILocalPublicationFilterListener _subscriberAsIPublicationFilterListener;

		int _state;

		EventHandler<LocalPublicationFilterEventArgs> _eventHandler_PublicationFilter;

		RunControl<LocalSubscription> _activateControl;

		readonly int _hashCode;

		public LocalSubscription(ILocalPublisher publisher, ILocalSubscriber subscriber)
			: this(publisher: publisher, subscriber: subscriber, initialState: LocalSubscriptionStates.None) { }

		public LocalSubscription(ILocalPublisher publisher, ILocalSubscriber subscriber, LocalSubscriptionStates initialState) {
			publisher.EnsureNotNull(nameof(publisher));
			subscriber.EnsureNotNull(nameof(subscriber));
			P_EnsureInitialStateValid(initialState);
			//
			_state = (int)initialState;
			unchecked {
				_hashCode = (publisher.GetHashCode() * 17) ^ (subscriber.GetHashCode() * 23);
			}
			_publisher = publisher;
			_publisherInfoText = publisher.ToString();
			_publisherAsIPublicationFilterListener = publisher as ILocalPublicationFilterListener;
			_subscriber = subscriber;
			_subscriberInfoText = subscriber.ToString();
			_subscriberAsIPublicationFilterListener = subscriber as ILocalPublicationFilterListener;
			_activateControl = new RunControl<LocalSubscription>(options: RunControlOptions.SingleStart, component: this, attemptState: null, beforeStart: null, start: P_DoActivationAsync, stop: P_DoDeactivationAsync);
			//
			publisher.BeforeDispose += P_EH_PublisherOrSubscriber_BeforeDispose;
			subscriber.BeforeDispose += P_EH_PublisherOrSubscriber_BeforeDispose;
		}

		public LocalSubscriptionStates State
			=> (LocalSubscriptionStates)VolatileUtilities.Read(ref _state);

		public bool IsActive
			=> (State & (LocalSubscriptionStates.Activated | LocalSubscriptionStates.Deactivated | LocalSubscriptionStates.Suspended)) == LocalSubscriptionStates.Activated && !IsDisposeRequested;

		public event EventHandler<LocalPublicationFilterEventArgs> PublicationFilter {
			add { AddEventHandler(ref _eventHandler_PublicationFilter, value); }
			remove { RemoveEventHandler(ref _eventHandler_PublicationFilter, value); }
		}

		// TODO: Put strings into the resources.
		//
		void P_EnsureInitialStateValid(LocalSubscriptionStates initialState) {
			var invalidFlags = ~(LocalSubscriptionStates.PublisherOwnsSubscription | LocalSubscriptionStates.Suspended | LocalSubscriptionStates.OwnsSubscriber) & initialState;
			if (invalidFlags != LocalSubscriptionStates.None)
				throw new ArgumentOutOfRangeException(paramName: nameof(initialState), message: $"Указано недопустимое исходное состояние подписки '{invalidFlags.ToString()}'.");
		}

		void P_EH_PublisherOrSubscriber_BeforeDispose(object sender, DisposeEventArgs e) {
			if (TryReadDA(ref _publisher, out var publisher)
				&& TryReadDA(ref _subscriber, out var subscriber)
				&& (ReferenceEquals(sender, publisher) || ReferenceEquals(sender, subscriber)))
				//
				DeactivateAsync().WaitWithTimeout();
		}

		LocalSubscriptionStates P_ChangeStateOr(LocalSubscriptionStates state)
			=> (LocalSubscriptionStates)InterlockedUtilities.Or(ref _state, (int)state);

		LocalSubscriptionStates P_ChangeStateXor(LocalSubscriptionStates state)
			=> (LocalSubscriptionStates)InterlockedUtilities.Xor(ref _state, (int)state);

		public Task ActivateAsync(TaskCreationOptions taskCreationOptions = default)
			=> ReadDA(ref _activateControl).StartAsync(taskCreationOptions);

		async Task P_DoActivationAsync(IRunControlAttemptState state) {
			state.EnsureNotNull(nameof(state));
			//
			await ReadDA(ref _subscriber).OnSubscriptionActivationAsync(subscription: this, ctx: state.Context).ConfigureAwait(false);
			P_ChangeStateOr(LocalSubscriptionStates.Activated);
		}

		public Task DeactivateAsync() {
			if ((State & LocalSubscriptionStates.Deactivated) != LocalSubscriptionStates.Deactivated) {
				var activateControl = TryReadDA(ref _activateControl);
				if (activateControl is null) {
					P_ChangeStateOr(LocalSubscriptionStates.Deactivated);
					return Task.CompletedTask;
				}
				else
					return activateControl.StopAsync();
			}
			else
				return Task.CompletedTask;
		}

		async Task P_DoDeactivationAsync(IRunControlAttemptState state) {
			state.EnsureNotNull(nameof(state));
			//
			if ((P_ChangeStateOr(LocalSubscriptionStates.Deactivated) & (LocalSubscriptionStates.Deactivated | LocalSubscriptionStates.Activated)) == LocalSubscriptionStates.Activated
				&& TryReadDA(ref _subscriber, out var locSubscriber)
				&& !(locSubscriber is null))
				await locSubscriber.OnSubscriptionDeactivationAsync(subscription: this, ctx: state.Context).ConfigureAwait(false);
		}

		public Task SuspendAsync() {
			P_ChangeStateOr(LocalSubscriptionStates.Suspended);
			return Task.CompletedTask;
		}

		public Task ResumeAsync() {
			try {
				if ((P_ChangeStateXor(LocalSubscriptionStates.Suspended) & LocalSubscriptionStates.Deactivated) == LocalSubscriptionStates.Deactivated)
					throw
						new EonException(message: $"Подписка была выведена из состояния приостановки действия, но остается не действующей, так ранее была деактивирована.{Environment.NewLine}\tПодписка:{this.FmtStr().GNLI2()}");
				return Task.CompletedTask;
			}
			catch (Exception exception) {
				return Task.FromException(exception);
			}
		}

		public Task<bool> TryResumeAsync()
			=> Task.FromResult((P_ChangeStateXor(LocalSubscriptionStates.Suspended) & LocalSubscriptionStates.Deactivated) != LocalSubscriptionStates.Deactivated);

		public ILocalPublisher Publisher
			=> ReadDA(ref _publisher);

		public ILocalSubscriber Subscriber
			=> ReadDA(ref _subscriber);

		public virtual bool Equals(ILocalSubscription other) {
			if (ReferenceEquals(this, other))
				return true;
			else if (other is null)
				return false;
			else
				return __PublisherEqComparer.Equals(x: Publisher, y: other.Publisher) && __SubscriberEqComparer.Equals(x: Subscriber, y: other.Subscriber);
		}

		public sealed override bool Equals(object obj)
			=> Equals(obj as ILocalSubscription);

		public override int GetHashCode()
			=> _hashCode;

		public async Task<LocalPublicationFilterResult> PublicationFilterAsync(ILocalPublicationFilterState state) {
			state.EnsureNotNull(nameof(state));
			//
			await Task.CompletedTask;
			//
			ILocalMessage msg;
			if ((msg = state.Message).IsDisposeRequested || !IsActive)
				return new LocalPublicationFilterResult(state: state, cancelPublication: true);
			else {
				bool eval() {
					bool locCancelPublication;
					//
					if (IsActive) {
						var locEventArgs = new LocalPublicationFilterEventArgs(subscription: this, message: msg);
						if (TryReadDA(ref _publisher, considerDisposeRequest: true, out var locIssuer)
							&& TryReadDA(ref _subscriber, considerDisposeRequest: true, out var locSubscriber)
							&& TryReadDA(ref _publisherAsIPublicationFilterListener, considerDisposeRequest: true, out var locFilterListener)
							&& TryReadDA(ref _subscriberAsIPublicationFilterListener, considerDisposeRequest: true, out var locSubscriberListener)) {
							//
							try {
								FirePublicationFilter(locEventArgs);
								locCancelPublication = locEventArgs.OnceCanceled || !IsActive;
							}
							catch (ObjectDisposedException) {
								if (IsDisposeRequested)
									locCancelPublication = true;
								else
									throw;
							}
							if (!locCancelPublication) {
								bool locIsCanceledByListener(ILocalPublicationFilterListener locListener) {
									if (locListener is null)
										return false;
									else {
										try {
											locListener.PublicationFilter(sender: this, e: locEventArgs);
											return locEventArgs.OnceCanceled;
										}
										catch (ObjectDisposedException) {
											var locListenerAsIDisposable = locListener as IOxyDisposable;
											if (locListenerAsIDisposable?.IsDisposeRequested ?? false)
												return true;
											else
												throw;
										}
									}
								}
								locCancelPublication = locIsCanceledByListener(locFilterListener) || locIsCanceledByListener(locSubscriberListener) || !IsActive;
							}
						}
						else
							locCancelPublication = true;
					}
					else
						locCancelPublication = true;
					//
					return locCancelPublication;
				}
				//
				return new LocalPublicationFilterResult(state: state, cancelPublication: eval());
			}
		}

		protected virtual void FirePublicationFilter(LocalPublicationFilterEventArgs e) {
			e.EnsureNotNull(nameof(e));
			//
			ReadDA(ref _eventHandler_PublicationFilter)?.Invoke(this, e);
		}

		// TODO: Put strings into the resources.
		//
		public override string ToString()
			=>
			$"Type:{GetType().FmtStr().GNLI()}"
			+ $"{Environment.NewLine}Publisher:{_publisherInfoText.FmtStr().GNLI()}"
			+ $"{Environment.NewLine}Subscriber:{_subscriberInfoText.FmtStr().GNLI()}"
			+ $"{Environment.NewLine}State (flags):{State.FmtStr().GNLI()}";

		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose) {
				DeactivateAsync().WaitWithTimeout();
			}
			//
			base.FireBeforeDispose(explicitDispose);
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_activateControl?.Dispose();
				_publisher.Fluent().NullCond(locIssuer => locIssuer.BeforeDispose -= P_EH_PublisherOrSubscriber_BeforeDispose);
				_subscriber.Fluent().NullCond(locSubscriber => locSubscriber.BeforeDispose -= P_EH_PublisherOrSubscriber_BeforeDispose);
				if ((State & LocalSubscriptionStates.OwnsSubscriber) == LocalSubscriptionStates.OwnsSubscriber)
					_subscriber?.Dispose();
			}
			_activateControl = null;
			_eventHandler_PublicationFilter = null;
			_publisher = null;
			_publisherAsIPublicationFilterListener = null;
			_subscriber = null;
			_subscriberAsIPublicationFilterListener = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}