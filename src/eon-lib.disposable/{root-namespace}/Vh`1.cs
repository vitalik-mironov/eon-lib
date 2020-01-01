using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using Eon.Reflection;
using Eon.Threading.Tasks;

using itrlck = Eon.Threading.InterlockedUtilities;
using vlt = Eon.Threading.VolatileUtilities;

namespace Eon {

	/// <summary>
	/// Defines implementation of <see cref="IVh{T}"/> — disposable value holder.
	/// </summary>
	/// <typeparam name="T">Value type constraint.</typeparam>
	public sealed class Vh<T>
		:CloneableDisposable, IVh<T> {

		#region Nested types

		static class P_SequenceCleanupTemplate<TElement> {

			static readonly Action<TElement> __ElementExplicitValueCleanup;

			static P_SequenceCleanupTemplate() {
				__ElementExplicitValueCleanup = P_CreateSingleValueExplicitCleanupDelegate<TElement>();
			}

			public static void ExplicitCleanup(IEnumerable<TElement> sequence) {
				if (!(sequence is null))
					foreach (var element in sequence)
						__ElementExplicitValueCleanup(element);
			}

		}

		#endregion

		#region Static members

		static readonly T __DefaultValue;

		static readonly Action<T> __ValueCleanup;

		static readonly Action<T> __ValueNopCleanup = _ => { };

		static readonly Func<T, ICloneContext, T> __CloneValue;

		static Vh() {
			__DefaultValue = default;
			//
			var typeOfValue = typeof(T);
			var valueExpr = Expression.Parameter(typeOfValue);
			if (typeOfValue.IsArray) {
				var elementType = typeOfValue.GetElementType();
				__ValueCleanup =
					Expression
					.Lambda<Action<T>>(
						Expression.Call(
							method: typeof(P_SequenceCleanupTemplate<>).MakeGenericType(typeOfValue, elementType).GetMethod(nameof(P_SequenceCleanupTemplate<T>.ExplicitCleanup), throwIfNotFound: true),
							arg0: Expression.Convert(valueExpr, typeof(IEnumerable<>).MakeGenericType(elementType))),
						valueExpr)
					.Compile();
			}
			else
				__ValueCleanup = P_CreateSingleValueExplicitCleanupDelegate<T>();
			//
			var valueStoreInstanceParameter = Expression.Parameter(typeof(Vh<T>));
			if (typeOfValue.IsValueType()) {
				if (typeof(IEonCloneable).IsAssignableFrom(typeOfValue)) {
					var cloneContextParameter = Expression.Parameter(typeof(ICloneContext));
					__CloneValue =
						Expression
						.Lambda<Func<T, ICloneContext, T>>(
							Expression.Call(
								instance: valueExpr,
								method: typeOfValue.GetMethod(
									nameof(IEonCloneable.Clone),
									parameterTypes: new[ ] { cloneContextParameter.Type },
									throwIfNotFound: true),
								arguments: cloneContextParameter),
							valueExpr,
							cloneContextParameter)
						.Compile();
				}
				else
					__CloneValue = (locValue, locContext) => locValue;
			}
			else {
				if (typeof(IEonCloneable).IsAssignableFrom(typeOfValue)) {
					var cloneContextParameter = Expression.Parameter(typeof(ICloneContext));
					__CloneValue =
						Expression
						.Lambda<Func<T, ICloneContext, T>>(
							Expression.Condition(
								Expression.ReferenceEqual(valueExpr, Expression.Constant(null, valueExpr.Type)),
								valueExpr,
								Expression.Convert(
									Expression.Call(
										Expression.Convert(valueExpr, typeof(IEonCloneable)),
										typeof(IEonCloneable).GetMethod(
											nameof(IEonCloneable.Clone),
											parameterTypes: new[ ] { cloneContextParameter.Type },
											throwIfNotFound: true),
										cloneContextParameter),
									valueExpr.Type)),
							valueExpr,
							cloneContextParameter)
						.Compile();
				}
				else
					__CloneValue = (locValue, locContext) => locValue;
			}
		}

		static Action<TSingleValue> P_CreateSingleValueExplicitCleanupDelegate<TSingleValue>() {
			var typeOfValue = typeof(TSingleValue);
			if (typeof(IDisposable).IsAssignableFrom(typeOfValue)) {
				var valueExpr = Expression.Parameter(typeOfValue);
				if (typeOfValue.IsValueType())
					return
						Expression
						.Lambda<Action<TSingleValue>>(
							Expression.Call(valueExpr, typeOfValue.GetMethod(nameof(IDisposable.Dispose), throwIfNotFound: true)),
							valueExpr)
						.Compile();
				else {
					var valueAsDisposableExpr = Expression.Convert(valueExpr, typeof(IDisposable));
					return
						Expression
						.Lambda<Action<TSingleValue>>(
							Expression.Block(
								Expression.IfThen(
									test: Expression.NotEqual(valueExpr, Expression.Constant(null, typeof(IDisposable))),
									ifTrue: Expression.Call(instance: valueAsDisposableExpr, method: typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose), throwIfNotFound: true)))),
							valueExpr)
						.Compile();
				}
			}
			else
				return locValue => { };
		}

		static T P_ValueGetter(Vh<T> holder, bool disposeTolerant) {
			if (disposeTolerant)
				return holder.TryReadDA(ref holder._value);
			else
				return holder.ReadDA(ref holder._value);
		}

		static T P_FailedState_ValueGetter(Vh<T> store, bool disposeTolerant) {
			if (disposeTolerant) {
				var exception = store.TryReadDA(location: ref store._exception);
				if (exception is null || ReferenceEquals(objA: exception, objB: ValueHolderUtilities.HasExceptionSentinel) || exception.SourceException is ObjectDisposedException)
					return default;
				else
					throw new ValueGetterException(innerException: exception.SourceException);
			}
			else
				throw new ValueGetterException(innerException: store.Exception.SourceException);
		}

		#endregion

		T _value;

		TaskWrap<T> _valueAwaitable;

		ExceptionDispatchInfo _exception;

		readonly Func<Vh<T>, bool, T> _valueGetter;

		readonly Action<T> _valueCleanup;

		Vh()
		 : this(value: __DefaultValue, ownsValue: false) { }

		Vh(T value)
			: this(value: value, ownsValue: false) { }

		Vh(T value, bool ownsValue)
			: this(value: value, ownsValue: ownsValue, nopDispose: false) { }

		Vh(T value, bool ownsValue, bool nopDispose)
			: base(nopDispose: nopDispose) {
			_exception = null;
			_value = value;
			_valueGetter = P_ValueGetter;
			if (ownsValue)
				_valueCleanup = __ValueCleanup;
			else
				_valueCleanup = __ValueNopCleanup;
		}

		public Vh(Exception exception) {
			exception.EnsureNotNull(nameof(exception));
			if (ReferenceEquals(objA: exception, objB: ValueHolderUtilities.HasExceptionSentinel.SourceException))
				throw new ArgumentOutOfRangeException(paramName: nameof(exception));
			//
			_exception = ExceptionDispatchInfo.Capture(source: exception);
			_value = __DefaultValue;
			_valueGetter = P_FailedState_ValueGetter;
			_valueCleanup = __ValueNopCleanup;
		}

		public Vh(ExceptionDispatchInfo exception) {
			exception.EnsureNotNull(nameof(exception));
			if (ReferenceEquals(objA: exception, objB: ValueHolderUtilities.HasExceptionSentinel))
				throw new ArgumentOutOfRangeException(paramName: nameof(exception));
			//
			_exception = exception;
			_value = __DefaultValue;
			_valueGetter = P_FailedState_ValueGetter;
			_valueCleanup = __ValueNopCleanup;
		}

		public T Value
			=> _valueGetter(arg1: this, arg2: false);

		public bool OwnsValue
			=> ReferenceEquals(_valueCleanup, __ValueCleanup);

		public T ValueDisposeTolerant
			=> _valueGetter(arg1: this, arg2: true);

		public bool HasException
			=> !(itrlck.Get(location: ref _exception) is null);

		public ExceptionDispatchInfo Exception {
			get {
				var exception = itrlck.Get(location: ref _exception);
				if (ReferenceEquals(objA: exception, objB: ValueHolderUtilities.HasExceptionSentinel))
					throw DisposableUtilities.NewObjectDisposedException(disposable: this, disposeRequestedException: false);
				else
					return exception;
			}
		}

		protected override void PopulateClone(ICloneContext cloneContext, CloneableDisposable clone) {
			clone.EnsureNotNull(nameof(clone));
			//
			var locClone = (Vh<T>)clone;
			locClone._value = __CloneValue(vlt.Read(ref _value), cloneContext);
		}

		public new Vh<T> Clone()
			=> (Vh<T>)base.Clone();

		public new Vh<T> Clone(ICloneContext cloneContext)
			=> (Vh<T>)base.Clone(cloneContext);

		public static implicit operator Vh<T>(T value)
			=> value == null ? null : new Vh<T>(value);

		public static implicit operator Vh<T>(MuttableVh<T> value)
			=> value is null ? null : new Vh<T>(value.Value);

		public static implicit operator MuttableVh<T>(Vh<T> value)
			=> new MuttableVh<T>(value is null ? default(T) : value.Value);

		public void RemoveValue()
			=> vlt.Write(ref _value, __DefaultValue);

		public TaskWrap<T> Awaitable() {
			try {
				var result = ReadDA(ref _valueAwaitable);
				if (result is null) {
					try {
						result = new TaskWrap<T>(task: Task.FromResult(result: Value));
					}
					catch (ValueGetterException exception) {
						result = new TaskWrap<T>(task: Task.FromException<T>(exception: exception));
					}
					var updateResult = itrlck.UpdateIfNull(location: ref _valueAwaitable, value: result);
					if (ReferenceEquals(result, updateResult)) {
						if (IsDisposeRequested) {
							itrlck.SetNull(location: ref _valueAwaitable, comparand: result);
							EnsureNotDisposeState(considerDisposeRequest: true); // this throws ObjectDisposedException.
						}
					}
					else
						result = updateResult;
				}
				return result;
			}
			catch (Exception exception) {
				return new TaskWrap<T>(task: Task.FromException<T>(exception: exception));
			}
		}

		ITaskWrap<T> IVh<T>.Awaitable()
			=> Awaitable();

		public override string ToString() {
			var exception = itrlck.Get(location: ref _exception);
			if (ReferenceEquals(exception, ValueHolderUtilities.HasExceptionSentinel))
				return $"<Holder contains error>";
			else if (exception is null)
				return _value.FmtStr().G();
			else
				return $"<Holder contains error '{exception.GetType()}'>";
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose)
				_valueCleanup?.Invoke(_value);
			_value = __DefaultValue;
			_valueAwaitable = null;
			itrlck.Update(location: ref _exception, transform: locValue => locValue is null ? null : ValueHolderUtilities.HasExceptionSentinel);
			//
			base.Dispose(explicitDispose);
		}

	}

}