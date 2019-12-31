using System.Collections.Generic;

namespace Eon.Interaction {

	public interface IInteractionFault {

		IInteractionFaultCode Code { get; }

		string Message { get; }

		string TypeName { get; }

		string StackTrace { get; }

		IEnumerable<IInteractionFault> InnerFaults { get; }

	}

}