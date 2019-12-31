using System;

using vlt = Eon.Threading.VolatileUtilities;

namespace Eon {

	public class BubbleEventArgs
		:EventArgs {

		readonly object _originator;

		bool _handleState;

		public BubbleEventArgs(object originator = default) {
			_originator = originator;
			_handleState = false;
		}

		public bool IsHandled {
			get => vlt.Read(ref _handleState);
			set => vlt.Write(ref _handleState, value);
		}

		public object Originator
			=> _originator;

	}

}