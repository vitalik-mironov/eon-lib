using System;

namespace Eon.Transactions {

	public sealed class DummyTransactionScope
		:Disposable, ITransactionScopeProxy {

		#region Static members

		public static readonly DummyTransactionScope Instance = new DummyTransactionScope();

		#endregion

		DummyTransactionScope() { }

		public IDisposable RealScope
			=> throw new NotImplementedException().SetErrorCode(code: GeneralErrorCodes.Operation.NotImplemented);

		public void Complete() { }

	}

}