using System;

namespace Eon.Transactions {

	public interface ITransactionScopeProxy
		:IDisposable {

		IDisposable RealScope { get; }

		void Complete();

	}

}