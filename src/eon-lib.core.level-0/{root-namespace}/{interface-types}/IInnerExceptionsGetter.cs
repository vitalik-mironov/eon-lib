using System;
using System.Collections.Generic;

namespace Eon {

	public interface IInnerExceptionsGetter {

		IEnumerable<Exception> InnerExceptions { get; }

	}

}