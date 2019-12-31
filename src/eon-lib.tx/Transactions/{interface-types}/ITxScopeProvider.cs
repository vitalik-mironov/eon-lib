
namespace Eon.Transactions {

	public interface ITxScopeProvider {

		ITransactionScopeProxy RequireTxSnapshot();

		ITransactionScopeProxy RequireTxSerializable();

		ITransactionScopeProxy RequireTxReadCommitted();

		ITransactionScopeProxy RequireTxRepeatableRead();

		ITransactionScopeProxy SuppressTx();

	}

}