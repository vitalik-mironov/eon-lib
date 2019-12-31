using System;
using System.Runtime.ExceptionServices;

namespace Eon {

	public readonly struct PropertyValueHolder<TInstance, T>
		where TInstance : class {

		readonly ValueHolderStruct<T> _holder;

		readonly TInstance _instance;

		public PropertyValueHolder(TInstance instance, T value) {
			if (instance is null)
				throw new ArgumentNullException(paramName: nameof(instance));
			//
			_holder = new ValueHolderStruct<T>(value: value);
			_instance = instance;
		}

		public PropertyValueHolder(TInstance instance, Exception exception) {
			if (instance is null)
				throw new ArgumentNullException(paramName: nameof(instance));
			//
			_holder = new ValueHolderStruct<T>(exception: exception);
			_instance = instance;
		}

		public PropertyValueHolder(TInstance instance, ExceptionDispatchInfo exception) {
			if (instance is null)
				throw new ArgumentNullException(paramName: nameof(instance));
			//
			_holder = new ValueHolderStruct<T>(exception: exception);
			_instance = instance;
		}

		public TInstance Instance
			=> _instance;

		public ExceptionDispatchInfo Exception
			=> _holder.Exception;

		public T Value
			=> _holder.Value;

	}

}