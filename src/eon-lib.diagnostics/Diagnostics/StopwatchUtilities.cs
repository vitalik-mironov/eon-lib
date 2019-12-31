using System;
using System.Diagnostics;
namespace Eon.Diagnostics {

	public static class StopwatchUtilities {

		static readonly double __DateTimeTickFrequency;

		static StopwatchUtilities() {
			if (Stopwatch.IsHighResolution)
				__DateTimeTickFrequency = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;
			else
				__DateTimeTickFrequency = 1.0;
		}

		public static long GetTimestampAsMilliseconds() {
			if (Stopwatch.IsHighResolution)
				return checked((long)(Math.Ceiling((Stopwatch.GetTimestamp() * __DateTimeTickFrequency) / 10000.0)));
			else
				return DateTime.UtcNow.Ticks / 10000;
		}

		public static TimeSpan GetTimestampAsTimeSpan() {
			if (Stopwatch.IsHighResolution)
				return new TimeSpan(ticks: checked((long)(Math.Ceiling((Stopwatch.GetTimestamp() * __DateTimeTickFrequency)))));
			else
				return new TimeSpan(ticks: DateTime.UtcNow.Ticks);
		}

		public static long GetTimestampAsTicks() {
			if (Stopwatch.IsHighResolution)
				return checked((long)(Math.Ceiling((Stopwatch.GetTimestamp() * __DateTimeTickFrequency))));
			else
				return DateTime.UtcNow.Ticks;
		}

	}

}