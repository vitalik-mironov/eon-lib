namespace Eon {

	public abstract class CloneContextBase
		:Disposable, ICloneContext {

		object _cloneOrigin;

		readonly bool _setCloneOrigin;

		protected CloneContextBase(object cloneOrigin, bool setCloneOrigin) {
			cloneOrigin.EnsureNotNull(nameof(cloneOrigin));
			//
			_cloneOrigin = cloneOrigin;
			_setCloneOrigin = setCloneOrigin;
		}

		public object CloneOrigin {
			get { return ReadDA(ref _cloneOrigin); }
		}

		public bool SetCloneOrigin {
			get { return _setCloneOrigin; }
		}

		protected override void Dispose(bool explicitDispose) {
			_cloneOrigin = null;
			base.Dispose(explicitDispose);
		}

	}

}