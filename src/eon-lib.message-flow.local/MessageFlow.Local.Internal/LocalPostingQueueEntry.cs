using System.Collections.Generic;
using System.Linq;

using Eon.Collections;
using Eon.Threading;

namespace Eon.MessageFlow.Local.Internal {

	internal sealed class LocalPostingQueueEntry
		:Disposable {

		LocalPostingToken _postingToken;

		List<ILocalSubscription> _postingSubscriptions;

		PrimitiveSpinLock _postingSubscriptionsSpinLock;

		internal LocalPostingQueueEntry(LocalPostingToken postingToken, IEnumerable<ILocalSubscription> postingSubscriptions) {
			postingToken.EnsureNotNull(nameof(postingToken));
			var locPostingSubscriptions = postingSubscriptions.EnsureNotNull(nameof(postingSubscriptions)).EnsureNoNullElements().Value.ToList();
			//
			_postingToken = postingToken;
			_postingSubscriptions = locPostingSubscriptions;
			_postingSubscriptionsSpinLock = new PrimitiveSpinLock();
		}

		public bool TryGetNextPostingSubscription(out LocalPostingToken postingToken, out ILocalSubscription postingSubscription) {
			if (TryReadDA(ref _postingSubscriptionsSpinLock, considerDisposeRequest: true, result: out var subscriptionsSpinLock)
				&& TryReadDA(ref _postingSubscriptions, considerDisposeRequest: true, result: out var subscriptions)
				&& TryReadDA(ref _postingToken, considerDisposeRequest: true, result: out var locPostingToken)) {
				//
				var locPostingSubscription =
					subscriptionsSpinLock
						.Invoke(
							() => {
								if (subscriptions.Count < 1)
									return null;
								else
									return subscriptions.RemoveItemAt(index: 0);
							});
				postingSubscription = locPostingSubscription;
				postingToken = locPostingSubscription == null ? null : locPostingToken;
				return locPostingSubscription != null;
			}
			else {
				postingToken = null;
				postingSubscription = null;
				return false;
			}
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose)
				_postingSubscriptionsSpinLock?.Invoke(() => _postingSubscriptions?.Clear());
			_postingToken = null;
			_postingSubscriptions = null;
			_postingSubscriptionsSpinLock = null;
			//
			base.Dispose(explicitDispose);
		}

	}


}