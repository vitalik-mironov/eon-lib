using System;

namespace Eon.Interaction {

	public interface IInteractionFaultTransformer {

		Exception ToException(IInteractionFault fault);

		IInteractionFault FromException(Exception exception, bool includeDetails);

	}

}