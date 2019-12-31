namespace Eon.MessageFlow.Local {

	public sealed class LocalPublicationFilterState
		:ILocalPublicationFilterState {

		readonly ILocalSubscription _subscription;

		readonly ILocalMessage _message;

		public LocalPublicationFilterState(ILocalMessage message, ILocalSubscription subscription) {
			message.EnsureNotNull(nameof(message));
			subscription.EnsureNotNull(nameof(subscription));
			//
			_message = message;
			_subscription = subscription;
		}

		public ILocalMessage Message
			=> _message;

		public ILocalSubscription Subscription 
			=> _subscription;

	}

}