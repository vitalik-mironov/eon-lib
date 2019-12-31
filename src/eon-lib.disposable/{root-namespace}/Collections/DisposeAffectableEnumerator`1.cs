using System.Collections;
using System.Collections.Generic;

using vlt = Eon.Threading.VolatileUtilities;

namespace Eon.Collections {

	public sealed class DisposeAffectableEnumerator<T>
		:IEnumerator<T> {

		#region Static members & constants

		const int __TrueFlag = 1;

		const int __FalseFlag = 0;

		#endregion

		readonly IEnumerator<T> _sourceEnumerator;

		readonly IOxyDisposable _disposableOwner;

		readonly bool _considerDisposeRequest;

		int _disposeStateFlag;

		public DisposeAffectableEnumerator(IEnumerable<T> source, IOxyDisposable owner, bool considerDisposeRequest) {
			source.EnsureNotNull(nameof(source));
			owner.EnsureNotNull(nameof(owner));
			//
			_sourceEnumerator = source.GetEnumerator();
			_disposableOwner = owner;
			_considerDisposeRequest = considerDisposeRequest;
			_disposeStateFlag = __FalseFlag;
		}

		public T Current {
			get {
				P_EnsireNotDisposeState();
				return _sourceEnumerator.Current;
			}
		}

		object IEnumerator.Current
			=> Current;

		void P_EnsireNotDisposeState() {
			if (vlt.Read(ref _disposeStateFlag) == __TrueFlag)
				throw DisposableUtilities.NewObjectDisposedException(disposable: this, disposeRequestedException: false);
			else
				_disposableOwner.EnsureNotDisposeState(considerDisposeRequest: _considerDisposeRequest);
		}

		public bool MoveNext() {
			P_EnsireNotDisposeState();
			return _sourceEnumerator.MoveNext();
		}

		public void Reset() {
			P_EnsireNotDisposeState();
			_sourceEnumerator.Reset();
		}

		public void Dispose()
			=> vlt.Write(ref _disposeStateFlag, __TrueFlag);

	}

}