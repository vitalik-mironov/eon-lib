using System.Diagnostics;
using Eon.Description;

namespace Eon {

	//[DebuggerStepThrough]
	//[DebuggerNonUserCode]
	public sealed class XInstanceStateSummary
		:IXInstanceStateSummary {

		readonly IDescriptionSummary _description;

		readonly XInstanceStates _state;

		public XInstanceStateSummary(
			IDescriptionSummary descriptionSummary,
			XInstanceStates state) {
			//
			_description = descriptionSummary;
			_state = state;
		}

		public IDescriptionSummary Description
			=> _description;

		public XInstanceStates State
			=> _state;

	}

}