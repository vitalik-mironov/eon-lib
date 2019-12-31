using Eon.Threading;

namespace Eon.Runtime.Options {

	public sealed class LockSmallTimeoutOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value: '00:00:02.819'.
		/// </summary>
		public static readonly TimeoutDuration MaxTimeout;

		/// <summary>
		/// Value: '00:00:00.937' (see <see cref="Timeout"/>).
		/// </summary>
		public static readonly LockSmallTimeoutOption Fallback;

		static LockSmallTimeoutOption() {
			MaxTimeout = TimeoutDuration.FromMilliseconds(milliseconds: 2819);
			Fallback = new LockSmallTimeoutOption(timeout: TimeoutDuration.FromMilliseconds(milliseconds: 937));
			RuntimeOptions.Option<LockSmallTimeoutOption>.SetFallback(option: Fallback);
		}

		public static TimeoutDuration Require()
			=> RuntimeOptions.Option<LockSmallTimeoutOption>.Require().Timeout;

		#endregion

		readonly TimeoutDuration _timeout;

		public LockSmallTimeoutOption(TimeoutDuration timeout) {
			timeout.EnsureNotNull(nameof(timeout)).EnsureNotInfinite().Value.Milliseconds.ArgProp($"{nameof(timeout)}.{nameof(timeout.Milliseconds)}").EnsureNotGreaterThan(operand: MaxTimeout.Milliseconds);
			//
			_timeout = timeout;
		}

		public TimeoutDuration Timeout
			=> _timeout;

	}

}