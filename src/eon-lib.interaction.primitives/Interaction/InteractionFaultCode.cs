using System;
using System.Runtime.Serialization;

using Eon.Diagnostics;

namespace Eon.Interaction {

	[DataContract]
	public sealed class InteractionFaultCode
		:IInteractionFaultCode {

		public InteractionFaultCode() { }

		public InteractionFaultCode(IInteractionFaultCode other) {
			other.EnsureNotNull(nameof(other));
			//
			Identifier = other.Identifier;
			Description = other.Description;
			SeverityLevel = other.SeverityLevel;
		}

		public InteractionFaultCode(IErrorCode code) {
			code.EnsureNotNull(nameof(code));
			//
			Identifier = code.Identifier;
			Description = code.Description;
			SeverityLevel = code.SeverityLevel;
		}

		[DataMember(Order = 0, IsRequired = true)]
		public string Identifier { get; set; }

		[DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
		public string Description { get; set; }

		[DataMember(Order = 2, IsRequired = false, EmitDefaultValue = false)]
		public SeverityLevel? SeverityLevel { get; set; }

		// TODO: Put strings into the resources.
		//
		public override string ToString()
			=>
			$"Степень серьёзности:{(SeverityLevel?.ToString()).FmtStr().GNLI()}"
			+ $"{Environment.NewLine}ИД:{Identifier.FmtStr().GNLI()}"
			+ $"{Environment.NewLine}Описание:{Description.FmtStr().GNLI()}";

	}

}