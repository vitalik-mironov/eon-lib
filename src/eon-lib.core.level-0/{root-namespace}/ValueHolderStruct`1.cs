using System;
using System.Runtime.ExceptionServices;

namespace Eon {

	public readonly struct ValueHolderStruct<T> {

		readonly T _value;

		readonly ExceptionDispatchInfo _exception;

		public ValueHolderStruct(T value) {
			_value = value;
			_exception = null;
		}

		public ValueHolderStruct(Exception exception) {
			if (exception is null)
				throw new ArgumentNullException(paramName: nameof(exception));
			//
			_value = default;
			_exception = ExceptionDispatchInfo.Capture(source: exception);
		}

		public ValueHolderStruct(ExceptionDispatchInfo exception) {
			_exception = exception ?? throw new ArgumentNullException(paramName: nameof(exception));
			_value = default;
		}

		public ExceptionDispatchInfo Exception
			=> _exception;

		public T Value {
			get {
				if (_exception is null)
					return _value;
				else {
					try {
						_exception.Throw();
					}
					catch (Exception exception) {
						throw new ValueGetterException(item: this, innerException: exception);
					}
					throw new ValueGetterException(item: this, innerException: null);
				}
			}
		}

		internal T ValueExceptionTolerant
			=> _value;

	}

}