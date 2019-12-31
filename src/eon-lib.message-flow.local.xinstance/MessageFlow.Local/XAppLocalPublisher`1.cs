using System;
using System.Threading.Tasks;

using Eon.Context;
using Eon.MessageFlow.Local.Description;
using Eon.Threading.Tasks;

namespace Eon.MessageFlow.Local {

	public class XAppLocalPublisher<TDescription>
		:XAppScopeInstanceBase<TDescription>, IXAppLocalPublisher<TDescription>
		where TDescription : class, IXAppLocalPublisherDescription {

		ILocalPublisher _innerPublisher;

		public XAppLocalPublisher(IXAppScopeInstance scope, TDescription description)
			: base(scope: scope, description: description) { }

		protected override Task OnInitializeAsync(IContext ctx = default) {
			try {
				var publisher = default(ILocalPublisher);
				try {
					publisher = new LocalPublisher(Description.UseDefaultSettings ? LocalPublisherSettings.Default : Description.CustomSettings);
					WriteDA(location: ref _innerPublisher, value: publisher);
				}
				catch (Exception exception) {
					publisher?.Dispose(exception);
					throw;
				}
				return Task.CompletedTask;
			}
			catch (Exception exception) {
				return Task.FromException(exception: exception);
			}
		}

		protected ILocalPublisher InnerPublisher
			=> ReadIA(location: ref _innerPublisher, locationName: nameof(_innerPublisher));

		public ILocalPublisherSettings RunningSettings
			=> InnerPublisher.RunningSettings;

		public virtual Task<ILocalMessagePostingToken> PublishAsync<TPayload>(TPayload payload)
			=> InnerPublisher.PublishAsync(payload: payload);

		public virtual Task<ILocalMessagePostingToken> PublishAsync<TPayload>(TPayload payload, bool disposePayloadAtEndOfPosting)
			=> InnerPublisher.PublishAsync(payload: payload, disposePayloadAtEndOfPosting: disposePayloadAtEndOfPosting);

		public virtual Task<ILocalMessagePostingToken> PublishMessageAsync(ILocalMessage message, bool disposeMessageAtEndOfPosting)
			=> InnerPublisher.PublishMessageAsync(message: message, disposeMessageAtEndOfPosting: disposeMessageAtEndOfPosting);

		public virtual Task StartAsync(IContext ctx = default)
			=> InnerPublisher.StartAsync(ctx: ctx);

		public virtual Task StartAsync(TaskCreationOptions options, IContext ctx = default)
			=> InnerPublisher.StartAsync(options: options, ctx: ctx);

		public virtual Task StopAsync()
			=> InnerPublisher.StopAsync();

		public virtual Task<ILocalSubscription> SubscribeAsync(ILocalSubscription subscription)
			=> InnerPublisher.SubscribeAsync(subscription: subscription);

		public virtual Task<ILocalSubscription> SubscribeAsync<TPayload>(ProcessLocalMessage<TPayload> process)
			=> InnerPublisher.SubscribeAsync(process: process);

		public virtual Task<ILocalSubscription> SubscribeAsync<TPayload>(ProcessLocalMessage<TPayload> process, LocalPublicationFilter<TPayload> publicationFilter = null)
			=> InnerPublisher.SubscribeAsync(publicationFilter: publicationFilter, process: process);

		public virtual Task<ILocalSubscription> SubscribeAsync<TPayload>(ProcessLocalMessage<TPayload> process, LocalPublicationFilter<TPayload> publicationFilter = default, LocalSubscriptionStates subscriptionInitialState = default)
			=> InnerPublisher.SubscribeAsync(publicationFilter: publicationFilter, process: process, subscriptionInitialState: subscriptionInitialState);

		public Task<ILocalSubscription> SubscribeAsync<TPayload>(ProcessLocalMessage<TPayload> process, LocalSubscriptionStates subscriptionInitialState = default)
			=> InnerPublisher.SubscribeAsync(process: process, state: subscriptionInitialState);

		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose)
				TryReadDA(ref _innerPublisher)?.StopAsync().WaitWithTimeout();
			//
			base.FireBeforeDispose(explicitDispose);
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose)
				_innerPublisher?.Dispose();
			_innerPublisher = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}