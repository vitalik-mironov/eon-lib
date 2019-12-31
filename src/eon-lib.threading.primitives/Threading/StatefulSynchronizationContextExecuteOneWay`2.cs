using System.Threading;

namespace Eon.Threading {

	public delegate void StatefulSynchronizationContextExecuteOneWay<in TContextState, in TState>(IStatefulSynchronizationContext<TContextState> ctx, IVh<TContextState> ctxState, TState state, CancellationToken ct);

}