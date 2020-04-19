using Eon.Diagnostics;

namespace Eon.Runtime.Options {

	public sealed class UnhandledExceptionObserverOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value: <see cref="NopUnhandledExceptionObserver.Instance"/>.
		/// </summary>
		public static readonly UnhandledExceptionObserverOption Fallback;

		static UnhandledExceptionObserverOption() {
			Fallback = new UnhandledExceptionObserverOption(observer: NopUnhandledExceptionObserver.Instance);
			RuntimeOptions.Option<UnhandledExceptionObserverOption>.SetFallback(option: Fallback);
		}

		public static IUnhandledExceptionObserver Require()
			=> RuntimeOptions.Option<UnhandledExceptionObserverOption>.Require().Observer;

		#endregion


		public UnhandledExceptionObserverOption(IUnhandledExceptionObserver observer) {
			observer.EnsureNotNull(nameof(observer));
			//
			Observer = observer;
		}

		public IUnhandledExceptionObserver Observer { get; }

	}

}