using System;
using System.Diagnostics;
using Eon.Context;

namespace Eon.Triggers {

	[DebuggerDisplay("{ToString(),nq}")]
	public class TriggerSignalProperties
		:ITriggerSignalProperties {

		#region Nested types

		[Flags]
		enum ExplicitlyDefinedProps {

			None = 0,

			Timestamp = 1,

			CorrelationId = 2

		}

		#endregion

		readonly ITrigger _trigger;

		readonly ITriggerSignalProperties _source;

		readonly DateTimeOffset _timestamp;

		readonly XFullCorrelationId _correlationId;

		readonly ExplicitlyDefinedProps _explicitlyDefinedProps;

		public TriggerSignalProperties(ITrigger trigger, ITriggerSignalProperties source, ArgumentPlaceholder<DateTimeOffset> timestamp = default) {
			trigger.EnsureNotNull(nameof(trigger));
			source.EnsureNotNull(nameof(source));
			//
			_trigger = trigger;
			_source = source;
			_timestamp = timestamp.Substitute(value: source.Timestamp);
			_correlationId = source.CorrelationId;
			_explicitlyDefinedProps = timestamp.HasExplicitValue ? ExplicitlyDefinedProps.Timestamp : ExplicitlyDefinedProps.None;
		}

		public TriggerSignalProperties(ITrigger trigger, ArgumentPlaceholder<DateTimeOffset> timestamp = default, ArgumentPlaceholder<XFullCorrelationId> correlationId = default) {
			trigger.EnsureNotNull(nameof(trigger));
			if (correlationId.HasExplicitValue && correlationId.ExplicitValue.IsEmpty)
				throw new ArgumentException(message: "Cannot be empty.", paramName: nameof(correlationId));
			//
			_trigger = trigger;
			_source = null;
			_timestamp = timestamp.Substitute(value: DateTimeOffset.Now);
			_correlationId = correlationId.Substitute(value: default);
			_explicitlyDefinedProps =
				ExplicitlyDefinedProps.Timestamp
				| (correlationId.HasExplicitValue ? ExplicitlyDefinedProps.CorrelationId : ExplicitlyDefinedProps.None);
		}

		public ITrigger Trigger
			=> _trigger;

		public ITriggerSignalProperties Source
			=> _source;

		public DateTimeOffset Timestamp
			=> _timestamp;

		public XFullCorrelationId CorrelationId
			=> _correlationId;

		// TODO: Put strings into the resources.
		//
		public override string ToString()
			=>
			$"Trigger:{_trigger.FmtStr().GNLI()}"
			+ ((_explicitlyDefinedProps & ExplicitlyDefinedProps.Timestamp) != ExplicitlyDefinedProps.Timestamp ? string.Empty : $"{Environment.NewLine}Timestamp: {_timestamp.ToString("o")}")
			+ ((_explicitlyDefinedProps & ExplicitlyDefinedProps.CorrelationId) != ExplicitlyDefinedProps.CorrelationId ? string.Empty : $"{Environment.NewLine}Correlation: {_correlationId.FmtStr().GNLI()}")
			+ (_source is null ? string.Empty : $"{Environment.NewLine}Source:{_source.FmtStr().GNLI()}");

	}

}