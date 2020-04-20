using Eon.Threading;

namespace Eon.Runtime.Options {

	public sealed class AsyncOperationTimeoutOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value: '00:00:20.011' (see <see cref="Timeout"/>).
		/// </summary>
		public static readonly AsyncOperationTimeoutOption Fallback;

		static AsyncOperationTimeoutOption() {
			Fallback = new AsyncOperationTimeoutOption(timeout: TimeoutDuration.FromMilliseconds(milliseconds: 20011));
			RuntimeOptions.Option<AsyncOperationTimeoutOption>.SetFallback(option: Fallback);
		}

		public static TimeoutDuration Require()
			=> RuntimeOptions.Option<AsyncOperationTimeoutOption>.Require().Timeout;

		#endregion


		public AsyncOperationTimeoutOption(TimeoutDuration timeout) {
			timeout.EnsureNotNull(nameof(timeout)).EnsureNotInfinite();
			//
			Timeout = timeout;
		}

		public TimeoutDuration Timeout { get; }

	}

}