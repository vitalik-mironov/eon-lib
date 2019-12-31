using System.Threading.Tasks;
using Eon.Context;

namespace Eon.MessageFlow.Local {

	/// <summary>
	/// Представляет издателя сообщений <see cref="ILocalMessage"/>.
	/// </summary>
	public interface ILocalPublisher
		:IDisposeNotifying {

		Task<ILocalSubscription> SubscribeAsync<TPayload>(ProcessLocalMessage<TPayload> process, LocalPublicationFilter<TPayload> publicationFilter, LocalSubscriptionStates subscriptionInitialState);

		Task<ILocalSubscription> SubscribeAsync<TPayload>(ProcessLocalMessage<TPayload> process, LocalPublicationFilter<TPayload> publicationFilter);

		Task<ILocalSubscription> SubscribeAsync<TPayload>(ProcessLocalMessage<TPayload> process, LocalSubscriptionStates state);

		Task<ILocalSubscription> SubscribeAsync<TPayload>(ProcessLocalMessage<TPayload> process);

		Task<ILocalSubscription> SubscribeAsync(ILocalSubscription subscription);

		Task<ILocalMessagePostingToken> PublishMessageAsync(ILocalMessage message, bool disposeMessageAtEndOfPosting);

		Task<ILocalMessagePostingToken> PublishAsync<TPayload>(TPayload payload, bool disposePayloadAtEndOfPosting);

		Task<ILocalMessagePostingToken> PublishAsync<TPayload>(TPayload payload);

		Task StartAsync(IContext ctx = default);

		Task StartAsync(TaskCreationOptions options, IContext ctx = default);

		Task StopAsync();

		ILocalPublisherSettings RunningSettings { get; }

	}

}