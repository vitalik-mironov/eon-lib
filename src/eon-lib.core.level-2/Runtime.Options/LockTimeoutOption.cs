using Eon.Threading;

namespace Eon.Runtime.Options {

	public sealed class LockTimeoutOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value: '00:00:30.011' (see <see cref="Timeout"/>).
		/// </summary>
		public static readonly LockTimeoutOption Fallback;

		static LockTimeoutOption() {
			Fallback = new LockTimeoutOption(timeout: TimeoutDuration.FromMilliseconds(milliseconds: 30011));
			RuntimeOptions.Option<LockTimeoutOption>.SetFallback(option: Fallback);
		}

		public static TimeoutDuration Require()
			=> RuntimeOptions.Option<LockTimeoutOption>.Require().Timeout;

		#endregion

		readonly TimeoutDuration _timeout;

		public LockTimeoutOption(TimeoutDuration timeout) {
			timeout.EnsureNotNull(nameof(timeout)).EnsureNotInfinite();
			//
			_timeout = timeout;
		}

		public TimeoutDuration Timeout
			=> _timeout;

	}

}