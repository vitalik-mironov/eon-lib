using System.Threading;

namespace Eon.Threading {

	public readonly struct StatefulSynchronizationContextInvokeArgs<TCtxState, TState> {

		public readonly IStatefulSynchronizationContext<TCtxState> SyncCtx;

		public readonly TCtxState SyncCtxState;

		public readonly TState State;

		public readonly CancellationToken Ct;

		public StatefulSynchronizationContextInvokeArgs(IStatefulSynchronizationContext<TCtxState> syncCtx, TCtxState syncCtxState, TState state, CancellationToken ct) {
			SyncCtx = syncCtx;
			SyncCtxState = syncCtxState;
			State = state;
			Ct = ct;
		}

	}

}