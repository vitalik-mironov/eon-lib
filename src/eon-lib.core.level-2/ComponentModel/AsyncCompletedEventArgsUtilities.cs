using System;
using System.ComponentModel;

namespace Eon.ComponentModel {

	public static class AsyncCompletedEventArgsUtilities {

		// TODO: Put exception messages into the resources.
		//
		public static void EnsureCancelledNorFaulted(this AsyncCompletedEventArgs asyncCompletedEventArgs) {
			asyncCompletedEventArgs.EnsureNotNull(nameof(asyncCompletedEventArgs));
			//
			if (asyncCompletedEventArgs.Error != null)
				throw new EonException(message: "Во время выполнения операции возникла ошибка.", innerException: asyncCompletedEventArgs.Error);
			else if (asyncCompletedEventArgs.Cancelled)
				throw new OperationCanceledException(message: "Операция была отменена.");
		}

	}

}