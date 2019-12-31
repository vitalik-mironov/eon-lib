using System.Runtime.Serialization;

namespace Eon.Interaction {

	[DataContract]
	public sealed class InteractionInnerFault {

		public InteractionInnerFault() { }

		public InteractionInnerFault(IInteractionFault fault) {
			fault.EnsureNotNull(nameof(fault));
			//
			Fault = fault.Fluent().ConvertNullCond(converter: locFault => new InteractionFault(locFault));
		}

		[DataMember(Order = 0, IsRequired = true)]
		public InteractionFault Fault { get; set; }

	}

}