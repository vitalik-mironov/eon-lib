namespace Eon.ComponentModel {

	public class RunControlBeforeStartEventArgs
		:RunControlEventArgs {

		readonly IRunControlAttemptState _state;

		public RunControlBeforeStartEventArgs(IRunControlAttemptState state)
			: base(runControl: state.EnsureNotNull(nameof(state)).Value.RunControl) {
			_state = state;
		}

		public IRunControlAttemptState State
			=> _state;

	}

}