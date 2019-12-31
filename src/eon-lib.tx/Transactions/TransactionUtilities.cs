using System;

using Eon.ComponentModel.Dependencies;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Transactions {

	public static class TransactionUtilities {

		static DefaultTxScopeProvider __DefaultTxScopeProvider;

		static DefaultTxScopeProvider P_RequireDefaultTxProvider()
			=> itrlck.UpdateIfNull(location: ref __DefaultTxScopeProvider, factory: () => new DefaultTxScopeProvider());

		[Obsolete(message: "Do not use this method. Use the parameterless form of this method.", error: true)]
		public static ITransactionScopeProxy RequireTxSnapshot(IDependencySupport dependencies)
			=> dependencies.RequireService<ITxScopeProvider>().RequireTxSnapshot();

		public static ITransactionScopeProxy RequireTxSnapshot()
			=> P_RequireDefaultTxProvider().RequireTxSnapshot();

		[Obsolete(message: "Do not use this method. Use the parameterless form of this method.", error: true)]
		public static ITransactionScopeProxy RequireTxSerializable(IDependencySupport dependencies)
			=> dependencies.RequireService<ITxScopeProvider>().RequireTxSerializable();

		public static ITransactionScopeProxy RequireTxSerializable()
			=> P_RequireDefaultTxProvider().RequireTxSerializable();

		[Obsolete(message: "Do not use this method. Use the parameterless form of this method.", error: true)]
		public static ITransactionScopeProxy RequireTxRepeatableRead(IDependencySupport dependencies)
			=> dependencies.RequireService<ITxScopeProvider>().RequireTxRepeatableRead();

		public static ITransactionScopeProxy RequireTxRepeatableRead()
			=> P_RequireDefaultTxProvider().RequireTxRepeatableRead();

		[Obsolete(message: "Do not use this method. Use the parameterless form of this method.", error: true)]
		public static ITransactionScopeProxy RequireTxReadCommitted(IDependencySupport dependencies)
			=> dependencies.RequireService<ITxScopeProvider>().RequireTxReadCommitted();

		public static ITransactionScopeProxy RequireTxReadCommitted()
			=> P_RequireDefaultTxProvider().RequireTxReadCommitted();

		[Obsolete(message: "Do not use this method. Use the parameterless form of this method.", error: true)]
		public static ITransactionScopeProxy SuppressTx(IDependencySupport dependencies)
			=> dependencies.RequireService<ITxScopeProvider>().SuppressTx();

		public static ITransactionScopeProxy SuppressTx()
			=> P_RequireDefaultTxProvider().SuppressTx();

	}

}