using System;

namespace Eon {

	public delegate Exception ExceptionFactoryDelegate(string errorCodeIdentifier, string errorCodeDescription, string message, Exception innerException);

}