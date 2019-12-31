using System;
using System.Threading;

namespace Eon.ComponentModel.Internal {

	internal sealed class RunControlStopOperationState<TComponent>
		:RunControlOperationState<TComponent>
		where TComponent : class {

		internal RunControlStopOperationState(RunControl<TComponent> runControl, TimeSpan beginTimestamp, CancellationToken ct, bool finiteStop)
			: base(runControl: runControl, beginTimestamp: beginTimestamp, ct: ct) {
			FiniteStop = finiteStop;
		}

		public bool FiniteStop { get; }

	}

}