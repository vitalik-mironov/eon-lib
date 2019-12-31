using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using Eon.Collections;
using Eon.Threading;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon {

	/// <summary>
	/// Provides lazy initialization for disposable component.
	/// </summary>
	/// <typeparam name="T">Component type.</typeparam>
	public sealed class DisposableLazy<T>
		:Disposable {

		#region Nested types

		sealed class P_ValueCreationCallCounterState {

			public int Counter;

			internal P_ValueCreationCallCounterState() {
				Counter = __MaxAllowedValueCreationCallCount;
			}

		}

		#endregion

		#region Static members

		/// <summary>
		/// Значение: '8'.
		/// </summary>
		static readonly int __MaxAllowedValueCreationCallCount = 8;

		static readonly Dictionary<object, P_ValueCreationCallCounterState> __ValueCreationCallCounters;

		static readonly PrimitiveSpinLock __ValueCreationCallCountersSpinLock;

		static DisposableLazy() {
			__ValueCreationCallCounters = new Dictionary<object, P_ValueCreationCallCounterState>(comparer: ReferenceEqualityComparer<object>.Instance);
			__ValueCreationCallCountersSpinLock = new PrimitiveSpinLock();
		}

		static P_ValueCreationCallCounterState P_GetValueCreationCallCounterState(object identity) {
			identity.EnsureNotNull(nameof(identity));
			//
			return __ValueCreationCallCounters.GetOrAdd(spinLock: __ValueCreationCallCountersSpinLock, key: identity, factory: locKey => new P_ValueCreationCallCounterState());
		}

		#endregion

		/// <summary>
		/// Используется только при отложенном создании значения (см. <seealso cref="P_GetOrCreateValue"/>).
		/// <para>Указывает, должно ли значение, возвращенное фабрикой <seealso cref="_factory"/>, выгружаться.</para>
		/// </summary>
		readonly bool _ownsValue;

		/// <summary>
		/// Используется только при отложенном создании значения (см. <seealso cref="P_GetOrCreateValue"/>). В остальных кейсах инициализируется значением null.
		/// </summary>
		readonly object _identity;

		/// <summary>
		/// Используется только при отложенном создании значения (см. <seealso cref="P_GetOrCreateValue"/>). В остальных кейсах инициализируется значением null.
		/// </summary>
		Func<T> _factory;

		IVh<T> _valueHolder;

		public DisposableLazy(Func<T> factory, bool ownsValue) {
			factory.EnsureNotNull(nameof(factory));
			//
			_identity = new object();
			_factory = factory;
			_ownsValue = ownsValue;
			_valueHolder = null;
		}

		public DisposableLazy(Func<T> factory)
			: this(factory: factory, ownsValue: true) { }

		public DisposableLazy(IVh<T> valueStore) {
			valueStore.EnsureNotNull(nameof(valueStore));
			//
			_identity = null;
			_factory = null;
			_valueHolder = valueStore;
		}

		public DisposableLazy(T value) {
			_identity = null;
			_factory = null;
			_valueHolder = value.ToValueHolder(ownsValue: false);
		}

		public DisposableLazy(T value, bool ownsValue) {
			_identity = null;
			_factory = null;
			_valueHolder = value.ToValueHolder(ownsValue: ownsValue);
		}

		public bool IsValueCreated
			=> ReadDA(ref _valueHolder) != null;

		public T Value
			=> P_GetOrCreateValue(disposeTolerant: false).Value;

		public T ValueDisposeTolerant {
			get {
				var valueStore = P_GetOrCreateValue(disposeTolerant: true);
				if (valueStore is null)
					return default;
				else
					return valueStore.ValueDisposeTolerant;
			}
		}

		// TODO: Put strings into the resources.
		//
		IVh<T> P_GetOrCreateValue(bool disposeTolerant) {
			var existingValueHolder = disposeTolerant ? TryReadDA(ref _valueHolder) : ReadDA(ref _valueHolder);
			if (existingValueHolder is null) {
				var identity = _identity;
				var factory = disposeTolerant ? TryReadDA(ref _factory) : ReadDA(ref _factory);
				if (identity is null)
					existingValueHolder = null;
				else {
					var callCounter = P_GetValueCreationCallCounterState(identity: identity);
					var isCallCounterDecremented = false;
					try {
						itrlck.Decrement(location: ref callCounter.Counter, minExclusive: -1, isDecremented: out isCallCounterDecremented);
						if (!isCallCounterDecremented)
							throw
								new EonException(
									message: $"Достигнуто максимально допустимое количество обращений к данному компоненту за получением значения.{Environment.NewLine}Одна из возможных причин может быть в том, что метод, создающий значение (указанный как аргумент конструктора данного компонента), обращается к свойству данного компонента, которое это значение должно возвращать — свойство '{nameof(Value)}', — что в свою очередь вызывает рекурсию.{Environment.NewLine}\tКомпонент:{Environment.NewLine}{this.FmtStr().GI2()}{Environment.NewLine}\tМетод, создающий значение:{Environment.NewLine}{factory.GetMethodInfo().FmtStr().GForMemberInfo().IndentLines2()}");
						//
						var createdValue = default(T);
						var createdValueHolder = default(IVh<T>);
						var caughtException = default(Exception);
						var hasValueCreationError = true;
						try {
							// Создание хранилища значения.
							//
							try {
								createdValue = factory();
								hasValueCreationError = false;
							}
							catch (Exception exception) {
								hasValueCreationError = true;
								createdValueHolder = new Vh<T>(exception: exception);
							}
							if (!hasValueCreationError)
								createdValueHolder = createdValue.ToValueHolder(ownsValue: _ownsValue);
							// Установка хранилища значения.
							//
							try {
								existingValueHolder = itrlck.UpdateIfNull(location: ref _valueHolder, value: createdValueHolder);
								if (IsDisposeRequested) {
									itrlck.SetNullBool(location: ref _valueHolder, comparand: createdValueHolder);
									if (disposeTolerant) {
										createdValueHolder.Dispose();
										existingValueHolder = null;
									}
									else
										throw DisposableUtilities.NewObjectDisposedException(disposable: this, disposeRequestedException: !(Disposing || IsDisposed));
								}
							}
							catch {
								itrlck.SetNullBool(location: ref _valueHolder, comparand: createdValueHolder);
								throw;
							}
						}
						catch (Exception exception) {
							caughtException = exception;
							if (createdValueHolder is null || hasValueCreationError) {
								if (_ownsValue) {
									try {
										createdValue.ToValueHolder(ownsValue: true).Dispose();
									}
									catch (Exception secondException) {
										throw new AggregateException(innerExceptions: new[ ] { exception, secondException });
									}
								}
							}
							else
								createdValueHolder.Dispose(exception);
							throw;
						}
						finally {
							if (caughtException is null && !ReferenceEquals(existingValueHolder, createdValueHolder))
								createdValueHolder?.Dispose();
						}
					}
					finally {
						if (isCallCounterDecremented)
							Interlocked.Increment(location: ref callCounter.Counter);
					}
				}
			}
			return existingValueHolder;
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_valueHolder?.Dispose();
				var identity = _identity;
				if (identity != null)
					__ValueCreationCallCountersSpinLock.Invoke(action: () => __ValueCreationCallCounters.Remove(key: identity));
			}
			_factory = null;
			_valueHolder = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}