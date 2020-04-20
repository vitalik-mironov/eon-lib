using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

using Eon.Transactions;

using Microsoft.EntityFrameworkCore.Storage;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Data.Persistence.EfCore {

	[DebuggerDisplay("{ToString(),nq}")]
	public sealed class PersistenceEfCoreDataContextTxScope
		:ITransactionScopeProxy {

		sealed class P_State {

			public readonly bool InitializationDone;

			public readonly IDbContextTransaction RealTx;

			public readonly Action<PersistenceEfCoreDataContextTxScope> DisposeCallback;

			public readonly Action<PersistenceEfCoreDataContextTxScope> CompleteCallback;

			public readonly bool ShouldRollback;

			public readonly bool CommitIntention;

			public readonly bool FinishingStart;

			internal P_State(bool initializationDone, IDbContextTransaction realTx, Action<PersistenceEfCoreDataContextTxScope> disposeCallback, Action<PersistenceEfCoreDataContextTxScope> completeCallback, bool shouldRollback = default, bool commitIntention = default, bool finishingStart = default) {
				InitializationDone = initializationDone;
				RealTx = realTx;
				DisposeCallback = disposeCallback;
				CompleteCallback = completeCallback;
				ShouldRollback = shouldRollback;
				CommitIntention = commitIntention;
				FinishingStart = finishingStart;
			}

			internal P_State(
				P_State other,
				ArgumentPlaceholder<bool> initializationDone = default,
				ArgumentPlaceholder<IDbContextTransaction> realTx = default,
				ArgumentPlaceholder<Action<PersistenceEfCoreDataContextTxScope>> disposeCallback = default,
				ArgumentPlaceholder<Action<PersistenceEfCoreDataContextTxScope>> completeCallback = default,
				ArgumentPlaceholder<bool> shouldRollback = default,
				ArgumentPlaceholder<bool> commitIntention = default,
				ArgumentPlaceholder<bool> finishingStart = default) {
				//
				InitializationDone = initializationDone.Substitute(other.InitializationDone);
				RealTx = realTx.Substitute(other.RealTx);
				DisposeCallback = disposeCallback.Substitute(other.DisposeCallback);
				CompleteCallback = completeCallback.Substitute(other.CompleteCallback);
				ShouldRollback = shouldRollback.Substitute(other.ShouldRollback);
				CommitIntention = commitIntention.Substitute(other.CommitIntention);
				FinishingStart = finishingStart.Substitute(other.FinishingStart);
			}

		}

		P_State _state;

		public PersistenceEfCoreDataContextTxScope(IsolationLevel isolationLevel) {
			RuntimeHashCodeHex = RuntimeHelpers.GetHashCode(o: this).ToString("x8");
			Outer = null;
			NestingLevel = 0;
			IsolationLevel = isolationLevel;
		}

		public PersistenceEfCoreDataContextTxScope(PersistenceEfCoreDataContextTxScope outer) {
			if (outer is null)
				throw new ArgumentNullException(paramName: nameof(outer));
			//
			RuntimeHashCodeHex = RuntimeHelpers.GetHashCode(o: this).ToString("x8");
			Outer = outer;
			NestingLevel = checked(outer.NestingLevel + 1);
			IsolationLevel = outer.IsolationLevel;
		}

		public string RuntimeHashCodeHex { get; }

		public PersistenceEfCoreDataContextTxScope Outer { get; }

		public int NestingLevel { get; }

		public IsolationLevel IsolationLevel { get; }

		public bool InitializationDone
			=> itrlck.Get(location: ref _state)?.InitializationDone ?? false;

		public bool CommitIntention
			=> itrlck.Get(location: ref _state)?.CommitIntention ?? false;

		public bool ShouldRollback
			=> (itrlck.Get(location: ref _state)?.ShouldRollback ?? false) || (Outer?.ShouldRollback ?? false);

		IDisposable ITransactionScopeProxy.RealScope
			=> NopDisposable.Instance;

		public IDbContextTransaction RealTx
			=> itrlck.Get(location: ref _state)?.RealTx;

		public bool FinishingStart
			=> itrlck.Get(location: ref _state)?.FinishingStart ?? false;

		public void Initialize(Action<PersistenceEfCoreDataContextTxScope> disposeCallback, Action<PersistenceEfCoreDataContextTxScope> completeCallback, IDbContextTransaction realTx = default) {
			for (; ; ) {
				var current = itrlck.Get(location: ref _state);
				if (current is null) {
					if (itrlck.UpdateBool(location: ref _state, value: new P_State(initializationDone: true, disposeCallback: disposeCallback, completeCallback: completeCallback, realTx: realTx), comparand: current))
						break;
					else
						continue;
				}
				else if (current.FinishingStart)
					throw new EonException(message: "Dispose has been called already.");
				else
					throw new EonException(message: "Initialization must be done once.");
			}
		}

		public void SetCommitIntention() {
			for (; ; ) {
				var current = itrlck.Get(location: ref _state);
				if (current is null)
					throw new EonException(message: "Not initialized yet.");
				else if (current.FinishingStart)
					throw new EonException(message: "Dispose has been called already.");
				else if (current.CommitIntention || itrlck.UpdateBool(location: ref _state, comparand: current, value: new P_State(other: current, commitIntention: true)))
					break;
			}
		}

		public void SetShouldRollback() {
			for (; ; ) {
				var current = itrlck.Get(location: ref _state);
				if (current is null)
					throw new EonException(message: "Not initialized yet.");
				else if (current.FinishingStart)
					throw new EonException(message: "Dispose has been called already.");
				else if (current.ShouldRollback || itrlck.UpdateBool(location: ref _state, comparand: current, value: new P_State(other: current, shouldRollback: true)))
					break;
			}
		}

		void ITransactionScopeProxy.Complete()
			=> itrlck.Get(location: ref _state).CompleteCallback(obj: this);

		public void Reset()
			=> itrlck.SetNull(location: ref _state);

		void IDisposable.Dispose()
			=> Finish(noCallback: false, shouldRollback: false);

		public void Finish(bool noCallback, bool shouldRollback) {
			for (; ; ) {
				var state = itrlck.Get(location: ref _state);
				var stringRepresentation = ToString();
				if (!ReferenceEquals(objA: state, objB: itrlck.Get(location: ref _state)))
					continue;
				else if (state is null) {
					if (itrlck.UpdateBool(location: ref _state, comparand: state, value: new P_State(initializationDone: false, realTx: null, completeCallback: null, disposeCallback: null, finishingStart: true)))
						break;
				}
				else if (state.FinishingStart)
					break;
				else if (itrlck.UpdateBool(location: ref _state, comparand: state, value: new P_State(other: state, disposeCallback: null, completeCallback: null, realTx: null, finishingStart: true))) {
					var realTx = state.RealTx;
					var caughtExceptions = new List<Exception>(capacity: 3);
					try {
						if (!noCallback)
							try { state.DisposeCallback?.Invoke(obj: this); }
							catch (Exception exception) { caughtExceptions.Add(exception); }
						//
						if (!(realTx is null))
							try {
								if (!(shouldRollback || caughtExceptions.Count != 0 || state.ShouldRollback) && state.CommitIntention)
									realTx.Commit();
								else if (!PersistenceEfCoreDataContextTxScopeSettings.Default.RollbackThroughDispose)
									realTx.Rollback();
							}
							catch (Exception exception) { caughtExceptions.Add(exception); }
					}
					finally {
						if (!(realTx is null))
							try { realTx.Dispose(); }
							catch (Exception exception) { caughtExceptions.Add(exception); }
					}
					if (caughtExceptions.Count != 0)
						throw new EonException(message: $"An exception occurred while finishing the tx scope.{Environment.NewLine}\tTx scope:{stringRepresentation.FmtStr().GNLI2()}", innerException: caughtExceptions.Count == 1 ? caughtExceptions[ 0 ] : new AggregateException(innerExceptions: caughtExceptions));
					break;
				}
			}
		}

		public override string ToString()
			=> $"(identity: 0x{RuntimeHashCodeHex}, nesting-level: {NestingLevel.ToString("d", CultureInfo.InvariantCulture)}, commit-intention: {(CommitIntention ? "y" : "n")}, should-rollback: {(ShouldRollback ? "y" : "n")}, finishing-start: {(FinishingStart ? "y" : "n")}, isolation: {IsolationLevel})";

	}

}