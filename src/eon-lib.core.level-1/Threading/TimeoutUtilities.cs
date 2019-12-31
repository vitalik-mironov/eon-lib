using System;
using System.Diagnostics;
using System.Threading;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Threading {

	public static class TimeoutUtilities {

		public static bool IsValidNonInfiniteTimeout(int timeout)
			=> timeout > Timeout.Infinite;

		public static bool IsValidTimeout(int timeout)
			=> timeout >= Timeout.Infinite;

		public static void EnsureValidTimeout(int timeout, string altTimeoutArgName = null) {
			if (!IsValidTimeout(timeout))
				throw
					new ArgumentOutOfRangeException(
						paramName: string.IsNullOrEmpty(altTimeoutArgName) ? nameof(timeout) : altTimeoutArgName,
						message: FormatXResource(typeof(TimeoutUtilities), "InvalidTimeout", timeout));
		}

		public static void EnsureValidNonInfiniteTimeout(int timeout, string altTimeoutArgName = null) {
			if (!IsValidNonInfiniteTimeout(timeout))
				throw new
					ArgumentOutOfRangeException(
						paramName: string.IsNullOrEmpty(altTimeoutArgName) ? nameof(timeout) : altTimeoutArgName,
						message: FormatXResource(typeof(TimeoutUtilities), "InvalidNonInfiniteTimeout", timeout));
		}

		public static TimeoutDuration Min(this TimeoutDuration a, TimeoutDuration b) {
			a.EnsureNotNull(nameof(a));
			b.EnsureNotNull(nameof(b));
			//
			if (a.IsInfinite)
				return b;
			else if (b.IsInfinite)
				return a;
			else if (a.Milliseconds < b.Milliseconds)
				return a;
			else
				return b;
		}

		public static TimeoutDuration Max(this TimeoutDuration a, TimeoutDuration b) {
			a.EnsureNotNull(nameof(a));
			b.EnsureNotNull(nameof(b));
			//
			if (a.IsInfinite)
				return a;
			else if (b.IsInfinite)
				return b;
			else if (a.Milliseconds > b.Milliseconds)
				return a;
			else
				return b;
		}

		public static TimeoutDuration SubtractElapsed(this TimeoutDuration timeout, Stopwatch stopwatch) {
			timeout.EnsureNotNull(nameof(timeout));
			stopwatch.EnsureNotNull(nameof(stopwatch));
			//
			if (timeout.IsInfinite)
				return timeout;
			else {
				var elapsedMilliseconds = (int)Math.Min(int.MaxValue, stopwatch.ElapsedMilliseconds);
				if (elapsedMilliseconds >= timeout.Milliseconds)
					return TimeoutDuration.Zero;
				else
					return new TimeoutDuration(milliseconds: timeout.Milliseconds - elapsedMilliseconds);
			}
		}

	}

}