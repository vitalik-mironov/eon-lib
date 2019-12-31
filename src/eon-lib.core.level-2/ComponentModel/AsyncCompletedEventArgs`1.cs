using System;
using System.ComponentModel;

namespace Eon.ComponentModel {

	public class AsyncCompletedEventArgs<TResult>
		:AsyncCompletedEventArgs {

		TResult _result;

		public AsyncCompletedEventArgs(Exception error, bool cancelled, object userState)
			: base(error, cancelled, userState) {
			_result = default;
		}

		public AsyncCompletedEventArgs(TResult result, object userState)
			: base(null, false, userState) {
			_result = result;
		}

		public AsyncCompletedEventArgs(TResult result)
			: this(result, (object)null) { }

		// TODO: Put exception messages into the resources.
		//
		public TResult GetResult() {
			if (!(Error is null))
				throw new EonException("Результат операции недоступен, так как операция была завершена сбоем.");
			else if (Cancelled)
				throw new EonException("Результат операции недоступен, так как выполнение операции было отменено.");
			return _result;
		}

	}

}