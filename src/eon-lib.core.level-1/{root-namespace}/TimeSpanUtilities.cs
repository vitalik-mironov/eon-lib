using System;

using Eon.Threading;

namespace Eon {

	public static class TimeSpanUtilities {

		static readonly TimeSpan __MaxTimeOfDayExclusive = TimeSpan.FromDays(1.0D);

		/// <summary>
		/// Значение: '-14:00:00'.
		/// </summary>
		public static readonly TimeSpan MinTimeZoneOffsetInclusive = (new TimeSpan(14, 0, 0)).Negate();

		/// <summary>
		/// Значение: '+14:00:00'.
		/// </summary>
		public static readonly TimeSpan MaxTimeZoneOffsetInclusive = new TimeSpan(14, 0, 0);

		public static TimeSpan Max(this TimeSpan tsA, TimeSpan tsB)
			=> tsB > tsA ? tsB : tsA;

		public static TimeSpan Min(this TimeSpan tsA, TimeSpan tsB)
			=> tsB < tsA ? tsB : tsA;

		public static bool IsTimeOfDay(this TimeSpan ts)
			=> ts >= TimeSpan.Zero && ts < __MaxTimeOfDayExclusive;

		public static bool HasMaxScaleOfSeconds(this TimeSpan ts)
			=> ts == new TimeSpan(days: ts.Days, hours: ts.Hours, minutes: ts.Minutes, seconds: ts.Seconds);

		public static bool HasMaxScaleOfMilliSeconds(this TimeSpan ts)
			=> ts == new TimeSpan(days: ts.Days, hours: ts.Hours, minutes: ts.Minutes, seconds: ts.Seconds, milliseconds: ts.Milliseconds);

		public static bool IsValidTimeZoneOffset(this TimeSpan timeZoneOffset)
			=> timeZoneOffset >= MinTimeZoneOffsetInclusive && timeZoneOffset <= MaxTimeZoneOffsetInclusive;

		public static bool? IsValidTimeZoneOffset(this TimeSpan? timeZoneOffset) {
			if (timeZoneOffset.HasValue)
				return IsValidTimeZoneOffset(timeZoneOffset.Value);
			else
				return null;
		}

		public static bool IsInfiniteTimeout(this TimeSpan ts)
			=> ts == TimeoutDuration.InfiniteTimeSpan;

		public static bool? IsInfiniteTimeout(this TimeSpan? ts)
			=> ts.HasValue ? ts.Value == TimeoutDuration.InfiniteTimeSpan : (bool?)null;

		public static TimeSpan RoundToSeconds(this TimeSpan ts)
			=>
			new TimeSpan(
				days: ts.Days,
				hours: ts.Hours,
				minutes: ts.Minutes,
				seconds: ts.Seconds + (Math.Abs(ts.Milliseconds) > 499 ? Math.Sign(ts.Seconds) * 1 : 0),
				milliseconds: 0);

		public static TimeSpan RoundToMilliseconds(this TimeSpan ts)
			=>
			new TimeSpan(
				days: ts.Days,
				hours: ts.Hours,
				minutes: ts.Minutes,
				seconds: ts.Seconds,
				milliseconds: ts.Milliseconds + (Math.Abs(ts.Ticks) % TimeSpan.TicksPerMillisecond > TimeSpan.TicksPerMillisecond / 2 ? Math.Sign(ts.Milliseconds) * 1 : 0));

		public static bool IsNegative(this TimeSpan ts)
			=> ts < TimeSpan.Zero;

		public static bool IsPositive(this TimeSpan ts)
			=> ts > TimeSpan.Zero;

		public static TimeSpan Abs(this TimeSpan ts)
			=> ts >= TimeSpan.Zero ? ts : ts.Negate();

	}

}