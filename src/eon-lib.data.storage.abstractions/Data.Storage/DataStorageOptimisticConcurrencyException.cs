using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Data.Storage {

	public class DataStorageOptimisticConcurrencyException
		:EonException {

		public DataStorageOptimisticConcurrencyException()
			: this(message: string.Empty, innerException: null) { }

		public DataStorageOptimisticConcurrencyException(string message)
			: this(message: message, innerException: null) { }

		public DataStorageOptimisticConcurrencyException(string message, Exception innerException)
			: base(
					message:
						string.IsNullOrEmpty(message)
						? FormatXResource(locator: typeof(DataStorageOptimisticConcurrencyException), subpath: "DefaultMessage")
						: FormatXResource(locator: typeof(DataStorageOptimisticConcurrencyException), subpath: "UserMessage", args: new object[ ] { message }),
					innerException: innerException) {
			//
			this.SetErrorCode(code: DataErrorCodes.Entity.OptimisticConcurrencyLoss);
		}

	}

}