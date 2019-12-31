using System;

namespace Eon.Triggers {

	public class TriggerSignalEventArgs
		:EventArgs {

		ITriggerSignalProperties _signalProperties;

		public TriggerSignalEventArgs(ITriggerSignalProperties signalProps) {
			signalProps.EnsureNotNull(nameof(signalProps));
			//
			_signalProperties = signalProps;
		}

		public ITriggerSignalProperties SignalProperties
			=> _signalProperties;

	}

}