namespace Eon.Prochost {

	public sealed class RunCallbackState {

		readonly XAppContainerControlRunState _state;

		bool _exitAfterCallback;

		internal RunCallbackState(XAppContainerControlRunState state) {
			state.EnsureNotNull(nameof(state));
			//
			_state = state;
			_exitAfterCallback = false;
		}

		public XAppContainerControlRunState State
			=> _state;

		public bool ExitAfterCallback {
			get => _exitAfterCallback;
			set => _exitAfterCallback = value;
		}

	}

}