using System;
using System.Runtime.ExceptionServices;

namespace Eon {

	/// <summary>
	/// Defines a value holder.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class ValueHolderClass<T> {

		readonly T _value;

		readonly ExceptionDispatchInfo _exception;

		public ValueHolderClass(T value) {
			_value = value;
			_exception = null;
		}

		public ValueHolderClass(Exception exception) {
			if (exception is null)
				throw new ArgumentNullException(paramName: nameof(exception));
			//
			_value = default;
			_exception = ExceptionDispatchInfo.Capture(source: exception);
		}

		public ValueHolderClass(ExceptionDispatchInfo exception) {
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