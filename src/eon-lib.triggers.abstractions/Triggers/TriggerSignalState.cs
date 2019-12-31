using System;

namespace Eon.Triggers {

	public class TriggerSignalState {

		public readonly long SignalCounter;

		public readonly DateTimeOffset LastSignalTimestamp;

		public TriggerSignalState(long signalCounter, DateTimeOffset lastSignalTimestamp) {
			signalCounter.Arg(nameof(signalCounter)).EnsureNotLessThanZero();
			if (signalCounter == 0) {
				if (lastSignalTimestamp != default)
					throw new ArgumentOutOfRangeException(paramName: nameof(lastSignalTimestamp));
			}
			else if (lastSignalTimestamp == default)
				throw new ArgumentOutOfRangeException(paramName: nameof(lastSignalTimestamp));
			//
			SignalCounter = signalCounter;
			LastSignalTimestamp = lastSignalTimestamp;
		}

	}

}