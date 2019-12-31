using System.Threading;

namespace Eon.Threading {

	public delegate TResult StatefulSynchronizationContextExecute<in TContextState, in TState, out TResult>(IStatefulSynchronizationContext<TContextState> ctx, IVh<TContextState> ctxState, TState state, CancellationToken ct);

}