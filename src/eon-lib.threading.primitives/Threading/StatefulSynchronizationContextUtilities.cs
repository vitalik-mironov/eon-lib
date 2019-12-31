using System;
using System.Threading;
using System.Threading.Tasks;

using Eon.Context;

namespace Eon.Threading {

	public static class StatefulSynchronizationContextUtilities {

		public static async Task<TResult> InvokeAsync<TCtxState, TResult>(this IStatefulSynchronizationContext<TCtxState> syncCtx, Func<StatefulSynchronizationContextInvokeArgs<TCtxState, Nil>, TResult> func, CancellationToken ct = default) {
			syncCtx.EnsureNotNull(nameof(syncCtx));
			func.EnsureNotNull(nameof(func));
			//
			return await syncCtx.ExecuteAsync(func: execute, state: Nil.Value, ct: ct).ConfigureAwait(false);
			//
			TResult execute(IStatefulSynchronizationContext<TCtxState> locSyncCtx, IVh<TCtxState> locSyncCtxState, Nil locState, CancellationToken locCt)
				=> func(arg: new StatefulSynchronizationContextInvokeArgs<TCtxState, Nil>(syncCtx: locSyncCtx, syncCtxState: locSyncCtxState.Value, state: Nil.Value, ct: locCt));
		}

		public static async Task<TResult> InvokeAsync<TCtxState, TResult>(this IStatefulSynchronizationContext<TCtxState> syncCtx, Func<StatefulSynchronizationContextInvokeArgs<TCtxState, Nil>, TResult> func, IContext ctx = default) {
			syncCtx.EnsureNotNull(nameof(syncCtx));
			func.EnsureNotNull(nameof(func));
			//
			return await syncCtx.ExecuteAsync(func: execute, state: Nil.Value, ct: ctx.Ct()).ConfigureAwait(false);
			//
			TResult execute(IStatefulSynchronizationContext<TCtxState> locSyncCtx, IVh<TCtxState> locSyncCtxState, Nil locState, CancellationToken locCt)
				=> func(arg: new StatefulSynchronizationContextInvokeArgs<TCtxState, Nil>(syncCtx: locSyncCtx, syncCtxState: locSyncCtxState.Value, state: Nil.Value, ct: locCt));
		}

		public static async Task InvokeAsync<TCtxState>(this IStatefulSynchronizationContext<TCtxState> syncCtx, Action<StatefulSynchronizationContextInvokeArgs<TCtxState, Nil>> action, CancellationToken ct = default) {
			syncCtx.EnsureNotNull(nameof(syncCtx));
			action.EnsureNotNull(nameof(action));
			//
			await syncCtx.ExecuteAsync(func: execute, state: Nil.Value, ct: ct).ConfigureAwait(false);
			//
			Nil execute(IStatefulSynchronizationContext<TCtxState> locSyncCtx, IVh<TCtxState> locSyncCtxState, Nil locState, CancellationToken locCt) {
				action(obj: new StatefulSynchronizationContextInvokeArgs<TCtxState, Nil>(syncCtx: locSyncCtx, syncCtxState: locSyncCtxState.Value, state: Nil.Value, ct: locCt));
				return Nil.Value;
			}
		}

		public static async Task InvokeAsync<TCtxState>(this IStatefulSynchronizationContext<TCtxState> syncCtx, Action<StatefulSynchronizationContextInvokeArgs<TCtxState, Nil>> action, IContext ctx = default) {
			syncCtx.EnsureNotNull(nameof(syncCtx));
			action.EnsureNotNull(nameof(action));
			//
			await syncCtx.ExecuteAsync(func: execute, state: Nil.Value, ct: ctx.Ct()).ConfigureAwait(false);
			//
			Nil execute(IStatefulSynchronizationContext<TCtxState> locSyncCtx, IVh<TCtxState> locSyncCtxState, Nil locState, CancellationToken locCt) {
				action(obj: new StatefulSynchronizationContextInvokeArgs<TCtxState, Nil>(syncCtx: locSyncCtx, syncCtxState: locSyncCtxState.Value, state: Nil.Value, ct: locCt));
				return Nil.Value;
			}
		}

		public static async Task<TResult> InvokeAsync<TResult, TCtxState, TState>(this IStatefulSynchronizationContext<TCtxState> syncCtx, Func<StatefulSynchronizationContextInvokeArgs<TCtxState, TState>, TResult> func, TState state, CancellationToken ct = default) {
			syncCtx.EnsureNotNull(nameof(syncCtx));
			func.EnsureNotNull(nameof(func));
			//
			return await syncCtx.ExecuteAsync(func: execute, state: state, ct: ct).ConfigureAwait(false);
			//
			TResult execute(IStatefulSynchronizationContext<TCtxState> locSyncCtx, IVh<TCtxState> locSyncCtxState, TState locState, CancellationToken locCt)
				=> func(arg: new StatefulSynchronizationContextInvokeArgs<TCtxState, TState>(syncCtx: locSyncCtx, syncCtxState: locSyncCtxState.Value, state: locState, ct: locCt));
		}

		public static async Task<TResult> InvokeAsync<TResult, TCtxState, TState>(this IStatefulSynchronizationContext<TCtxState> syncCtx, Func<StatefulSynchronizationContextInvokeArgs<TCtxState, TState>, TResult> func, TState state, IContext ctx = default) {
			syncCtx.EnsureNotNull(nameof(syncCtx));
			func.EnsureNotNull(nameof(func));
			//
			return await syncCtx.ExecuteAsync(func: execute, state: state, ct: ctx.Ct()).ConfigureAwait(false);
			//
			TResult execute(IStatefulSynchronizationContext<TCtxState> locSyncCtx, IVh<TCtxState> locSyncCtxState, TState locState, CancellationToken locCt)
				=> func(arg: new StatefulSynchronizationContextInvokeArgs<TCtxState, TState>(syncCtx: locSyncCtx, syncCtxState: locSyncCtxState.Value, state: locState, ct: locCt));
		}

		public static async Task InvokeAsync<TCtxState, TState>(this IStatefulSynchronizationContext<TCtxState> syncCtx, Action<StatefulSynchronizationContextInvokeArgs<TCtxState, TState>> action, TState state, CancellationToken ct = default) {
			syncCtx.EnsureNotNull(nameof(syncCtx));
			action.EnsureNotNull(nameof(action));
			//
			await syncCtx.ExecuteAsync(func: execute, state: state, ct: ct).ConfigureAwait(false);
			//
			Nil execute(IStatefulSynchronizationContext<TCtxState> locSyncCtx, IVh<TCtxState> locSyncCtxState, TState locState, CancellationToken locCt) {
				action(obj: new StatefulSynchronizationContextInvokeArgs<TCtxState, TState>(syncCtx: locSyncCtx, syncCtxState: locSyncCtxState.Value, state: locState, ct: locCt));
				return Nil.Value;
			}
		}

		public static async Task InvokeAsync<TCtxState, TState>(this IStatefulSynchronizationContext<TCtxState> syncCtx, Action<StatefulSynchronizationContextInvokeArgs<TCtxState, TState>> action, TState state, IContext ctx = default) {
			syncCtx.EnsureNotNull(nameof(syncCtx));
			action.EnsureNotNull(nameof(action));
			//
			await syncCtx.ExecuteAsync(func: execute, state: state, ct: ctx.Ct()).ConfigureAwait(false);
			//
			Nil execute(IStatefulSynchronizationContext<TCtxState> locSyncCtx, IVh<TCtxState> locSyncCtxState, TState locState, CancellationToken locCt) {
				action(obj: new StatefulSynchronizationContextInvokeArgs<TCtxState, TState>(syncCtx: locSyncCtx, syncCtxState: locSyncCtxState.Value, state: locState, ct: locCt));
				return Nil.Value;
			}
		}

		public static void InvokeOneWay<TCtxState, TState>(this IStatefulSynchronizationContext<TCtxState> syncCtx, Action<StatefulSynchronizationContextInvokeArgs<TCtxState, TState>> action, TState state, CancellationToken ct = default) {
			syncCtx.EnsureNotNull(nameof(syncCtx));
			action.EnsureNotNull(nameof(action));
			//
			syncCtx.ExecuteOneWay(action: execute, state: state, ct: ct);
			//
			void execute(IStatefulSynchronizationContext<TCtxState> locSyncCtx, IVh<TCtxState> locSyncCtxState, TState locState, CancellationToken locCt)
				=> action(obj: new StatefulSynchronizationContextInvokeArgs<TCtxState, TState>(syncCtx: locSyncCtx, syncCtxState: locSyncCtxState.Value, state: locState, ct: locCt));
		}

		public static void InvokeOneWay<TCtxState, TState>(this IStatefulSynchronizationContext<TCtxState> syncCtx, Action<StatefulSynchronizationContextInvokeArgs<TCtxState, TState>> action, TState state, IContext ctx = default) {
			syncCtx.EnsureNotNull(nameof(syncCtx));
			action.EnsureNotNull(nameof(action));
			//
			syncCtx.ExecuteOneWay(action: execute, state: state, ct: ctx.Ct());
			//
			void execute(IStatefulSynchronizationContext<TCtxState> locSyncCtx, IVh<TCtxState> locSyncCtxState, TState locState, CancellationToken locCt)
				=> action(obj: new StatefulSynchronizationContextInvokeArgs<TCtxState, TState>(syncCtx: locSyncCtx, syncCtxState: locSyncCtxState.Value, state: locState, ct: locCt));
		}

		public static void InvokeOneWay<TCtxState>(this IStatefulSynchronizationContext<TCtxState> syncCtx, Action<StatefulSynchronizationContextInvokeArgs<TCtxState, Nil>> action, CancellationToken ct = default) {
			syncCtx.EnsureNotNull(nameof(syncCtx));
			action.EnsureNotNull(nameof(action));
			//
			syncCtx.ExecuteOneWay(action: execute, state: Nil.Value, ct: ct);
			//
			void execute(IStatefulSynchronizationContext<TCtxState> locSyncCtx, IVh<TCtxState> locSyncCtxState, Nil locState, CancellationToken locCt)
				=> action(obj: new StatefulSynchronizationContextInvokeArgs<TCtxState, Nil>(syncCtx: locSyncCtx, syncCtxState: locSyncCtxState.Value, state: locState, ct: locCt));
		}

		public static void InvokeOneWay<TCtxState>(this IStatefulSynchronizationContext<TCtxState> syncCtx, Action<StatefulSynchronizationContextInvokeArgs<TCtxState, Nil>> action, IContext ctx = default) {
			syncCtx.EnsureNotNull(nameof(syncCtx));
			action.EnsureNotNull(nameof(action));
			//
			syncCtx.ExecuteOneWay(action: execute, state: Nil.Value, ct: ctx.Ct());
			//
			void execute(IStatefulSynchronizationContext<TCtxState> locSyncCtx, IVh<TCtxState> locSyncCtxState, Nil locState, CancellationToken locCt)
				=> action(obj: new StatefulSynchronizationContextInvokeArgs<TCtxState, Nil>(syncCtx: locSyncCtx, syncCtxState: locSyncCtxState.Value, state: locState, ct: locCt));
		}

	}

}