namespace Eon.MessageFlow.Local {

	public interface ILocalPublicationFilterState {

		ILocalSubscription Subscription { get; }

		ILocalMessage Message { get; }

	}

}