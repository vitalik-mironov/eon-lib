namespace Eon.MessageFlow.Local {

	/// <summary>
	/// Аргументы события предварительной обработки публикации сообщения <see cref="ILocalMessage{TData}"/>.
	/// </summary>
	public sealed class LocalPublicationFilterEventArgs<TPayload>
		:OnceCanceledEventArgs {

		readonly ILocalSubscription _subscription;

		readonly ILocalMessage<TPayload> _message;

		public LocalPublicationFilterEventArgs(ILocalSubscription subscription, ILocalMessage<TPayload> message) {
			subscription.EnsureNotNull(nameof(subscription));
			message.EnsureNotNull(nameof(message));
			//
			_subscription = subscription;
			_message = message;
		}

		public ILocalSubscription Subscription 
			=> _subscription;

		public ILocalMessage<TPayload> Message 
			=> _message;

	}

}