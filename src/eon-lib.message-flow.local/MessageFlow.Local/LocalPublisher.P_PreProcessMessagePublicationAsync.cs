using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Eon.ComponentModel;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.MessageFlow.Local {

	public partial class LocalPublisher {

		#region Static members

		// TODO: Put strings into the resources.
		//
		static async Task<IList<ILocalSubscription>> P_PublicationFilterAsync(IRunControl publisherRunControl, ILocalMessage msg, IList<ILocalSubscription> sourceSubscriptions) {
			publisherRunControl.EnsureNotNull(nameof(publisherRunControl));
			msg.EnsureNotNull(nameof(msg));
			sourceSubscriptions.EnsureNotNull(nameof(sourceSubscriptions));
			//
			if (sourceSubscriptions.Count < 1 || publisherRunControl.HasStopRequested)
				return new ILocalSubscription[ 0 ];
			else {
				var sourceSubscriptionsList = new List<ILocalSubscription>();
				var length = sourceSubscriptions.Count;
				for (var offset = 0; offset < length; offset++) {
					var subscription = sourceSubscriptions[ offset ];
					if (subscription is null)
						throw
							new ArgumentException(
								paramName: $"{nameof(sourceSubscriptions)}[{offset:d}]",
								message: FormatXResource(typeof(Array), "CanNotContainNull/NullAt", offset.ToString("d")));
					sourceSubscriptionsList.Add(subscription);
				}
				//
				var filterResultList = new List<ILocalSubscription>();
				for (var i = 0; i < sourceSubscriptionsList.Count; i++) {
					if (publisherRunControl.HasStopRequested)
						break;
					//
					var subscription = sourceSubscriptionsList[ i ];
					var subscriptionFilterResult = await subscription.PublicationFilterAsync(state: new LocalPublicationFilterState(message: msg, subscription: subscription)).ConfigureAwait(false);
					if (!subscriptionFilterResult.CancelPublication)
						filterResultList.Add(subscription);
				}
				//
				if (publisherRunControl.HasStopRequested)
					return new ILocalSubscription[ 0 ];
				else
					return filterResultList;
			}
		}

		#endregion

	}

}