using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public class OperationIllegalParameterException
		:EonException {

		public OperationIllegalParameterException()
			: this(message: string.Empty) { }

		public OperationIllegalParameterException(string message)
			: this(message: message, innerException: null) { }

		public OperationIllegalParameterException(string message, Exception innerException)
			: base(message: string.IsNullOrEmpty(message) ? FormatXResource(typeof(OperationIllegalParameterException), "DefaultMessage") : message, innerException: innerException) {
			//
			this.SetErrorCode(code: GeneralErrorCodes.Operation.Params.Illegal);
		}

	}

}