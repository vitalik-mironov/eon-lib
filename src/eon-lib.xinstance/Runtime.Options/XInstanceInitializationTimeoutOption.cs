using Eon.Threading;

namespace Eon.Runtime.Options {

	public sealed class XInstanceInitializationTimeoutOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value: '00:00:20.011' (see <see cref="AsyncOperationTimeoutOption.Fallback"/>, <see cref="Timeout"/>).
		/// </summary>
		public static readonly XInstanceInitializationTimeoutOption Fallback;

		static XInstanceInitializationTimeoutOption() {
			Fallback = new XInstanceInitializationTimeoutOption(timeout: AsyncOperationTimeoutOption.Fallback.Timeout);
			RuntimeOptions.Option<XInstanceInitializationTimeoutOption>.SetFallback(option: Fallback);
		}

		public static TimeoutDuration Require()
			=> RuntimeOptions.Option<XInstanceInitializationTimeoutOption>.Require().Timeout;

		#endregion

		readonly TimeoutDuration _timeout;

		public XInstanceInitializationTimeoutOption(TimeoutDuration timeout) {
			timeout.EnsureNotNull(nameof(timeout)).EnsureNotInfinite();
			//
			_timeout = timeout;
		}

		public TimeoutDuration Timeout
			=> _timeout;

	}

}