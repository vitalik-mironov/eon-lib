using System;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel;
using Eon.Context;

namespace Eon.Threading {

	public interface IStatefulSynchronizationContext<out TCtxState> {

		XFullCorrelationId CorrelationId { get; }

		bool AutoStart { get; }

		IRunControl<IStatefulSynchronizationContext<TCtxState>> RunControl { get; }

		bool HasShutdownRequested { get; }

		bool HasShutdownFinished { get; }

		Task ShutdownAsync();

		void TryCancelInvokes();

		Task<TResult> ExecuteAsync<TState, TResult>(StatefulSynchronizationContextExecute<TCtxState, TState, TResult> func, TState state, CancellationToken ct = default);

		void ExecuteOneWay<TState>(StatefulSynchronizationContextExecuteOneWay<TCtxState, TState> action, TState state, CancellationToken ct = default);

	}

}