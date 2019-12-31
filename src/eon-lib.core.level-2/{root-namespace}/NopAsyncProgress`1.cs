using System;
using System.Threading.Tasks;

using Eon.ComponentModel;
using Eon.Threading.Tasks;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon {

	public sealed class NopAsyncProgress<T>
		:Disposable, IAsyncProgress<T> {

		#region Nested types

		sealed class P_NopReportedEventSubscribeManager
			:IAsyncEventSubscribeManager<AsyncProgressReportedEventArgs<T>> {

			#region Nested types

			sealed class P_NopDisposable
				:IDisposable {

				#region Static & constant members

				public static readonly P_NopDisposable Instance = new P_NopDisposable();

				#endregion

				P_NopDisposable() { }

				public void Dispose() { }

			}

			#endregion

			#region Static & constant members

			public static readonly P_NopReportedEventSubscribeManager Instance = new P_NopReportedEventSubscribeManager();

			#endregion

			P_NopReportedEventSubscribeManager() { }

			public IDisposable Subscribe(AsyncEventHandler<AsyncProgressReportedEventArgs<T>> asyncSubscriber)
				=> P_NopDisposable.Instance;

			public IDisposable Subscribe(EventHandler2<AsyncProgressReportedEventArgs<T>> subscriber)
				=> P_NopDisposable.Instance;

			public IDisposable Subscribe(IAsyncEventSubscriber<AsyncProgressReportedEventArgs<T>> subscriber)
				=> P_NopDisposable.Instance;

			public IDisposable Subscribe(IAsyncEventSubscriber<AsyncProgressReportedEventArgs<T>> subscriber, bool ownsSubscriber)
				=> P_NopDisposable.Instance;

			public void Dispose() { }

		}

		#endregion

		#region Static & constant members

		static NopAsyncProgress<T> __Instance;

		public static NopAsyncProgress<T> Instance {
			get {
				return
					itrlck
					.Update(
						location: ref __Instance,
						transform: locCurrent => locCurrent is null || locCurrent.IsDisposeRequested ? new NopAsyncProgress<T>() : locCurrent)
					.Current;
			}
		}

		#endregion

		public NopAsyncProgress() { }

		public IAsyncEventSubscribeManager<AsyncProgressReportedEventArgs<T>> ReportedEvent
			=> P_NopReportedEventSubscribeManager.Instance;

		public void Report(T value) { }

		public Task<bool> ReportAsync(T value)
			=> TaskUtilities.FromFalse();

		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose) {
				itrlck.SetNullBool(ref __Instance, comparand: this);
			}
			//
			base.FireBeforeDispose(explicitDispose);
		}

	}

}