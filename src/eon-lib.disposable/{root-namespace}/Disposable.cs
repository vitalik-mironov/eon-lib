using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Threading;

using Eon.Collections;
using Eon.Runtime.Serialization;
using Eon.Threading;

namespace Eon {

	[DataContract]
	public class Disposable
		:IOxyDisposable {

		private protected readonly bool NopDispose;

		DisposableImplementation<Disposable> _disposableImplementation;

		private protected Disposable(bool nopDispose) {
			NopDispose = nopDispose;
			_disposableImplementation = new DisposableImplementation<Disposable>(disposable: this);
		}

		public Disposable() {
			NopDispose = false;
			_disposableImplementation = new DisposableImplementation<Disposable>(disposable: this);
		}

		protected Disposable(SerializationContext ctx) {
			NopDispose = false;
			_disposableImplementation = new DisposableImplementation<Disposable>(disposable: this);
		}

		/// <summary>
		/// Возвращает признак, указывающий был ли хотя бы один запрос выгрузки данного объекта (вызов метода <seealso cref="IDisposable.Dispose"/>).
		/// <para>Свойство не подвержено влиянию состояния выгрузки объекта.</para>
		/// <para>Следует учитывать, что после того, как поступил запрос выгрузки, модификация состояния объекта (такими методами как <seealso cref="Disposable.WriteDA{T}(ref T, T)"/>, <seealso cref="Disposable.UpdDABool{T}(ref T, Transform2{T})"/> и др.) приведет к вызову исключения <seealso cref="ObjectDisposedException"/>.</para>
		/// </summary>
		public bool IsDisposeRequested
			=> _disposableImplementation.IsDisposeRequested;

		public bool Disposing
			=> _disposableImplementation.Disposing;

		public bool IsDisposed
			=> _disposableImplementation.IsDisposed;

		public void SetDisposeRequested()
			=> _disposableImplementation.SetDisposeRequested();

		/// <summary>
		/// Выполняет проверку на предмет состояния выгрузки данного объекта.
		/// <para>Если для объекта была начата выгрузка или он уже выгружен (см. <seealso cref="Disposing"/> и <seealso cref="IsDisposed"/>), то вызывается исключение <seealso cref="ObjectDisposedException"/>.</para>
		/// </summary>
		/// <exception cref="ObjectDisposedException">Когда данный объект либо выгружается, либо уже выгружен.</exception>
		protected internal void EnsureNotDisposeState()
			=> _disposableImplementation.EnsureNotDisposeState();

		void IOxyDisposable.EnsureNotDisposeState() => _disposableImplementation.EnsureNotDisposeState();

		/// <summary>
		/// Выполняет проверку на предмет состояния выгрузки данного объекта или наличия запроса на его выгрузку (см. <seealso cref="IsDisposeRequested"/>).
		/// <para>Если для объекта была начата выгрузка или он уже выгружен (см. <seealso cref="Disposing"/> и <seealso cref="IsDisposed"/>), то вызывается исключение <seealso cref="ObjectDisposedException"/>.</para>
		/// <para>Если <paramref name="considerDisposeRequest"/> == true и для объекта была запрошена выгрузка (см. <seealso cref="IsDisposeRequested"/>), то вызывается исключение <seealso cref="ObjectDisposedException"/>.</para>
		/// </summary>
		/// <param name="considerDisposeRequest">
		/// Признак, указывающий проверять ли наличие запроса на выгрузку.
		/// </param>
		public void EnsureNotDisposeState(bool considerDisposeRequest)
			=> _disposableImplementation.EnsureNotDisposeState(considerDisposeRequest: considerDisposeRequest);

		/// <summary>
		/// Метод, вызываемый перед явной или неявной выгрузкой.
		/// <para>Не предназначен для прямого вызова из кода.</para>
		/// <para>Метод не вызывается после того, как один из вызывов был выполнен успешно.</para>
		/// <para>На момент вызова метода <seealso cref="IsDisposeRequested"/> == true.</para>
		/// </summary>
		/// <param name="explicitDispose">Указывает на явную/не явную выгрузку.</param>
		protected virtual void FireBeforeDispose(bool explicitDispose) { }

		/// <summary>
		/// Метод, вызываемый после выполнения явной или неявной выгрузки.
		/// <para>Метод не вызывается после успешного выполнения.</para>
		/// </summary>
		/// <param name="explicitDispose">Указывает на явную/не явную выгрузку.</param>
		protected virtual void FireAfterDisposed(bool explicitDispose) { }

		/// <summary>
		/// Метод выполняет считывание значения из <paramref name="location"/> и проверяет состояние выгрузки данного объекта.
		/// <para>Если данный объект находится в состоянии выгрузки (или уже выгружен), то метод вызовет исключение <see cref="ObjectDisposedException"/>.</para>
		/// <para>Считывание значения выполняется посредством утилит <see cref="VolatileUtilities"/>.</para>
		/// </summary>
		/// <typeparam name="T">Тип считывамого значения.</typeparam>
		/// <param name="location">Адрес считывания.</param>
		/// <returns>Значение <typeparamref name="T"/>.</returns>
		protected internal T ReadDA<T>(ref T location)
			=> _disposableImplementation.ReadDA(ref location);

		protected internal T ReadDA<T>(ref T location, bool considerDisposeRequest)
			=> _disposableImplementation.ReadDA(location: ref location, considerDisposeRequest: considerDisposeRequest);

		protected internal bool TryReadDA<T>(ref T location, bool considerDisposeRequest, out T result)
			=> _disposableImplementation.TryReadDA(location: ref location, considerDisposeRequest: considerDisposeRequest, result: out result);

		protected internal bool TryReadDA<T>(ref T location, out T result)
			=> _disposableImplementation.TryReadDA(location: ref location, considerDisposeRequest: false, result: out result);

		protected internal T TryReadDA<T>(ref T location, bool considerDisposeRequest) {
			T result;
			if (_disposableImplementation.TryReadDA(location: ref location, considerDisposeRequest: considerDisposeRequest, result: out result))
				return result;
			else
				return default;
		}

		protected internal T TryReadDA<T>(ref T location) {
			T result;
			if (_disposableImplementation.TryReadDA(location: ref location, considerDisposeRequest: false, result: out result))
				return result;
			else
				return default;
		}

		protected internal bool TryReadDA<T>(ref IVh<T> location, out T result) {
			if (Disposing || IsDisposed) {
				result = default(T);
				return false;
			}
			else
				try {
					var valueStore = ReadDA(ref location);
					if (valueStore == null) {
						result = default(T);
						return true;
					}
					else if (valueStore.Disposing || valueStore.IsDisposed) {
						result = default(T);
						return false;
					}
					else
						try {
							result = valueStore.Value;
							return true;
						}
						catch (ObjectDisposedException) {
							result = default(T);
							return false;
						}
				}
				catch (ObjectDisposedException) {
					result = default(T);
					return false;
				}
		}

		protected internal T WriteDA<T>(ref T location, T value, T comparand)
			where T : class
			=> _disposableImplementation.WriteDA(location: ref location, value: value, comparand: comparand);

		protected internal void WriteDA<T>(ref T location, T value, T comparand, out bool result)
			where T : class
			=> _disposableImplementation.WriteDA(location: ref location, value: value, comparand: comparand, result: out result);

		protected internal T WriteDA<T>(ref T location, T value)
			where T : class
			=> _disposableImplementation.WriteDA(ref location, value);

		protected internal bool TryWriteDA<T>(ref T location, T value)
			where T : class {
			if (IsDisposeRequested || Disposing || IsDisposed)
				return false;
			else
				try {
					WriteDA(ref location, value);
					return true;
				}
				catch (ObjectDisposedException) {
					if (IsDisposeRequested)
						return false;
					else
						throw;
				}
		}

		protected internal bool TryWriteDA<T>(ref T location, T newValue, out T previousValue)
			where T : class {
			if (IsDisposeRequested || Disposing || IsDisposed) {
				previousValue = default(T);
				return false;
			}
			else
				try {
					previousValue = WriteDA(ref location, newValue);
					return true;
				}
				catch (ObjectDisposedException) {
					previousValue = default(T);
					return false;
				}
		}

		protected internal bool TryWriteDA<T>(ref T location, T newValue, T comparand, out T previousValue)
			where T : class {
			if (IsDisposeRequested || Disposing || IsDisposed) {
				previousValue = default(T);
				return false;
			}
			else
				try {
					previousValue = WriteDA(ref location, newValue, comparand);
					return true;
				}
				catch (ObjectDisposedException) {
					if (IsDisposeRequested) {
						previousValue = default(T);
						return false;
					}
					else
						throw;
				}
		}

		protected internal bool TryWriteDA<T>(ref T location, T newValue, T comparand)
			where T : class {
			if (IsDisposeRequested || Disposing || IsDisposed)
				return false;
			else
				try {
					WriteDA(location: ref location, value: newValue, comparand: comparand);
					return true;
				}
				catch (ObjectDisposedException) {
					if (IsDisposeRequested)
						return false;
					else
						throw;
				}
		}

		protected internal T InitDA<T>(ref T location, Func<T> factory, Action<T> cleanup = default)
			where T : class
			=> _disposableImplementation.InitDA(location: ref location, factory: factory, cleanup: cleanup);

		protected internal T InitDA<T>(ref T location, OutParamFunc<T> factory, Action<T> cleanup = default)
			where T : class
			=> _disposableImplementation.InitDA(location: ref location, factory: factory, cleanup: cleanup);

		protected internal T InitDA<T>(ref IVh<T> location, OutParamFunc<IVh<T>> factory)
			where T : class
			=> _disposableImplementation.InitDA(location: ref location, factory: factory);

		protected internal T InitDA<T>(ref IVh<T> location, Func<IVh<T>> factory)
			where T : class
			=> _disposableImplementation.InitDA(location: ref location, factory: factory);

		protected internal T InitDA<T>(ref Vh<T> location, OutParamFunc<Vh<T>> factory)
			where T : class
			=> _disposableImplementation.InitDA(location: ref location, factory: factory);

		protected internal T InitDA<T>(ref Vh<T> location, Func<Vh<T>> factory)
			where T : class
			=> _disposableImplementation.InitDA(location: ref location, factory: factory);

		protected internal T InitDA<T>(ref ValueHolderClass<T> location, OutParamFunc<T> factory, Action<T> cleanup = default)
			where T : class
			=> _disposableImplementation.InitDA(location: ref location, factory: factory, cleanup: cleanup);

		protected internal T InitDA<T>(ref ValueHolderClass<T> location, Func<T> factory, Action<T> cleanup = default)
			where T : class
			=> _disposableImplementation.InitDA(location: ref location, factory: factory, cleanup: cleanup);

		protected internal T InitDANullable<T>(ref ValueHolderClass<T> location, OutParamFunc<T> factory, Action<T> cleanup = default)
			=> _disposableImplementation.InitDANullable(location: ref location, factory: factory, cleanup: cleanup);

		protected internal T InitDANullable<T>(ref ValueHolderClass<T> location, Func<T> factory, Action<T> cleanup = default)
			=> _disposableImplementation.InitDANullable(location: ref location, factory: factory, cleanup: cleanup);

		/// <summary>
		/// Метод заменяет оригинальное значение в <paramref name="location"/> новым значением, полученным от <paramref name="transform"/>.
		/// <para>Если оригинальное и новое значение идентичны, то метод возвратит false, иначе — true.</para>
		/// <para>В зависимости от наличия изменения значения в <paramref name="location"/> после вызова функции <paramref name="transform"/> эта функция будет вызвана повторно для нового значения из <paramref name="location"/>.</para>
		/// <para>Метод выполняет проверку выгрузки данного объекта при считывании оригинального значения из <paramref name="location"/>, а также при записи нового значения в <paramref name="location"/>.</para>
		/// </summary>
		/// <typeparam name="T">Тип значения переменной.</typeparam>
		/// <param name="location">Адрес переменной, в которой производится обновление значения.</param>
		/// <param name="transform">
		/// Функция трансформации оригинального значения в новое.
		/// <para>В зависимости от наличия изменения значения в <paramref name="location"/> после вызова функции <paramref name="transform"/> эта функция будет вызвана повторно для нового значения из <paramref name="location"/>.</para>
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="transformResultCleanup">
		/// Метод очистки результата вызова функции трансформации <paramref name="transform"/>.
		/// <para>Вызывается в тех случаях, когда результат функции трансформации не востребован:</para>
		/// <para>	• начата выгрузка объекта;</para>
		/// <para>	• после вызова функции <paramref name="transform"/> значение в <paramref name="location"/> изменилось и функция <paramref name="transform"/> должна быть вызвана для изменённого значения в <paramref name="location"/>.</para>
		/// </param>
		/// <returns>
		/// Значение <see cref="bool"/>.
		/// <para>Если значение в переменной <paramref name="location"/> было заменено новым, полученным от <paramref name="transform"/>, то возвращается true, иначе — false.</para>
		/// </returns>
		protected internal bool UpdDABool<T>(ref T location, Transform<T> transform, Action<T> transformResultCleanup = default)
			where T : class
			=> _disposableImplementation.UpdDABool(location: ref location, transform: transform, cleanup: transformResultCleanup);

		/// <summary>
		/// Метод заменяет оригинальное значение в <paramref name="location"/> новым значением, полученным от <paramref name="transform"/>.
		/// <para>Если оригинальное и новое значение идентичны, то метод возвратит false, иначе — true.</para>
		/// <para>В зависимости от наличия изменения значения в <paramref name="location"/> после вызова функции <paramref name="transform"/> эта функция будет вызвана повторно для нового значения из <paramref name="location"/>.</para>
		/// <para>Метод выполняет проверку выгрузки данного объекта при считывании оригинального значения из <paramref name="location"/>, а также при записи нового значения в <paramref name="location"/>.</para>
		/// </summary>
		/// <typeparam name="T">Тип значения переменной.</typeparam>
		/// <param name="location">Адрес переменной, в которой производится обновление значения.</param>
		/// <param name="transform">
		/// Функция трансформации оригинального значения в новое.
		/// <para>В зависимости от наличия изменения значения в <paramref name="location"/> после вызова функции <paramref name="transform"/> эта функция будет вызвана повторно для нового значения из <paramref name="location"/>.</para>
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="transformResultCleanup">
		/// Метод очистки результата вызова функции трансформации <paramref name="transform"/>.
		/// <para>Вызывается в тех случаях, когда результат функции трансформации не востребован:</para>
		/// <para>	• начата выгрузка объекта;</para>
		/// <para>	• после вызова функции <paramref name="transform"/> значение в <paramref name="location"/> изменилось и функция <paramref name="transform"/> должна быть вызвана для изменённого значения в <paramref name="location"/>.</para>
		/// </param>
		/// <param name="result">
		/// Переменная, в которую будет помещен результат операции обновления.
		/// </param>
		/// <returns>
		/// Значение <see cref="bool"/>.
		/// <para>Если значение в переменной <paramref name="location"/> было заменено новым, полученным от <paramref name="transform"/>, то возвращается true, иначе — false.</para>
		/// </returns>
		protected internal bool UpdDABool<T>(ref T location, Transform<T> transform, out T result, Action<T> transformResultCleanup = default)
			where T : class
			=> _disposableImplementation.UpdDABool(location: ref location, transform: transform, cleanup: transformResultCleanup, result: out result);

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
		protected internal void UpdDA<T>(ref T location, Transform<T> transform, out UpdateResult<T> result, Action<T> cleanup = default)
			where T : class
			=> _disposableImplementation.UpdDA(location: ref location, transform: transform, cleanup: cleanup, result: out result);

		protected internal UpdateResult<T> UpdDA<T>(ref T location, Transform<T> transform, Action<T> cleanup = default)
			where T : class {
			_disposableImplementation.UpdDA(location: ref location, transform: transform, cleanup: cleanup, result: out var result);
			return result;
		}

		protected internal bool UpdDABool<T>(ref T location, Transform2<T> transform)
			where T : class
			=> _disposableImplementation.UpdDABool(location: ref location, transform: transform);

		protected internal bool UpdDABool<T>(ref T location, Transform2<T> transform, out T current)
			where T : class
			=> _disposableImplementation.UpdDABool(location: ref location, transform: transform, current: out current);

		protected internal void UpdDA<T>(ref T location, Transform2<T> transform, out UpdateResult<T> result)
			where T : class
			=> _disposableImplementation.UpdDA(location: ref location, transform: transform, result: out result);

		protected internal UpdateResult<T> UpdDA<T>(ref T location, Transform2<T> transform)
			where T : class {
			_disposableImplementation.UpdDA(location: ref location, transform: transform, result: out var result);
			return result;
		}

		protected internal bool UpdDABool<T>(ref T location, T value, T comparand)
			where T : class
			=> ReferenceEquals(objA: _disposableImplementation.WriteDA(location: ref location, value: value, comparand: comparand), objB: comparand);

		/// <summary>
		/// Заменяет значение в <paramref name="location"/> значением <paramref name="value"/>, если значение в <paramref name="location"/> идентично значению <paramref name="comparand"/>.
		/// </summary>
		/// <typeparam name="T">
		/// Тип значения.</typeparam>
		/// <param name="location">
		/// Адрес переменной, в которой производится замена.</param>
		/// <param name="value">
		/// Значение, которым заменяется значение в <paramref name="location"/>.</param>
		/// <param name="comparand">
		/// Значение, которому должно быть идентично значение в <paramref name="location"/>, чтобы была произведена замена.</param>
		/// <param name="current">
		/// Результат операции.
		/// <para>Если в <paramref name="location"/> было установлено значение <paramref name="value"/>, то <paramref name="current"/> == <paramref name="value"/>.</para>
		/// <para>Если в <paramref name="location"/> не было установлено значение <paramref name="value"/>, то <paramref name="current"/> == значение в <paramref name="location"/>.</para>
		/// </param>
		/// <returns>Значение <see cref="bool"/>.</returns>
		protected internal bool UpdDABool<T>(ref T location, T value, T comparand, out T current)
			where T : class {
			var exchangeResult = _disposableImplementation.WriteDA(location: ref location, value: value, comparand: comparand);
			if (ReferenceEquals(exchangeResult, comparand)) {
				current = value;
				return true;
			}
			else {
				current = exchangeResult;
				return false;
			}
		}

		protected internal bool UpdDAIfNullBool<T>(ref T location, T value)
			where T : class
			=> UpdDABool(location: ref location, value: value, comparand: null);

		protected internal bool UpdDAIfNullBool<T>(ref T location, T value, out T current)
			where T : class
			=> UpdDABool(location: ref location, value: value, comparand: null, current: out current);

		protected internal bool UpdDAIfNullBool<T>(ref T location, Func<T> factory, out T current)
			where T : class {
			factory.EnsureNotNull(nameof(factory));
			//
			var locCurrent = ReadDA(location: ref location, considerDisposeRequest: true);
			if (locCurrent is null)
				return UpdDABool(location: ref location, value: factory(), comparand: null, current: out current);
			else {
				current = locCurrent;
				return false;
			}
		}

		protected internal T UpdDAIfNull<T>(ref T location, T value)
			where T : class {
			UpdDABool(location: ref location, value: value, comparand: null, current: out var current);
			return current;
		}

		protected internal T UpdDAIfNull<T>(ref T location, Func<T> factory)
			where T : class {
			factory.EnsureNotNull(nameof(factory));
			//
			var current = ReadDA(location: ref location, considerDisposeRequest: true);
			if (current is null)
				UpdDABool(location: ref location, value: factory(), comparand: null, current: out current);
			return current;
		}

		/// <summary>
		/// Updates the variable in <paramref name="location"/> using special transform function <paramref name="transform"/>.
		/// </summary>
		/// <typeparam name="T">Type of <paramref name="location"/>.</typeparam>
		/// <typeparam name="TState">Type of <paramref name="state"/>.</typeparam>
		/// <param name="location">
		/// Location to be updated.
		/// </param>
		/// <param name="state">
		/// Special state object, that will be used in transform function <paramref name="transform"/>.
		/// </param>
		/// <param name="transform">
		/// Transform function.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="cleanup">
		/// Cleanup function.
		/// <para>Will be called to clean up any state created during transfomation.</para>
		/// </param>
		/// <returns></returns>
		protected internal UpdateResult<T> UpdDA<T, TState>(ref T location, ref TState state, TransformStateful<T, TState> transform, TransformCleanupStateful<T, TState> cleanup = default)
			where T : class
			=> _disposableImplementation.UpdDA(location: ref location, state: ref state, transform: transform, cleanup: cleanup);

		protected internal (UpdateResult<ImmutableDictionary<TKey, TValue>> updateResult, TValue currentValue) UpdDA<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TValue> factory, string locationName = default, Action<TKey, TValue> cleanup = default)
			=> _disposableImplementation.UpdDA(location: ref location, key: key, factory: factory, locationName: locationName, cleanup: cleanup);

		#region Move to 'oxy.ui' package.

		// TODO_HIGH: Move to 'oxy.ui' package.

		//protected void AddEventHandler(ref SynchronizationContextBoundEventHandler<EventHandler, EventArgs> location, EventHandler eventHandler)
		//	=> _disposableImplementation.AddEventHandler(location: ref location, eventHandler: eventHandler);

		//protected void AddEventHandler<TEventArgs>(ref SynchronizationContextBoundEventHandler<EventHandler<TEventArgs>, TEventArgs> location, EventHandler<TEventArgs> eventHandler)
		//	where TEventArgs : EventArgs
		//	=> _disposableImplementation.AddEventHandler(ref location, eventHandler);

		#endregion

		protected internal void AddEventHandler<TEventArgs>(ref EventHandler<TEventArgs> location, EventHandler<TEventArgs> eventHandler)
			where TEventArgs : EventArgs
			=> _disposableImplementation.AddEventHandler(location: ref location, eventHandler: eventHandler);

		protected internal void AddEventHandler(ref EventHandler location, EventHandler eventHandler)
			=> _disposableImplementation.AddEventHandler(ref location, eventHandler);

		#region Move to 'oxy.ui' package.

		// TODO_HIGH: Move to 'oxy.ui' package.

		//protected void RemoveEventHandler(ref SynchronizationContextBoundEventHandler<EventHandler, EventArgs> location, EventHandler eventHandler)
		//	=> _disposableImplementation.RemoveEventHandler(ref location, eventHandler);

		//protected void RemoveEventHandler<TEventArgs>(ref SynchronizationContextBoundEventHandler<EventHandler<TEventArgs>, TEventArgs> location, EventHandler<TEventArgs> eventHandler)
		//	where TEventArgs : EventArgs
		//	=> _disposableImplementation.RemoveEventHandler(ref location, eventHandler);

		#endregion

		protected internal void RemoveEventHandler<TEventArgs>(ref EventHandler<TEventArgs> location, EventHandler<TEventArgs> eventHandler)
			where TEventArgs : EventArgs
			=> _disposableImplementation.RemoveEventHandler(ref location, eventHandler);

		protected internal void RemoveEventHandler(ref EventHandler location, EventHandler eventHandler)
			=> _disposableImplementation.RemoveEventHandler(ref location, eventHandler);

		protected internal IEnumerable<T> EnumerateDA<T>(ref T[ ] location)
			=> EnumerateDA(value: ReadDA(ref location));

		protected internal IEnumerable<T> EnumerateDA<T>(ref T[ ] location, bool considerDisposeRequest)
			=> EnumerateDA(value: ReadDA(ref location, considerDisposeRequest: considerDisposeRequest), considerDisposeRequest: considerDisposeRequest);

		/// <summary>
		/// Перечисляет элементы последовательности <paramref name="value"/> с учетом состояния выгрузки данного объекта.
		/// </summary>
		/// <typeparam name="T">Тип элементов последовательности.</typeparam>
		/// <param name="value">
		/// Перечисляемая последовательность.
		/// <para>Может быть <see langword="null"/>. В этом случае результатом метода будет пустая последовательность.</para>
		/// </param>
		/// <returns>Объект-перечислитель <see cref="IEnumerable{T}"/>.</returns>
		protected internal IEnumerable<T> EnumerateDA<T>(IEnumerable<T> value) {
			EnsureNotDisposeState();
			if (value is null)
				yield break;
			using (var enumerator = new DisposeAffectableEnumerator<T>(source: value, owner: this, considerDisposeRequest: false)) {
				for (; enumerator.MoveNext();)
					yield return enumerator.Current;
			}
		}

		protected internal IEnumerable<T> EnumerateDA<T>(IEnumerable<T> value, bool considerDisposeRequest) {
			EnsureNotDisposeState(considerDisposeRequest: considerDisposeRequest);
			if (value is null)
				yield break;
			using (var enumerator = new DisposeAffectableEnumerator<T>(source: value, owner: this, considerDisposeRequest: considerDisposeRequest)) {
				for (; enumerator.MoveNext();)
					yield return enumerator.Current;
			}
		}

		/// <summary>
		/// Метод, вызываемый механизмами десериализации перед десериализацией объекта.
		/// <para>Метод не предназначен для прямого вызова из кода.</para>
		/// </summary>
		/// <param name="ctx">Контекст десериализации.</param>
		[OnDeserializing]
		internal void Internal_OnDeserializing(StreamingContext ctx) {
			Interlocked.CompareExchange(ref _disposableImplementation, new DisposableImplementation<Disposable>(this), comparand: null);
			//
			OnDeserializing(ctx);
		}

		/// <summary>
		/// Метод, вызываемый механизмами десериализации после десериализации объекта.
		/// <para>Внимание! Метод не предназначен для прямого вызова из кода.</para>
		/// </summary>
		/// <param name="ctx">Контекст десериализации.</param>
		[OnDeserialized]
		internal void Internal_OnDeserialized(StreamingContext ctx) {
			try {
				OnDeserialized(ctx);
			}
			catch (Exception firstException) {
				this.Dispose(firstException);
				throw;
			}
		}

		protected virtual void OnDeserializing(StreamingContext ctx) { }

		protected virtual void OnDeserialized(StreamingContext ctx) { }

		internal void RecreateDisposableImplementation()
			=> WriteDA(ref _disposableImplementation, new DisposableImplementation<Disposable>(disposable: this));

		/// <summary>
		/// Реализует логику выгрузку.
		/// </summary>
		/// <param name="explicitDispose">Указывает на явную/не явную выгрузку.</param>
		protected virtual void Dispose(bool explicitDispose) { }

		public void Dispose() {
			if (!NopDispose) {
				_disposableImplementation.Dispose(explicitDispose: true);
				GC.SuppressFinalize(obj: this);
			}
		}

		void P_Dispose(bool explicitDispose)
			=> Dispose(explicitDispose: explicitDispose);

		~Disposable() {
			if (!NopDispose)
				_disposableImplementation?.Dispose(explicitDispose: false);
		}

	}

}