using System;
using System.Transactions;

namespace Eon.Transactions {

	public sealed class DefaultTxScopeProvider
		:ITxScopeProvider {

		#region Nested types

		sealed class P_TxScopeProxy
			:Disposable, ITransactionScopeProxy {

			TransactionScope _realScope;

			internal P_TxScopeProxy(TransactionScope realScope) {
				if (realScope is null)
					throw new ArgumentNullException(nameof(realScope));
				//
				_realScope = realScope;
			}

			public IDisposable RealScope
				=> ReadDA(ref _realScope);

			public void Complete()
				=> ReadDA(ref _realScope).Complete();

			protected override void Dispose(bool explicitDispose) {
				if (explicitDispose)
					_realScope?.Dispose();
				_realScope = null;
				//
				base.Dispose(explicitDispose);
			}

		}

		#endregion

		#region Static members

		/// <summary>
		/// Значение: '00:15:00'.
		/// </summary>
		public static readonly TimeSpan DefaultTransactionTimeout = TimeSpan.FromMinutes(15.0D);

		static TransactionScope P_RequireTransactionReadCommitted()
			=>
		 new TransactionScope(
			 scopeOption: TransactionScopeOption.Required,
			 transactionOptions:
				new TransactionOptions() {
					IsolationLevel = IsolationLevel.ReadCommitted,
					Timeout = DefaultTransactionTimeout
				},
			 asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled);

		static TransactionScope P_RequireTransactionRepeatableRead()
			=>
			new TransactionScope(
				scopeOption: TransactionScopeOption.Required,
				transactionOptions:
					new TransactionOptions() {
						IsolationLevel = IsolationLevel.RepeatableRead,
						Timeout = DefaultTransactionTimeout
					},
			 asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled);

		static TransactionScope P_RequireTransactionSerializable()
			=>
			new TransactionScope(
				scopeOption: TransactionScopeOption.Required,
				transactionOptions:
					new TransactionOptions() {
						IsolationLevel = IsolationLevel.Serializable,
						Timeout = DefaultTransactionTimeout
					},
			 asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled);

		static TransactionScope P_RequireTransactionSnapshot()
			=>
			new TransactionScope(
				scopeOption: TransactionScopeOption.Required,
				transactionOptions:
					new TransactionOptions() {
						IsolationLevel = IsolationLevel.Snapshot,
						Timeout = DefaultTransactionTimeout,
					},
				asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled);

		static TransactionScope P_SuppressTransaction()
			=> new TransactionScope(scopeOption: TransactionScopeOption.Suppress, asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled);

		static P_TxScopeProxy P_CreateTransactionScopeProxy(Func<TransactionScope> realScopeFactory) {
			if (realScopeFactory is null)
				throw new ArgumentNullException(nameof(realScopeFactory));
			//
			var realScope = default(TransactionScope);
			try {
				realScope = realScopeFactory();
				return new P_TxScopeProxy(realScope: realScope);
			}
			catch (Exception exception) {
				realScope?.Dispose(exception);
				throw;
			}
		}

		#endregion

		public DefaultTxScopeProvider() { }

		public ITransactionScopeProxy RequireTxReadCommitted()
			=> P_CreateTransactionScopeProxy(P_RequireTransactionReadCommitted);

		public ITransactionScopeProxy RequireTxRepeatableRead()
			=> P_CreateTransactionScopeProxy(P_RequireTransactionRepeatableRead);

		public ITransactionScopeProxy RequireTxSerializable()
			=> P_CreateTransactionScopeProxy(P_RequireTransactionSerializable);

		public ITransactionScopeProxy RequireTxSnapshot()
			=> P_CreateTransactionScopeProxy(P_RequireTransactionSnapshot);

		public ITransactionScopeProxy SuppressTx()
			=> P_CreateTransactionScopeProxy(P_SuppressTransaction);

	}

}