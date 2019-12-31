using System;

using Eon.Context;

namespace Eon.ComponentModel {

	public abstract class RunControlAttemptPropertiesBase
		:IRunControlAttemptProperties {

		// TODO: Put strings into the resources.
		//
		protected RunControlAttemptPropertiesBase(IRunControl runControl, bool isStart, int attemptNumber, int succeededAttemptCountBefore, XFullCorrelationId correlationId = default, object tag = default) {
			runControl.EnsureNotNull(nameof(runControl));
			attemptNumber.Arg(nameof(attemptNumber)).EnsureNotLessThanZero();
			succeededAttemptCountBefore.Arg(nameof(succeededAttemptCountBefore)).EnsureNotLessThanZero().EnsureNotGreaterThan(attemptNumber);
			if (correlationId?.IsEmpty == true)
				throw new ArgumentException(message: "Value cann't be an empty.", paramName: nameof(correlationId));
			//
			CorrelationId = correlationId;
			RunControl = runControl;
			IsStart = isStart;
			AttemptNumber = attemptNumber;
			SucceededAttemptCountBefore = succeededAttemptCountBefore;
			Tag = tag;
		}

		protected RunControlAttemptPropertiesBase(IRunControlAttemptProperties props)
			: this(runControl: props.EnsureNotNull(nameof(props)).Value.RunControl, isStart: props.IsStart, attemptNumber: props.AttemptNumber, succeededAttemptCountBefore: props.SucceededAttemptCountBefore, correlationId: props.CorrelationId, tag: props.Tag) { }

		public XFullCorrelationId CorrelationId { get; }

		public IRunControl RunControl { get; }

		public bool IsStart { get; }

		public bool IsStop => !IsStart;

		public int AttemptNumber { get; }

		public int SucceededAttemptCountBefore { get; }

		public object Tag { get; }

	}

}