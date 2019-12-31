using System;

using Eon.Context;

namespace Eon.Triggers {

	public interface ITriggerSignalProperties {

		ITrigger Trigger { get; }

		DateTimeOffset Timestamp { get; }

		XFullCorrelationId CorrelationId { get; }

		ITriggerSignalProperties Source { get; }

	}

}