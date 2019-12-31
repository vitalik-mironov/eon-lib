using Eon.Diagnostics;

namespace Eon.Interaction {

	public interface IInteractionFaultCode {

		string Identifier { get; }

		string Description { get; }

		SeverityLevel? SeverityLevel { get; }

	}

}