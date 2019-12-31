using System.Diagnostics;
using System.Threading.Tasks;

using Eon.Context;

namespace Eon.MessageFlow.Local {

	[DebuggerDisplay("{ToString(),nq}")]
	public class DelegateLocalSubscriber<TPayload>
		:LocalSubscriberBase<TPayload> {

		ProcessLocalMessage<TPayload> _processMessageUserDelegate;

		LocalPublicationFilter<TPayload> _publicationFilter;

		LocalSubscriptionActivationHandler _onSubscriptionActivationUserDelegate;

		LocalSubscriptionDeactivationHandler _onSubscriptionDeactivationUserDelegate;

		public DelegateLocalSubscriber(ProcessLocalMessage<TPayload> processMessage, string aboutInfo = default)
			: this(publicationFilter: null, process: processMessage, aboutInfo: aboutInfo) { }

		public DelegateLocalSubscriber(
			ProcessLocalMessage<TPayload> process,
			LocalPublicationFilter<TPayload> publicationFilter = default,
			LocalSubscriptionActivationHandler onSubscriptionActivation = default,
			LocalSubscriptionDeactivationHandler onSubscriptionDeactivation = default,
			string aboutInfo = default)
			: base(aboutInfo: aboutInfo) {
			//
			process.EnsureNotNull(nameof(process));
			//
			_processMessageUserDelegate = process;
			_publicationFilter = publicationFilter;
			_onSubscriptionActivationUserDelegate = onSubscriptionActivation;
			_onSubscriptionDeactivationUserDelegate = onSubscriptionDeactivation;
		}

		protected override async Task OnSubscriptionActivationAsync(ILocalSubscription subscription, IContext ctx = default) {
			var userDelegate = ReadDA(ref _onSubscriptionActivationUserDelegate);
			if (!(userDelegate is null))
				await userDelegate(subscription: subscription, ctx: ctx).ConfigureAwait(false);
		}

		protected override async Task OnSubscriptionDeactivationAsync(ILocalSubscription subscription, IContext ctx = default) {
			var userDelegate = ReadDA(ref _onSubscriptionDeactivationUserDelegate);
			if (!(userDelegate is null))
				await userDelegate(subscription: subscription, ctx: ctx).ConfigureAwait(false);
		}

		protected sealed override async Task ProcessMessageAsync(ILocalSubscription subscription, ILocalMessage<TPayload> message, IContext ctx = default) {
			var userDelegate = ReadDA(ref _processMessageUserDelegate);
			await userDelegate(subscription: subscription, message: message, ctx: ctx).ConfigureAwait(false);
		}

		protected sealed override void PublicationFilter(LocalPublicationFilterEventArgs<TPayload> e) {
			base.PublicationFilter(e);
			//
			if (!e.OnceCanceled)
				TryReadDA(ref _publicationFilter)?.Invoke(e);
		}

		protected override void Dispose(bool explicitDispose) {
			_publicationFilter = null;
			_processMessageUserDelegate = null;
			_onSubscriptionActivationUserDelegate = null;
			_onSubscriptionDeactivationUserDelegate = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}