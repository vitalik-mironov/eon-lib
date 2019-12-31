using System;

using Eon.Threading;

namespace Eon.MessageFlow.Local.Internal {

	internal sealed class LocalPostingToken
		:ILocalMessagePostingToken {

		readonly bool _disposeMessageAtEndOfPosting;

		readonly int _postingCount;

		readonly ILocalMessage _message;

		int _completedPostingCountdown;

		internal LocalPostingToken(ILocalMessage message, bool disposeMessageAtEndOfPosting) {
			message.EnsureNotNull(nameof(message));
			//
			_disposeMessageAtEndOfPosting = disposeMessageAtEndOfPosting;
			_postingCount = 0;
			_message = message;
			_completedPostingCountdown = 0;
		}

		internal LocalPostingToken(ILocalMessage message, int postingCount, bool disposeMessageAtEndOfPosting) {
			message.EnsureNotNull(nameof(message));
			postingCount
				.Arg(nameof(postingCount))
				.EnsureNotLessThan(operand:1);
			//
			_disposeMessageAtEndOfPosting = disposeMessageAtEndOfPosting;
			_postingCount = postingCount;
			_message = message;
			_completedPostingCountdown = postingCount;
		}

		public ILocalMessage Message
			=> _message;

		public int PostingCount
			=> _postingCount;

		public bool DisposeMessageAtEnd
			=> _disposeMessageAtEndOfPosting;

		public void NotifySubscriptionPosted() {
			int decrementedCounter;
			if (_postingCount == 0 || !InterlockedUtilities.Decrement(location: ref _completedPostingCountdown, minExclusive: 0, result: out decrementedCounter))
				throw new InvalidOperationException();
			else if (decrementedCounter == 0 && _disposeMessageAtEndOfPosting)
				_message?.Dispose();
		}

	}

}