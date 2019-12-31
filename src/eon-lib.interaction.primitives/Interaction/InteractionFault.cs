using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Eon.Linq;

namespace Eon.Interaction {

	[DataContract]
	public class InteractionFault
		:IInteractionFault {

		public InteractionFault() {
			InnerFaults = new InteractionInnerFault[ ] { };
		}

		public InteractionFault(IInteractionFault other) {
			other.EnsureNotNull(nameof(other));
			//
			Code =
				other
				.Code
				.Fluent().ConvertNullCond(locCode => new InteractionFaultCode(locCode));
			Message = other.Message;
			TypeName = other.TypeName;
			StackTrace = other.StackTrace;
			InnerFaults =
				other
				.InnerFaults
				.EmptyIfNull()
				.Select(i => i == null ? new InteractionInnerFault() : new InteractionInnerFault(i))
				.ToArray();
		}

		[DataMember(Order = 0, IsRequired = false, EmitDefaultValue = false)]
		public InteractionFaultCode Code { get; set; }

		IInteractionFaultCode IInteractionFault.Code
			=> Code;

		[DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
		public string Message { get; set; }

		[DataMember(Order = 2, IsRequired = false, EmitDefaultValue = false)]
		public string TypeName { get; set; }

		[DataMember(Order = 3, IsRequired = false, EmitDefaultValue = false)]
		public string StackTrace { get; set; }

		[DataMember(Order = 4, IsRequired = false, EmitDefaultValue = false)]
		public InteractionInnerFault[ ] InnerFaults { get; set; }

		IEnumerable<IInteractionFault> IInteractionFault.InnerFaults
			=> InnerFaults.EmptyIfNull().Select(i => i.Fault).SkipNull();

	}

}