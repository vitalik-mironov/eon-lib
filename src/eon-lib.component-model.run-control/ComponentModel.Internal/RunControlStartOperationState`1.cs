using System;
using System.Threading;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.ComponentModel.Internal {

	using StopRequestSignalState = Tuple<CancellationTokenSource, bool>;

	internal sealed class RunControlStartOperationState<TComponent>
		:RunControlOperationState<TComponent>
		where TComponent : class {

		StopRequestSignalState _stopRequestSignalState;

		internal RunControlStartOperationState(RunControl<TComponent> runControl, TimeSpan beginTimestamp, CancellationToken ct)
			: base(runControl: runControl, beginTimestamp: beginTimestamp, ct: ct) { }

		public void SignalStopRequested() {
			var updateResult = itrlck.Update(location: ref _stopRequestSignalState, transform: (locCurrent) => locCurrent?.Item2 == true ? locCurrent : new StopRequestSignalState(item1: locCurrent?.Item1, item2: true));
			if (updateResult.IsUpdated && !(updateResult.Current.Item1 is null))
				try {
					updateResult.Current.Item1.Cancel(throwOnFirstException: false);
				}
				catch (ObjectDisposedException) { }
		}

		public void SetStopRequestTokenSource(CancellationTokenSource cts) {
			cts.EnsureNotNull(nameof(cts));
			//
			var updateResult =
				itrlck
				.Update(
					location: ref _stopRequestSignalState,
					transform:
						(locCurrent) => {
							if (locCurrent is null)
								return new StopRequestSignalState(item1: cts, item2: false);
							else if (ReferenceEquals(locCurrent.Item1, cts))
								return locCurrent;
							else if (!(locCurrent.Item1 is null))
								throw new EonException();
							else
								return new StopRequestSignalState(item1: cts, item2: locCurrent.Item2);
						});
			if (updateResult.IsUpdated && updateResult.Current.Item2)
				updateResult.Current.Item1.Cancel(throwOnFirstException: false);
		}

	}

}