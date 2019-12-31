using System;

using Eon.Triggers.Description;

namespace Eon.Triggers {

	public class FrequencyLimitTrigger<TDescription>
		:AggregateTrigger<TDescription>
		where TDescription : class, IFrequencyLimitTriggerDescription {

		readonly TimeSpan _signalFrequencyLimit;

		public FrequencyLimitTrigger(IXAppScopeInstance scope, TDescription description)
			: base(scope: scope, description: description) {
			_signalFrequencyLimit = description.SignalFrequencyLimit;
		}

		protected override void SignalCondition(ITriggerSignalProperties signalProps, TriggerSignalState inState, out TriggerSignalState outState) {
			signalProps.EnsureNotNull(nameof(signalProps));
			//
			if ((inState is null) || inState.SignalCounter == 0 || signalProps.Timestamp.Subtract(inState.LastSignalTimestamp) > _signalFrequencyLimit)
				base.SignalCondition(signalProps: signalProps, inState: inState, outState: out outState);
			else
				outState = inState;
		}

	}

}