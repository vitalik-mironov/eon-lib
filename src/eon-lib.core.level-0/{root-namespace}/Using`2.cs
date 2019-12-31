using System;

namespace Eon {

	public readonly struct Using<T, TInner>
		:IDisposable {

		readonly T _value;

		readonly Using<TInner> _inner;

		// TODO: Put strings into the resources.
		//
		public Using(T value, Using<TInner> inner) {
			if (!inner.IsInitialized)
				throw new ArgumentException(paramName: nameof(inner), message: "Не инициализирован.");
			//
			_value = value;
			_inner = inner;
		}

		public T Value => _value;

		public bool IsInitialized => _inner.IsInitialized;

		public void Dispose() => _inner.Dispose();

		public override string ToString() => _inner.ToString();

	}

}