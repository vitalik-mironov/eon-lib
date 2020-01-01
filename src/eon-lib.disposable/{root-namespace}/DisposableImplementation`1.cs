using System;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Eon.Reflection;
using Eon.Threading;

using itrlck = Eon.Threading.InterlockedUtilities;
using vlt = Eon.Threading.VolatileUtilities;

namespace Eon {

	/// <summary>
	/// Представляет общую реализацию выгружаемого объекта.
	/// <para>Тип не предназначен для прямого использования в коде.</para>
	/// <para>Тип используется такими типами как <see cref="Disposable"/>, <see cref="DisposeNotifying"/> и др.</para>
	/// <para>Тип <typeparamref name="TClass"/>:</para>
	/// <para>• Является классом;</para>
	/// <para>• Может иметь метод 'FireBeforeDispose(bool explicitDispose)'.</para>
	/// <para>• Может иметь метод 'FireAfterDisposed(bool explicitDispose)'.</para>
	/// <para>• Может иметь приватный метод 'P_Dispose(bool explicitDispose)' или неприватный 'Dispose(bool explicitDispose)', который реализует логику выгрузки.</para>
	/// </summary>
	/// <typeparam name="TClass">Тип, поддерживающий выгрузку.</typeparam>
	public sealed class DisposableImplementation<TClass>
		:DisposableImplementation
		where TClass : class {

		const int __Disposing_StateFlag = 1;

		const int __Disposed_StateFlag = 2;

		/// <summary>
		/// Этот флаг состояния выгрузки необходим для предотвращения выполнения логики выгрузки одновременно из двух (и более) потоков. 
		/// </summary>
		const int __DisposeLogicCalled_StateFlag = 4;

		const int __DisposeRequested_StateFlag = 8;

		const int __DisposingOrDisposed_StateFlag = __Disposing_StateFlag | __Disposed_StateFlag;

		#region Static members

		static readonly Type __ObjectType = typeof(object);

		static readonly Type __BoolType = typeof(bool);

		static readonly Action<TClass, bool> __FireBeforeDisposeDelegate;

		static readonly Action<TClass, bool> __FireAfterDisposedDelegate;

		static readonly Action<TClass, bool> __DisposeLogicDelegate;

		// TODO: Put strings into the resources.
		//
		static DisposableImplementation() {
			var typeOfClass = typeof(TClass);
			if (typeOfClass.IsGenericType() && typeOfClass.GetGenericTypeDefinition() == typeof(DisposableImplementation<>))
				throw
					new EonException(message: $"Аргумент '{nameof(TClass)}' данного универсального типа '{typeof(DisposableImplementation<>).FullName}' не может быть '{typeOfClass.FullName}'.");
			// TODO: Возможно, имеет смысл заюзать атрибуты для определения методов.
			//
			var fireBeforeDisposeImplMethodInfo = P_FindImplementationMethod(typeOfClass, "FireBeforeDispose");
			var notifyAfterDisposedImplMethodInfo = P_FindImplementationMethod(typeOfClass, "FireAfterDisposed");
			var disposeLogicImplMethodInfo =
				P_FindImplementationMethod(typeOfClass, nameof(P_Dispose), privateFlag: true)
				?? P_FindImplementationMethod(typeOfClass, nameof(IDisposable.Dispose));
			var disposableParameterExpr = Expression.Parameter(typeOfClass);
			var explicitDisposeParameterExpr = Expression.Parameter(__BoolType);
			if (fireBeforeDisposeImplMethodInfo == null)
				__FireBeforeDisposeDelegate = null;
			else
				__FireBeforeDisposeDelegate = Expression.Lambda<Action<TClass, bool>>(Expression.Call(disposableParameterExpr, fireBeforeDisposeImplMethodInfo, explicitDisposeParameterExpr), disposableParameterExpr, explicitDisposeParameterExpr).Compile();
			if (notifyAfterDisposedImplMethodInfo == null)
				__FireAfterDisposedDelegate = null;
			else
				__FireAfterDisposedDelegate = Expression.Lambda<Action<TClass, bool>>(Expression.Call(disposableParameterExpr, notifyAfterDisposedImplMethodInfo, explicitDisposeParameterExpr), disposableParameterExpr, explicitDisposeParameterExpr).Compile();
			if (disposeLogicImplMethodInfo == null)
				__DisposeLogicDelegate = (i, e) => { };
			else
				__DisposeLogicDelegate = Expression.Lambda<Action<TClass, bool>>(Expression.Call(disposableParameterExpr, disposeLogicImplMethodInfo, explicitDisposeParameterExpr), disposableParameterExpr, explicitDisposeParameterExpr).Compile();
		}

		static MethodInfo P_FindImplementationMethod(Type implementor, string methodName, bool privateFlag = false) {
			implementor.EnsureNotNull(nameof(implementor));
			methodName
				.Arg(nameof(methodName))
				.EnsureNotNullOrEmpty();
			//
			var baseType = implementor;
			var foundMethod = default(MethodInfo);
			for (; foundMethod == null && baseType != __ObjectType;) {
				foundMethod = (from method in (from method in baseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
																			 where method.IsPrivate == privateFlag
																			 where !method.IsGenericMethod
																			 where string.Equals(method.Name, methodName, StringComparison.Ordinal)
																			 select method)
											 let methodParameters = method.GetParameters()
											 where methodParameters.Length == 1 && methodParameters[ 0 ].ParameterType == __BoolType
											 select method)
											 .SingleOrDefault();
				baseType = baseType.BaseType();
			}
			return foundMethod;
		}

		#endregion

		int _disposeStateFlags = 0;

		readonly TClass _instance;

		public DisposableImplementation(TClass disposable)
#if DEBUG
			: base(disposable)
#else
			: base()
#endif
			{
			disposable.EnsureNotNull(nameof(disposable));
			//
			_instance = disposable;
		}

		public bool Disposing
			=> (vlt.Read(ref _disposeStateFlags) & __DisposingOrDisposed_StateFlag) == __Disposing_StateFlag;

		public bool IsDisposed
			=> (vlt.Read(ref _disposeStateFlags) & __DisposingOrDisposed_StateFlag) == __DisposingOrDisposed_StateFlag;

		public bool IsDisposeRequested
			=> (vlt.Read(ref _disposeStateFlags) & __DisposeRequested_StateFlag) == __DisposeRequested_StateFlag;

		public void SetDisposeRequested()
			=> itrlck.Or(ref _disposeStateFlags, __DisposeRequested_StateFlag);

		/// <summary>
		/// Выполняет проверку на предмет состояния выгрузки объекта.
		/// <para>Если для объекта была начата выгрузка или он уже выгружен (см. <seealso cref="Disposing"/> и <seealso cref="IsDisposed"/>), то вызывается исключение <seealso cref="ObjectDisposedException"/>.</para>
		/// </summary>
		/// <exception cref="ObjectDisposedException">Когда данный объект либо выгружается, либо уже выгружен.</exception>
		public void EnsureNotDisposeState() {
			if (Disposing || IsDisposed)
				throw DisposableUtilities.NewObjectDisposedException(disposable: _instance, disposeRequestedException: false);
		}

		/// <summary>
		/// Выполняет проверку на предмет состояния выгрузки данного объекта или наличия запроса на его выгрузку (см. <seealso cref="IsDisposeRequested"/>).
		/// <para>Если для объекта была начата выгрузка или он уже выгружен (см. <seealso cref="Disposing"/> и <seealso cref="IsDisposed"/>), то вызывается исключение <seealso cref="ObjectDisposedException"/>.</para>
		/// <para>Если <paramref name="considerDisposeRequest"/> == true и для объекта была запрошена выгрузка (см. <seealso cref="IsDisposeRequested"/>), то вызывается исключение <seealso cref="ObjectDisposedException"/>.</para>
		/// </summary>
		/// <param name="considerDisposeRequest">
		/// Признак, указывающий проверять ли наличие запроса на выгрузку.
		/// </param>
		public void EnsureNotDisposeState(bool considerDisposeRequest) {
			if (Disposing || IsDisposed)
				throw DisposableUtilities.NewObjectDisposedException(disposable: _instance, disposeRequestedException: false);
			else if (considerDisposeRequest && IsDisposeRequested)
				throw DisposableUtilities.NewObjectDisposedException(disposable: _instance, disposeRequestedException: true);
		}

		void P_Dispose(bool explicitDispose) {
			itrlck.Or(ref _disposeStateFlags, __DisposeRequested_StateFlag);
			//
			if ((vlt.Read(ref _disposeStateFlags) & __DisposingOrDisposed_StateFlag) == 0)
				__FireBeforeDisposeDelegate?.Invoke(_instance, explicitDispose);
			// После выполнение FireBeforeDispose, устанавливается флаг состояния начала выгрузки.
			//
			var disposeState = itrlck.Or(ref _disposeStateFlags, __Disposing_StateFlag);
			if ((disposeState & __Disposed_StateFlag) == 0) {
				// Еще не выгружен.
				//
				var exceptionThrown = true;
				disposeState = __DisposeLogicCalled_StateFlag;
				try {
					disposeState = itrlck.Or(ref _disposeStateFlags, __DisposeLogicCalled_StateFlag);
					if ((disposeState & __DisposeLogicCalled_StateFlag) == 0)
						__DisposeLogicDelegate(_instance, explicitDispose);
					exceptionThrown = false;
				}
				finally {
					if ((disposeState & __DisposeLogicCalled_StateFlag) == 0) {
						// В данном вызове был в '_disposeStateFlags' установлен флаг вызова метода '__DisposeLogicDelegate' — логика выгрузки.
						//
						if (exceptionThrown) {
							// Возникло исключение при вызове '__DisposeLogicDelegate' (логика выгрузки).
							// Восстанивливается состояние '_disposeStateFlags' для возможности последующего вызова логики выгрузки.
							//
							Interlocked.CompareExchange(location1: ref _disposeStateFlags, value: disposeState, comparand: disposeState | __DisposeLogicCalled_StateFlag);
						}
						else {
							// Делегат выгрузки успешно выполнен.
							//
							Interlocked
								.CompareExchange(location1: ref _disposeStateFlags, value: disposeState | __Disposed_StateFlag, comparand: disposeState | __DisposeLogicCalled_StateFlag);
							// Вызов делегата логики уведомления о заверешении выгрузки.
							//
							__FireAfterDisposedDelegate?.Invoke(_instance, explicitDispose);
						}
					}
				}
			}
		}

		private protected override void DoDispose(bool explicitDispose)
			=> P_Dispose(explicitDispose);

		public T ReadDA<T>(ref T location) {
			var result = vlt.Read(ref location);
			EnsureNotDisposeState();
			return result;
		}

		public T ReadDA<T>(ref T location, bool considerDisposeRequest) {
			var result = vlt.Read(ref location);
			EnsureNotDisposeState(considerDisposeRequest: considerDisposeRequest);
			return result;
		}

		public bool TryReadDA<T>(ref T location, bool considerDisposeRequest, out T result) {
			var resultBuffer = vlt.Read(ref location);
			if ((considerDisposeRequest && IsDisposeRequested) || Disposing || IsDisposed) {
				result = default;
				return false;
			}
			else {
				result = resultBuffer;
				return true;
			}
		}

		public T WriteDA<T>(ref T location, T value, T comparand)
			where T : class
			=> DisposableUtilities.WriteDA(ensureAllowedDisposeState: EnsureNotDisposeState, isDisposed: () => IsDisposed, location: ref location, value: value, comparand: comparand);

		public void WriteDA<T>(ref T location, T value, T comparand, out bool result)
			where T : class
			=> DisposableUtilities.WriteDA(ensureAllowedDisposeState: EnsureNotDisposeState, isDisposed: () => IsDisposed, location: ref location, value: value, comparand: comparand, result: out result);

		public T WriteDA<T>(ref T location, T value)
			where T : class
			=> DisposableUtilities.WriteDA(EnsureNotDisposeState, () => IsDisposed, ref location, value);

		public bool UpdDABool<T>(ref T location, Transform<T> transform, Action<T> cleanup = default)
			where T : class {
			UpdDA(location: ref location, transform: transform, result: out var locResult, cleanup: cleanup);
			return locResult.IsUpdated;
		}

		public bool UpdDABool<T>(ref T location, Transform<T> transform, out T result)
			where T : class {
			transform.EnsureNotNull(nameof(transform));
			//
			UpdDA(location: ref location, transform: transform, result: out var locResult, cleanup: null);
			result = locResult.Current;
			return locResult.IsUpdated;
		}

		public bool UpdDABool<T>(ref T location, Transform<T> transform, out T result, Action<T> cleanup = null)
			where T : class {
			UpdDA(location: ref location, transform: transform, result: out var locResult, cleanup: cleanup);
			result = locResult.Current;
			return locResult.IsUpdated;
		}

		/// <summary>
		/// Метод заменяет оригинальное значение в <paramref name="location"/> новым значением, полученным от <paramref name="transform"/>.
		/// <para>В зависимости от наличия изменения в <paramref name="location"/> после вызова функции <paramref name="transform"/> эта функция будет вызвана повторно для актуального значения из <paramref name="location"/>.</para>
		/// <para>Метод выполняет проверку выгрузки данного объекта при считывании из <paramref name="location"/>, а также при записи в <paramref name="location"/>, учитывая <seealso cref="IsDisposeRequested"/>.</para>
		/// </summary>
		/// <typeparam name="T">Тип значения переменной.</typeparam>
		/// <param name="location">Адрес переменной, в которой производится обновление.</param>
		/// <param name="transform">
		/// Функция трансформации оригинального значения в новое.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="cleanup">
		/// Метод очистки результата функции <paramref name="transform"/>.
		/// <para>Вызывается только в тех случаях, когда результат функции не востребован:</para>
		/// <para>	• начата выгрузка объекта и результат НЕ записан в <paramref name="location"/>;</para>
		/// <para>	• после вызова функции <paramref name="transform"/> значение в <paramref name="location"/> изменилось и функция <paramref name="transform"/> должна быть вызвана повторно для актуального значения из <paramref name="location"/>.</para>
		/// </param>
		/// <param name="result">
		/// Переменная, в которую будет помещен результат операции обновления.
		/// </param>
		/// <returns>
		/// Значение <see cref="IInterlockedUpdateResult{T}"/>.
		/// </returns>
		public void UpdDA<T>(ref T location, Transform<T> transform, out UpdateResult<T> result, Action<T> cleanup = default)
			where T : class {
			transform.EnsureNotNull(nameof(transform));
			//
			T originalValue;
			T exchangeResult;
			for (; ; ) {
				originalValue = ReadDA(location: ref location, considerDisposeRequest: true);
				var newValue = transform(current: originalValue);
				if (ReferenceEquals(newValue, originalValue)) {
					// Проверка состояния выгрузки.
					//
					EnsureNotDisposeState(considerDisposeRequest: true);
					// Новое значение идентично существующему.
					//
					result = new UpdateResult<T>(current: originalValue, original: originalValue);
					break;
				}
				else {
					// Проверка состояния выгрузки.
					//
					try {
						EnsureNotDisposeState(considerDisposeRequest: true);
					}
					catch (ObjectDisposedException firstException) {
						// Поскольку "новое" значение еще не записано в location.
						//
						try {
							cleanup?.Invoke(newValue);
						}
						catch (Exception secondException) {
							throw new AggregateException(firstException, secondException);
						}
						throw;
					}
					// Обновление значения.
					//
					exchangeResult = Interlocked.CompareExchange(location1: ref location, value: newValue, comparand: originalValue);
					if (ReferenceEquals(originalValue, exchangeResult)) {
						// Значение обновлено.
						// Проверка состояния выгрузки.
						//
						try {
							EnsureNotDisposeState(considerDisposeRequest: true);
						}
						catch (ObjectDisposedException) {
							// Объект в состоянии выгрузки (выгружается или уже выгружен).
							//
							if (IsDisposed)
								// Поскольку объект выгружен...
								//
								Interlocked.Exchange(location1: ref location, value: null);
							throw;
						}
						//
						result = new UpdateResult<T>(current: newValue, original: originalValue);
						break;
					}
					else {
						// Значение не обновлено.
						// Очистка результата трансформации.
						//
						cleanup?.Invoke(newValue);
					}
				}
			}
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public UpdateResult<T> UpdDA<T, TState>(ref T location, ref TState state, TransformStateful<T, TState> transform, TransformCleanupStateful<T, TState> cleanup = default)
			where T : class {
			transform.EnsureNotNull(nameof(transform));
			//
			T originalValue;
			T exchangeResult;
			for (; ; ) {
				originalValue = ReadDA(location: ref location, considerDisposeRequest: true);
				var newValue = transform(current: originalValue, state: ref state);
				if (ReferenceEquals(objA: newValue, objB: originalValue)) {
					// Проверка состояния выгрузки.
					//
					EnsureNotDisposeState(considerDisposeRequest: true);
					// Новое значение идентично существующему.
					//
					return new UpdateResult<T>(current: originalValue, original: originalValue);
				}
				else {
					// Проверка состояния выгрузки.
					//
					try {
						EnsureNotDisposeState(considerDisposeRequest: true);
					}
					catch (ObjectDisposedException exception) {
						// Поскольку "новое" значение еще не записано в location.
						//
						try {
							cleanup?.Invoke(value: newValue, state: ref state);
						}
						catch (Exception secondException) {
							throw new AggregateException(exception, secondException);
						}
						throw;
					}
					// Обновление значения.
					//
					exchangeResult = Interlocked.CompareExchange(location1: ref location, value: newValue, comparand: originalValue);
					if (ReferenceEquals(objA: originalValue, objB: exchangeResult)) {
						// Значение обновлено.
						// Проверка состояния выгрузки.
						//
						try {
							EnsureNotDisposeState(considerDisposeRequest: true);
						}
						catch (ObjectDisposedException) {
							// Объект в состоянии выгрузки (выгружается или уже выгружен).
							//
							if (IsDisposed)
								// Поскольку объект выгружен...
								//
								Interlocked.Exchange(location1: ref location, value: null);
							throw;
						}
						//
						return new UpdateResult<T>(current: newValue, original: originalValue);
					}
					else {
						// Значение не обновлено.
						// Очистка результата трансформации.
						//
						cleanup?.Invoke(value: newValue, state: ref state);
					}
				}
			}
		}

		public bool UpdDABool<T>(ref T location, Transform2<T> transform)
			where T : class
			=> UpdDABool(location: ref location, transform: transform, current: out _);

		public bool UpdDABool<T>(ref T location, Transform2<T> transform, out T current)
			where T : class {
			UpdDA(location: ref location, transform: transform, result: out var locResult);
			current = locResult.Current;
			return locResult.IsUpdated;
		}

		public void UpdDA<T>(ref T location, Transform2<T> transform, out UpdateResult<T> result)
			where T : class {
			transform.EnsureNotNull(nameof(transform));
			//
			T originalValue;
			T exchangeResult;
			var previousTransformResult = default(T);
			for (; ; ) {
				originalValue = ReadDA(location: ref location, considerDisposeRequest: true);
				var newValue = transform(current: originalValue, previousTransformResult: previousTransformResult);
				// Проверка состояния выгрузки.
				//
				EnsureNotDisposeState(considerDisposeRequest: true);
				if (ReferenceEquals(newValue, originalValue)) {
					// Новое значение идентично существующему.
					//
					result = new UpdateResult<T>(current: originalValue, original: originalValue);
					break;
				}
				else {
					// Обновление значения.
					//
					exchangeResult = Interlocked.CompareExchange(location1: ref location, value: newValue, comparand: originalValue);
					if (ReferenceEquals(originalValue, exchangeResult)) {
						// Значение обновлено.
						// Проверка состояния выгрузки.
						//
						try {
							EnsureNotDisposeState(considerDisposeRequest: true);
						}
						catch (ObjectDisposedException) {
							// Объект в состоянии выгрузки (выгружается или уже выгружен).
							//
							if (IsDisposed)
								// Поскольку объект выгружен...
								//
								Interlocked.Exchange(ref location, null);
							throw;
						}
						//
						result = new UpdateResult<T>(current: newValue, original: originalValue);
						break;
					}
				}
				previousTransformResult = newValue;
			}
		}

		public (UpdateResult<ImmutableDictionary<TKey, TValue>> updateResult, TValue currentValue) UpdDA<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TValue> factory, string locationName = default, Action<TKey, TValue> cleanup = default) {
			key.EnsureNotNull(nameof(key));
			factory.EnsureNotNull(nameof(factory));
			//
			var state = (existing: default(TValue), factory, factoried: default(TValue), factoryCall: false, key);
			var updateResult =
				UpdDA(
					location: ref location,
					state: ref state,
					transform:
						(ImmutableDictionary<TKey, TValue> locCurrent, ref (TValue existing, Func<TKey, TValue> factory, TValue factoried, bool factoryCall, TKey key) locState) => {
							if (locCurrent is null) {
								EnsureNotDisposeState(considerDisposeRequest: true);
								throw ExceptionUtilities.NewNullReferenceException(varName: string.IsNullOrEmpty(locationName) ? nameof(location) : locationName, component: _instance);
							}
							else if (locCurrent.TryGetValue(key: locState.key, value: out var locExistingValue)) {
								locState = (existing: locExistingValue, locState.factory, locState.factoried, locState.factoryCall, locState.key);
								return locCurrent;
							}
							else if (!locState.factoryCall) {
								var locFactoried = locState.factory(arg: locState.key);
								locState = (existing: locFactoried, locState.factory, factoried: locFactoried, factoryCall: true, locState.key);
							}
							return locCurrent.Add(key: locState.key, value: locState.factoried);
						});
			if (!updateResult.IsUpdated && state.factoryCall && !(cleanup is null))
				cleanup(arg1: key, arg2: state.factoried);
			return (updateResult, currentValue: state.existing);
		}

		// TODO: Put strings into the resources.
		//
		public T InitDA<T>(ref T location, OutParamFunc<T> factory, Action<T> cleanup = default)
			where T : class {
			factory.EnsureNotNull(nameof(factory));
			//
			var existingValue = ReadDA(location: ref location);
			if (existingValue is null) {
				factory(param: out var newValue);
				if (newValue is null)
					throw new EonException(message: $"Factory method has returned invalid result '{FormatStringUtilitiesCoreL0.GetNullValueText()}'.{Environment.NewLine}\tMethod:{factory.FmtStr().GNLI2()}");
				var exchange = WriteDA(location: ref location, value: newValue, comparand: null);
				if (exchange is null)
					return newValue;
				else if (cleanup is null)
					return exchange;
				else {
					cleanup(obj: newValue);
					return exchange;
				}
			}
			return existingValue;
		}

		// TODO: Put strings into the resources.
		//
		public T InitDA<T>(ref T location, Func<T> factory, Action<T> cleanup = default)
			where T : class {
			factory.EnsureNotNull(nameof(factory));
			//
			var existingValue = ReadDA(location: ref location);
			if (existingValue is null) {
				var newValue = factory();
				if (newValue is null)
					throw new EonException(message: $"Factory method has returned invalid result '{FormatStringUtilitiesCoreL0.GetNullValueText()}'.{Environment.NewLine}\tMethod:{factory.FmtStr().GNLI2()}");
				var exchange = WriteDA(location: ref location, value: newValue, comparand: null);
				if (exchange is null)
					return newValue;
				else if (cleanup is null)
					return exchange;
				else {
					cleanup(obj: newValue);
					return exchange;
				}
			}
			return existingValue;
		}

		// TODO: Put strings into the resources.
		//
		public T InitDA<T>(ref IVh<T> location, OutParamFunc<IVh<T>> factory)
			where T : class {
			factory.EnsureNotNull(nameof(factory));
			//
			var existingHolder = ReadDA(location: ref location);
			if (existingHolder is null) {
				IVh<T> newHolder;
				try {
					factory(param: out newHolder);
					if (newHolder is null || (!newHolder.HasException && newHolder.ValueDisposeTolerant is null && !newHolder.IsDisposeRequested))
						throw new EonException(message: $"Factory method has returned invalid result '{FormatStringUtilitiesCoreL0.GetNullValueText()}'.{Environment.NewLine}\tMethod:{factory.FmtStr().GNLI2()}");
				}
				catch (Exception exception) {
					newHolder = new Vh<T>(exception: exception);
				}
				ExceptionDispatchInfo newHolderException;
				var exchange = WriteDA(location: ref location, value: newHolder, comparand: null);
				if (exchange is null)
					return newHolder.Value;
				else if ((newHolderException = newHolder.Exception) is null) {
					newHolder.Dispose();
					return exchange.Value;
				}
				else {
					newHolder.Dispose();
					newHolderException.Throw();
					return exchange.Value;
				}
			}
			return existingHolder.Value;
		}

		// TODO: Put strings into the resources.
		//
		public T InitDA<T>(ref IVh<T> location, Func<IVh<T>> factory)
			where T : class {
			factory.EnsureNotNull(nameof(factory));
			//
			var existingHolder = ReadDA(location: ref location);
			if (existingHolder is null) {
				IVh<T> newHolder;
				try {
					newHolder = factory();
					if (newHolder is null || (!newHolder.HasException && newHolder.ValueDisposeTolerant is null && !newHolder.IsDisposeRequested))
						throw new EonException(message: $"Factory method has returned invalid result '{FormatStringUtilitiesCoreL0.GetNullValueText()}'.{Environment.NewLine}\tMethod:{factory.FmtStr().GNLI2()}");
				}
				catch (Exception exception) {
					newHolder = new Vh<T>(exception: exception);
				}
				ExceptionDispatchInfo newHolderException;
				var exchange = WriteDA(location: ref location, value: newHolder, comparand: null);
				if (exchange is null)
					return newHolder.Value;
				else if ((newHolderException = newHolder.Exception) is null) {
					newHolder.Dispose();
					return exchange.Value;
				}
				else {
					newHolder.Dispose();
					newHolderException.Throw();
					return exchange.Value;
				}
			}
			return existingHolder.Value;
		}

		// TODO: Put strings into the resources.
		//
		public T InitDA<T>(ref Vh<T> location, OutParamFunc<Vh<T>> factory)
			where T : class {
			factory.EnsureNotNull(nameof(factory));
			//
			var existingHolder = ReadDA(location: ref location);
			if (existingHolder is null) {
				Vh<T> newHolder;
				try {
					factory(param: out newHolder);
					if (newHolder is null || (!newHolder.HasException && newHolder.ValueDisposeTolerant is null && !newHolder.IsDisposeRequested))
						throw new EonException(message: $"Factory method has returned invalid result '{FormatStringUtilitiesCoreL0.GetNullValueText()}'.{Environment.NewLine}\tMethod:{factory.FmtStr().GNLI2()}");
				}
				catch (Exception exception) {
					newHolder = new Vh<T>(exception: exception);
				}
				ExceptionDispatchInfo newHolderException;
				var exchange = WriteDA(location: ref location, value: newHolder, comparand: null);
				if (exchange is null)
					return newHolder.Value;
				else if ((newHolderException = newHolder.Exception) is null) {
					newHolder.Dispose();
					return exchange.Value;
				}
				else {
					newHolder.Dispose();
					newHolderException.Throw();
					return exchange.Value;
				}
			}
			return existingHolder.Value;
		}

		// TODO: Put strings into the resources.
		//
		public T InitDA<T>(ref Vh<T> location, Func<Vh<T>> factory)
			where T : class {
			factory.EnsureNotNull(nameof(factory));
			//
			var existingHolder = ReadDA(location: ref location);
			if (existingHolder is null) {
				Vh<T> newHolder;
				try {
					newHolder = factory();
					if (newHolder is null || (!newHolder.HasException && newHolder.ValueDisposeTolerant is null && !newHolder.IsDisposeRequested))
						throw new EonException(message: $"Factory method has returned invalid result '{FormatStringUtilitiesCoreL0.GetNullValueText()}'.{Environment.NewLine}\tMethod:{factory.FmtStr().GNLI2()}");
				}
				catch (Exception exception) {
					newHolder = new Vh<T>(exception: exception);
				}
				ExceptionDispatchInfo newHolderException;
				var exchange = WriteDA(location: ref location, value: newHolder, comparand: null);
				if (exchange is null)
					return newHolder.Value;
				else if ((newHolderException = newHolder.Exception) is null) {
					newHolder.Dispose();
					return exchange.Value;
				}
				else {
					newHolder.Dispose();
					newHolderException.Throw();
					return exchange.Value;
				}
			}
			return existingHolder.Value;
		}

		// TODO: Put strings into the resources.
		//
		public T InitDA<T>(ref ValueHolderClass<T> location, OutParamFunc<T> factory, Action<T> cleanup = default)
			where T : class {
			factory.EnsureNotNull(nameof(factory));
			//
			var existingHolder = ReadDA(location: ref location);
			if (existingHolder is null) {
				ValueHolderClass<T> newHolder;
				try {
					factory(param: out var factoriedValue);
					if (factoriedValue is null)
						throw new EonException(message: $"Factory method has returned invalid result '{FormatStringUtilitiesCoreL0.GetNullValueText()}'.{Environment.NewLine}\tMethod:{factory.FmtStr().GNLI2()}");
					newHolder = new ValueHolderClass<T>(value: factoriedValue);
				}
				catch (Exception exception) {
					newHolder = new ValueHolderClass<T>(exception: exception);
				}
				var exchange = WriteDA(location: ref location, value: newHolder, comparand: null);
				if (exchange is null)
					return newHolder.Value;
				else if (newHolder.Exception is null) {
					cleanup?.Invoke(obj: newHolder.Value);
					return exchange.Value;
				}
				else {
					newHolder.Exception.Throw();
					return exchange.Value;
				}
			}
			return existingHolder.Value;
		}

		// TODO: Put strings into the resources.
		//
		public T InitDA<T>(ref ValueHolderClass<T> location, Func<T> factory, Action<T> cleanup = default)
			where T : class {
			factory.EnsureNotNull(nameof(factory));
			//
			var existingHolder = ReadDA(location: ref location);
			if (existingHolder is null) {
				ValueHolderClass<T> newHolder;
				try {
					var factoriedValue = factory();
					if (factoriedValue is null)
						throw new EonException(message: $"Factory method has returned invalid result '{FormatStringUtilitiesCoreL0.GetNullValueText()}'.{Environment.NewLine}\tMethod:{factory.FmtStr().GNLI2()}");
					newHolder = new ValueHolderClass<T>(value: factoriedValue);
				}
				catch (Exception exception) {
					newHolder = new ValueHolderClass<T>(exception: exception);
				}
				var exchange = WriteDA(location: ref location, value: newHolder, comparand: null);
				if (exchange is null)
					return newHolder.Value;
				else if (newHolder.Exception is null) {
					cleanup?.Invoke(obj: newHolder.Value);
					return exchange.Value;
				}
				else {
					newHolder.Exception.Throw();
					return exchange.Value;
				}
			}
			return existingHolder.Value;
		}

		public T InitDANullable<T>(ref ValueHolderClass<T> location, OutParamFunc<T> factory, Action<T> cleanup = default) {
			factory.EnsureNotNull(nameof(factory));
			//
			var existing = ReadDA(location: ref location);
			if (existing is null) {
				ValueHolderClass<T> created;
				try {
					factory(param: out var createdValue);
					created = new ValueHolderClass<T>(value: createdValue);
				}
				catch (Exception exception) {
					created = new ValueHolderClass<T>(exception: exception);
				}
				var exchange = WriteDA(location: ref location, value: created, comparand: null);
				if (exchange is null)
					return created.Value;
				else if (created.Exception is null) {
					if (cleanup is null || created.Value == default)
						return exchange.Value;
					else {
						cleanup(obj: created.Value);
						return exchange.Value;
					}
				}
				else {
					created.Exception.Throw();
					return exchange.Value;
				}
			}
			return existing.Value;
		}

		public T InitDANullable<T>(ref ValueHolderClass<T> location, Func<T> factory, Action<T> cleanup = default) {
			factory.EnsureNotNull(nameof(factory));
			//
			var existing = ReadDA(location: ref location);
			if (existing is null) {
				ValueHolderClass<T> created;
				try {
					created = new ValueHolderClass<T>(value: factory());
				}
				catch (Exception exception) {
					created = new ValueHolderClass<T>(exception: exception);
				}
				var exchange = WriteDA(location: ref location, value: created, comparand: null);
				if (exchange is null)
					return created.Value;
				else if (created.Exception is null) {
					if (cleanup is null || created.Value == default)
						return exchange.Value;
					else {
						cleanup(obj: created.Value);
						return exchange.Value;
					}
				}
				else {
					created.Exception.Throw();
					return exchange.Value;
				}
			}
			return existing.Value;
		}

		#region Add/Remove event handler

		#region Move to 'eon.ui' package.

		//public void AddEventHandler(ref SynchronizationContextBoundEventHandler<EventHandler, EventArgs> location, EventHandler eventHandler) {
		//	if (eventHandler != null)
		//		for (; ; ) {
		//			var current = ReadDA(ref location, considerDisposeRequest: true);
		//			if (ReferenceEquals(current, WriteDA(location: ref location, value: current + eventHandler, comparand: current)))
		//				break;
		//		}
		//}

		//public void AddEventHandler<TEventArgs>(ref SynchronizationContextBoundEventHandler<EventHandler<TEventArgs>, TEventArgs> location, EventHandler<TEventArgs> eventHandler)
		//	where TEventArgs : EventArgs {
		//	if (eventHandler != null)
		//		for (; ; ) {
		//			var original = ReadDA(ref location, considerDisposeRequest: true);
		//			if (ReferenceEquals(original, WriteDA(ref location, original + eventHandler, original)))
		//				break;
		//		}
		//}

		#endregion

		public void AddEventHandler<TEventArgs>(ref EventHandler<TEventArgs> location, EventHandler<TEventArgs> eventHandler)
			where TEventArgs : EventArgs {
			if (!(eventHandler is null))
				for (; ; ) {
					var original = ReadDA(ref location, considerDisposeRequest: true);
					if (ReferenceEquals(original, WriteDA(location: ref location, value: original + eventHandler, comparand: original)))
						break;
				}
		}

		public void AddEventHandler(ref EventHandler location, EventHandler eventHandler) {
			if (eventHandler != null)
				for (; ; ) {
					var original = ReadDA(ref location, considerDisposeRequest: true);
					if (ReferenceEquals(original, WriteDA(ref location, original + eventHandler, original)))
						break;
				}
		}

		#region Move to 'eon.ui' package.

		//public void RemoveEventHandler(ref SynchronizationContextBoundEventHandler<EventHandler, EventArgs> location, EventHandler eventHandler) {
		//	if (eventHandler != null)
		//		for (; ; ) {
		//			var current = itrlck.Get(ref location);
		//			if (itrlck.Update(location: ref location, value: current - eventHandler, comparand: current))
		//				break;
		//		}
		//}

		//public void RemoveEventHandler<TEventArgs>(ref SynchronizationContextBoundEventHandler<EventHandler<TEventArgs>, TEventArgs> location, EventHandler<TEventArgs> eventHandler)
		//	where TEventArgs : EventArgs {
		//	if (eventHandler != null)
		//		for (; ; ) {
		//			var current = itrlck.Get(ref location);
		//			if (itrlck.Update(location: ref location, value: current - eventHandler, comparand: current))
		//				break;
		//		}
		//}

		#endregion

		public void RemoveEventHandler<TEventArgs>(ref EventHandler<TEventArgs> location, EventHandler<TEventArgs> eventHandler)
			where TEventArgs : EventArgs {
			if (eventHandler != null)
				for (; ; ) {
					var current = itrlck.Get(ref location);
					if (itrlck.UpdateBool(location: ref location, value: current - eventHandler, comparand: current))
						break;
				}
		}

		public void RemoveEventHandler(ref EventHandler location, EventHandler eventHandler) {
			if (eventHandler != null)
				for (; ; ) {
					var current = itrlck.Get(ref location);
					if (itrlck.UpdateBool(location: ref location, value: current - eventHandler, comparand: current))
						break;
				}
		}

		#endregion

	}

}