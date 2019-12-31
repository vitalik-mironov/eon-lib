namespace Eon.MessageFlow.Local {

	/// <summary>
	/// Аргументы события предварительной обработки издания сообщения <see cref="ILocalMessage"/>.
	/// </summary>
	public sealed class LocalPublicationFilterEventArgs
		:OnceCanceledEventArgs {

		readonly ILocalSubscription _subscription;

		readonly ILocalMessage _message;

		public LocalPublicationFilterEventArgs(ILocalSubscription subscription, ILocalMessage message) {
			subscription.EnsureNotNull(nameof(subscription));
			message.EnsureNotNull(nameof(message));
			//
			_subscription = subscription;
			_message = message;
		}

		public ILocalSubscription Subscription => _subscription;

		public ILocalMessage Message => _message;

	}

}