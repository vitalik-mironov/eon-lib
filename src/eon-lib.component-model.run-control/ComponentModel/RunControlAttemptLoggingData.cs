using System;

using Eon.Context;
using Eon.Diagnostics;

namespace Eon.ComponentModel {

	public sealed class RunControlAttemptLoggingData
		:RunControlAttemptPropertiesBase, IRunControlAttemptLoggingData {

		public RunControlAttemptLoggingData(IRunControl runControl, bool isStart, int attemptNumber, int succeededAttemptCountBefore, OperationCompletionStatusCode status, TimeSpan duration, Exception exception = default, XFullCorrelationId correlationId = default)
			: base(runControl: runControl, isStart: isStart, attemptNumber: attemptNumber, succeededAttemptCountBefore: succeededAttemptCountBefore, correlationId: correlationId) {
			//
			if (status == OperationCompletionStatusCode.Undefined)
				throw new ArgumentOutOfRangeException(paramName: nameof(status));
			else if (duration < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(paramName: nameof(duration));
			else if (status == OperationCompletionStatusCode.Fault)
				exception.EnsureNotNull(nameof(exception));
			else
				exception.Arg(nameof(exception)).EnsureIsNull();
			//
			Status = status;
			Duration = duration;
			Exception = exception;
		}

		public OperationCompletionStatusCode Status { get; }

		public TimeSpan Duration { get; }

		public Exception Exception { get; }

	}

}