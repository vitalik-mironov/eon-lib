namespace Eon.MessageFlow.Local {

	public readonly struct LocalPublicationFilterResult {

		readonly ILocalPublicationFilterState _state;

		readonly bool _cancelPublication;

		public LocalPublicationFilterResult(ILocalPublicationFilterState state, bool cancelPublication) {
			state.EnsureNotNull(nameof(state));
			//
			_state = state;
			_cancelPublication = cancelPublication;
		}

		public ILocalPublicationFilterState State
			=> _state;

		public bool CancelPublication
			=> _cancelPublication;

	}

}