using System;

namespace Eon {

	public class OnceCanceledEventArgs
		:EventArgs {

		volatile bool _cancel;

		volatile bool _onceCanceled;

		public OnceCanceledEventArgs(bool cancel) {
			_cancel = cancel;
			_onceCanceled = cancel;
		}

		public OnceCanceledEventArgs()
			: this(false) { }

		public bool Cancel {
			get => _cancel;
			set {
				_onceCanceled = _onceCanceled || value;
				_cancel = value;
			}
		}

		public bool OnceCanceled
			=> _onceCanceled;

	}

}