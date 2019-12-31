namespace Eon.ComponentModel {

	public class RunControlLogAttemptEventArgs
		:RunControlEventArgs {

		public RunControlLogAttemptEventArgs(IRunControlAttemptLoggingData data)
			: base(runControl: data.EnsureNotNull(nameof(data)).Value.RunControl) {
			Data = data;
		}

		public IRunControlAttemptLoggingData Data { get; private set; }

	}

}