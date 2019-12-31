using System;

using Eon.Diagnostics;

namespace Eon {

	public delegate Exception ExceptionFactoryDelegate2(string message, Exception innerException = default, IErrorCode errorCode = default);

}