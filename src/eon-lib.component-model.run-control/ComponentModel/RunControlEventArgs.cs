using System;

namespace Eon.ComponentModel {

	public class RunControlEventArgs
		:EventArgs {

		public RunControlEventArgs(IRunControl runControl) {
			runControl.EnsureNotNull(nameof(runControl));
			//
			RunControl = runControl;
		}

		public IRunControl RunControl { get; private set; }

	}

}