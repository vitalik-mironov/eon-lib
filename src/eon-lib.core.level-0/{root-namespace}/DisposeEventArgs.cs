using System;

namespace Eon {

	public sealed class DisposeEventArgs
		:EventArgs {

		public static readonly DisposeEventArgs Explicit = new DisposeEventArgs(explicitDispose: true);

		public static readonly DisposeEventArgs Implicit = new DisposeEventArgs(explicitDispose: false);

		readonly bool _explicitDispose;

		public DisposeEventArgs(bool explicitDispose) {
			_explicitDispose = explicitDispose;
		}

		public bool ExplicitDispose => _explicitDispose;

	}

}