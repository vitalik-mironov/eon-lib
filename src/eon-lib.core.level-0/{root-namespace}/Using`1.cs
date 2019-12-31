using System;

namespace Eon {

	public readonly struct Using<T>
		:IDisposable {

		readonly bool _isInitialized;

		readonly T _value;

		readonly DisposeAction<T> _dispose;

		public Using(T value, DisposeAction<T> dispose) {
			_value = value;
			_dispose = dispose;
			_isInitialized = true;
		}

		public T Value => _value;

		public bool IsInitialized => _isInitialized;

		internal void Internal_GetArgs(out T value, out DisposeAction<T> dispose) {
			value = _value;
			dispose = _dispose;
		}

		public void Dispose() => _dispose?.Invoke(value: _value, explicitDispose: true);

		public override string ToString() => _value?.ToString();

	}

}