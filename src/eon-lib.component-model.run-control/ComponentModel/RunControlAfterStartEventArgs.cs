namespace Eon.ComponentModel {

	public class RunControlAfterStartEventArgs
		:RunControlEventArgs {

		public RunControlAfterStartEventArgs(IRunControlAttemptSuccess result)
			: base(runControl: result.EnsureNotNull(nameof(result)).Value.RunControl) {
			Result = result;
		}

		public IRunControlAttemptSuccess Result { get; }

	}

}