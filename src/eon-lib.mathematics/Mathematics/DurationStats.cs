using System;
using System.Collections.Generic;

namespace Eon.Mathematics {

	public sealed class DurationStats {

		#region Static members

		public static DurationStats Calculate(IEnumerable<TimeSpan> values, DurationStats entire = default)
			=> Calculate(values: values.Arg(nameof(values)), entire: entire);

		public static DurationStats Calculate(ArgumentUtilitiesHandle<IEnumerable<TimeSpan>> values, DurationStats entire = default)
			=> new DurationStats(stats: Stats.Calculate(values: values.Select(selector: locValue => locValue.TotalMilliseconds), entire: entire?._stats), entire: entire);

		#endregion

		readonly Stats _stats;

		DurationStats(Stats stats, DurationStats entire) {
			stats.EnsureNotNull(nameof(stats));
			//
			_stats = stats;
		}

		public Stats Stats
			=> _stats;

		public override string ToString()
			=> ToString(options: StatsFormattingOptions.Default);

		public string ToString(StatsFormattingOptions options)
			=> _stats.ToString(options: options, formatter: locValue => TimeSpan.FromMilliseconds(value: locValue).ToString("c"));

	}

}