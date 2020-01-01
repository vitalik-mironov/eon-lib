using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Eon.Threading;

namespace Eon.Collections {

	public static class DictionaryUtilities {

		public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, PrimitiveSpinLock spinLock, TKey key) {
			dictionary.EnsureNotNull(nameof(dictionary));
			spinLock.EnsureNotNull(nameof(spinLock));
			//
			var value = default(TValue);
			return spinLock.Invoke(() => dictionary.TryGetValue(key, out value)) ? value : default;
		}

		public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) {
			dictionary.EnsureNotNull(nameof(dictionary));
			//
			TValue value;
			if (dictionary.TryGetValue(key, out value))
				return value;
			else
				return default;
		}

		public static bool GetOrDefaultCast<TKey, TValue, TCastTo>(this IDictionary<TKey, TValue> dictionary, TKey key, out TCastTo value)
			where TValue : class
			where TCastTo : TValue {
			//
			dictionary.EnsureNotNull(nameof(dictionary));
			//
			if (dictionary.TryGetValue(key: key, value: out var locValue)) {
				value = (TCastTo)locValue;
				return true;
			}
			else {
				value = default;
				return false;
			}
		}

		public static TValue GetOrDefaultOfType<TValue>(this IDictionary<string, object> dictionary, string key) {
			dictionary.EnsureNotNull(nameof(dictionary));
			//
			if (dictionary.TryGetValue(key: key, value: out var valueObject) && valueObject is TValue value)
				return value;
			else
				return default;
		}

		/// <summary>
		/// Получает из словаря значение, соответствующее ключу <paramref name="key"/>.
		/// <para>Если в словаре соответсвующего ключу значения нет, то в словарь помещается значение, полученное от <paramref name="factory"/>.</para>
		/// <para>Операции со словарем выполняются в контексте захвата блокировки посредством <paramref name="spinLock"/>.</para>
		/// </summary>
		/// <typeparam name="TKey">Тип ключа.</typeparam>
		/// <typeparam name="TValue">Тип значения.</typeparam>
		/// <param name="dictionary">
		/// Словарь.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="dictionaryOwner">
		/// Выгружаемый объект-владелец указанного словаря <paramref name="dictionary"/>.
		/// <para>Используется для согласования доступа к указанному словарю на предмет состояния выгрузки объекта-владельца словаря.</para>
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="spinLock">
		/// Объект блокировки, посредством которой выполняется синхронизация доступа к словарю.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="key">
		/// Ключ.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="factory">
		/// Метод, возвращающий значение, которое будет добавлено в словарь, если соответствующего указанному ключу значения нет.
		/// <para>Вызывается единажды. Не вызвается вообще, если в словаре присутствует значение, соответствующее указанному ключу.</para>
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="unclaimedValue">
		/// Метод, вызываемый, когда значение, полученное от <paramref name="factory"/>, не было добавлено в словарь.
		/// <para>Второй аргумент метода — значение, полученное от <paramref name="factory"/>.</para>
		/// </param>
		/// <returns>Значение <typeparamref name="TValue"/>, соответствующее указанному ключу <paramref name="key"/>.</returns>
		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEonDisposable dictionaryOwner, PrimitiveSpinLock spinLock, TKey key, Func<TKey, TValue> factory, Action<TKey, TValue> unclaimedValue = default) {
			dictionary.EnsureNotNull(nameof(dictionary));
			dictionaryOwner.EnsureNotNull(nameof(dictionaryOwner));
			spinLock.EnsureNotNull(nameof(spinLock));
			factory.EnsureNotNull(nameof(factory));
			//
			var resultValue = default(TValue);
			if (spinLock.Invoke(() => { dictionaryOwner.EnsureNotDisposeState(considerDisposeRequest: false); return dictionary.TryGetValue(key, out resultValue); }))
				return resultValue;
			else {
				var callUnclaimedValue = false;
				TValue newValue = default;
				Exception caghtException = default;
				try {
					newValue = factory(arg: key);
					callUnclaimedValue = true;
					spinLock
						.Invoke(
							() => {
								if (dictionary.ContainsKey(key: key)) {
									dictionaryOwner.EnsureNotDisposeState(considerDisposeRequest: false);
									resultValue = dictionary[ key: key ];
								}
								else {
									dictionaryOwner.EnsureNotDisposeState(considerDisposeRequest: true);
									dictionary.Add(key: key, value: resultValue = newValue);
									callUnclaimedValue = false;
								}
							});
					return resultValue;
				}
				catch (Exception exception) {
					caghtException = exception;
					throw;
				}
				finally {
					if (callUnclaimedValue)
						try {
							unclaimedValue?.Invoke(key, newValue);
						}
						catch (Exception firstException) {
							if (caghtException == null)
								throw;
							else
								throw new AggregateException(caghtException, firstException);
						}
				}
			}
		}

		/// <summary>
		/// Получает из словаря значение, соответствующее ключу <paramref name="key"/>.
		/// <para>Если в словаре соответсвующего ключу значения нет, то в словарь помещается значение, полученное от <paramref name="factory"/>.</para>
		/// <para>Операции со словарем выполняются в контексте захвата блокировки посредством <paramref name="spinLock"/>.</para>
		/// </summary>
		/// <typeparam name="TKey">Тип ключа.</typeparam>
		/// <typeparam name="TValue">Тип значения.</typeparam>
		/// <param name="dictionary">
		/// Словарь.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="spinLock">
		/// Объект блокировки, посредством которой выполняется синхронизация доступа к словарю.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="key">
		/// Ключ.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="factory">
		/// Метод, возвращающий значение, которое будет добавлено в словарь, если соответствующего указанному ключу значения нет.
		/// <para>Вызывается единажды. Не вызвается вообще, если в словаре присутствует значение, соответствующее указанному ключу.</para>
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="unclaimedValue">
		/// Метод, вызываемый, когда значение, полученное от <paramref name="factory"/>, не было добавлено в словарь.
		/// <para>Второй аргумент метода — значение, полученное от <paramref name="factory"/>.</para>
		/// </param>
		/// <returns>Значение <typeparamref name="TValue"/>, соответствующее указанному ключу <paramref name="key"/>.</returns>
		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, PrimitiveSpinLock spinLock, TKey key, Func<TKey, TValue> factory, Action<TKey, TValue> unclaimedValue = default) {
			dictionary.EnsureNotNull(nameof(dictionary));
			spinLock.EnsureNotNull(nameof(spinLock));
			factory.EnsureNotNull(nameof(factory));
			//
			var resultValue = default(TValue);
			if (spinLock.Invoke(() => dictionary.TryGetValue(key, out resultValue)))
				return resultValue;
			var createdValue = default(TValue);
			var callUnclaimedValue = false;
			var error = default(Exception);
			try {
				createdValue = factory(key);
				callUnclaimedValue = true;
				spinLock
					.Invoke(
						() => {
							if (dictionary.ContainsKey(key))
								resultValue = dictionary[ key ];
							else {
								dictionary.Add(key, resultValue = createdValue);
								callUnclaimedValue = false;
							}
						});
				return resultValue;
			}
			catch (Exception exception) {
				error = exception;
				throw;
			}
			finally {
				if (callUnclaimedValue)
					try {
						unclaimedValue?.Invoke(key, createdValue);
					}
					catch (Exception firstException) {
						if (error == null)
							throw;
						else
							throw new AggregateException(error, firstException);
					}
			}
		}

		/// <summary>
		/// Получает из словаря значение, соответствующее ключу <paramref name="key"/>.
		/// <para>Если в словаре соответсвующего ключу значения нет, то в словарь помещается значение, полученное от <paramref name="factory"/>.</para>
		/// </summary>
		/// <typeparam name="TKey">Тип ключа.</typeparam>
		/// <typeparam name="TValue">Тип значения.</typeparam>
		/// <param name="dictionary">
		/// Словарь.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="key">
		/// Ключ.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="factory">
		/// Метод-делегат, возвращающий значение, которое будет добавлено в словарь, если соответствующего указанному ключу значения нет.
		/// <para>Вызывается единажды. Не вызвается вообще, если в словаре присутствует значение, соответствующее указанному ключу.</para>
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="unclaimedValue">
		/// Метод-делегат, вызываемый, когда значение, полученное от <paramref name="factory"/>, не было добавлено в словарь.
		/// <para>Первый аргумент метода — значение, полученное от <paramref name="factory"/>.</para>
		/// </param>
		/// <returns>Значение <typeparamref name="TValue"/>, соответствующее указанному ключу <paramref name="key"/>.</returns>
		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> factory, Action<TValue> unclaimedValue = default) {
			dictionary.EnsureNotNull(nameof(dictionary));
			factory.EnsureNotNull(nameof(factory));
			//
			var resultValue = default(TValue);
			if (dictionary.TryGetValue(key, out resultValue))
				return resultValue;
			var createdValue = default(TValue);
			var callNotAddCallback = false;
			var error = default(Exception);
			try {
				createdValue = factory(key);
				callNotAddCallback = true;
				if (dictionary.ContainsKey(key))
					resultValue = dictionary[ key ];
				else {
					dictionary.Add(key, resultValue = createdValue);
					callNotAddCallback = false;
				}
				return resultValue;
			}
			catch (Exception firstException) {
				error = firstException;
				throw;
			}
			finally {
				if (callNotAddCallback && unclaimedValue != null)
					try {
						unclaimedValue(createdValue);
					}
					catch (Exception firstException) {
						if (error == null)
							throw;
						else
							throw new AggregateException(error, firstException);
					}
			}
		}

		public static async Task<TValue> GetOrAddAsync<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, Task<TValue>> factory, Func<TKey, TValue, Task> unclaimedValue = default) {
			dictionary.EnsureNotNull(nameof(dictionary));
			factory.EnsureNotNull(nameof(factory));
			//
			if (dictionary.TryGetValue(key, out var resultValue))
				return resultValue;
			else {
				var createdValue = default(TValue);
				var callUnclaimedValue = false;
				var tryException = default(Exception);
				try {
					createdValue = await factory(key).ConfigureAwait(false);
					callUnclaimedValue = true;
					if (!dictionary.TryGetValue(key, out resultValue)) {
						dictionary.Add(key, resultValue = createdValue);
						callUnclaimedValue = false;
					}
					return resultValue;
				}
				catch (Exception exception) {
					tryException = exception;
					throw;
				}
				finally {
					if (callUnclaimedValue && !(unclaimedValue is null))
						try {
							await unclaimedValue(key, createdValue).ConfigureAwait(false);
						}
						catch (Exception exception) {
							if (tryException is null)
								throw;
							else
								throw new AggregateException(tryException, exception);
						}
				}
			}
		}

		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) {
			dictionary.EnsureNotNull(nameof(dictionary));
			if (key == null)
				throw new ArgumentNullException(paramName: nameof(key));
			//
			if (!dictionary.TryGetValue(key, out var result)) {
				result = value;
				try {
					dictionary.Add(key, value);
				}
				catch (ArgumentException exception) {
					try {
						result = dictionary[ key ];
					}
					catch (Exception secondException) {
						throw new AggregateException(exception, secondException);
					}
				}
			}
			return result;
		}

		public static async Task<TValue> GetOrAddAsync<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEonDisposable owner, PrimitiveSpinLock spinLock, TKey key, Func<TKey, Task<TValue>> factory, Func<TKey, TValue, Task> unclaimedValue = default) {
			dictionary.EnsureNotNull(nameof(dictionary));
			owner.EnsureNotNull(nameof(owner));
			spinLock.EnsureNotNull(nameof(spinLock));
			if (key == null)
				throw new ArgumentNullException(paramName: nameof(key));
			factory.EnsureNotNull(nameof(factory));
			//
			var resultValue = default(TValue);
			if (spinLock.Invoke(() => { owner.EnsureNotDisposeState(considerDisposeRequest: false); return dictionary.TryGetValue(key: key, value: out resultValue); }))
				return resultValue;
			else {
				var callUnclaimedValue = false;
				TValue newValue = default;
				Exception caghtException = default;
				try {
					newValue = await factory(arg: key).ConfigureAwait(false);
					callUnclaimedValue = true;
					spinLock
						.Invoke(
							action:
								() => {
									if (dictionary.ContainsKey(key: key)) {
										owner.EnsureNotDisposeState(considerDisposeRequest: false);
										resultValue = dictionary[ key: key ];
									}
									else {
										owner.EnsureNotDisposeState(considerDisposeRequest: true);
										dictionary.Add(key: key, value: resultValue = newValue);
										callUnclaimedValue = false;
									}
								});
					return resultValue;
				}
				catch (Exception exception) {
					caghtException = exception;
					throw;
				}
				finally {
					if (callUnclaimedValue && !(unclaimedValue is null))
						try {
							await unclaimedValue(arg1: key, arg2: newValue).ConfigureAwait(false);
						}
						catch (Exception exception) {
							if (caghtException is null)
								throw;
							else
								throw new AggregateException(caghtException, exception);
						}
				}
			}
		}

		public static bool Remove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, PrimitiveSpinLock spinLock, TKey key, out TValue value) {
			dictionary.EnsureNotNull(nameof(dictionary));
			spinLock.EnsureNotNull(nameof(spinLock));
			if (key == null)
				throw new ArgumentNullException(paramName: nameof(key));
			//
			var result =
				spinLock
				.Invoke(
					() => {
						TValue locValue;
						if (dictionary.TryGetValue(key, out locValue))
							return
								new {
									IsValueRemoved = dictionary.Remove(key),
									Value = locValue
								};
						else
							return
								new {
									IsValueRemoved = false,
									Value = default(TValue)
								};
					});
			value = result.Value;
			return result.IsValueRemoved;
		}

		public static bool Remove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, PrimitiveSpinLock spinLock, TKey key)
			=> Remove(dictionary: dictionary, spinLock: spinLock, key: key, value: out var locValue);

		public static bool TryGetAnyKeyValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, out TValue value, params TKey[ ] keys) {
			dictionary.EnsureNotNull(nameof(dictionary));
			//
			int keysLength;
			if (!(keys is null || (keysLength = keys.Length) == 0)) {
				var lowerBound = keys.GetLowerBound(0);
				for (var offset = 0; offset < keysLength; offset++) {
					var key = keys[ offset + lowerBound ];
					if (key == null)
						throw new ArgumentNullException(paramName: $"{nameof(keys)}[{(offset + lowerBound):d}]");
					else if (dictionary.TryGetValue(key, out value))
						return true;
				}
			}
			value = default;
			return false;
		}

		public static bool TryGetAnyKeyValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key1, TKey key2, out TValue value)
			=> TryGetAnyKeyValue(dictionary, out value, keys: new[ ] { key1, key2 });

		public static bool TryGetAnyKeyValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key1, TKey key2, TKey key3, out TValue value)
			=> TryGetAnyKeyValue(dictionary, out value, keys: new[ ] { key1, key2, key3 });

	}

}