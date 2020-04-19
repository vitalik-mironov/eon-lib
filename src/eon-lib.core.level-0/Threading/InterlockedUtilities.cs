using System;
using System.Runtime.CompilerServices;
using System.Threading;

using vlt = Eon.Threading.VolatileUtilities;

namespace Eon.Threading {

	public static class InterlockedUtilities {

		public static int Or(ref int location, int value) {
			var original = vlt.Read(ref location);
			int memo;
			for (; ; ) {
				memo = original;
				original = Interlocked.CompareExchange(ref location, original | value, original);
				if (memo == original)
					return original;
			}
		}

		public static int And(ref int location, int value) {
			var original = vlt.Read(ref location);
			int memo;
			for (; ; ) {
				memo = original;
				original = Interlocked.CompareExchange(location1: ref location, value: original & value, comparand: original);
				if (memo == original)
					return original;
			}
		}

		public static int Xor(ref int location, int value)
			=> And(location: ref location, value: ~value);

		/// <summary>
		/// Выполняет уменьшение значения в <paramref name="location"/> на единицу, при условии, что текущее значение в <paramref name="location"/> больше ограничителя <paramref name="minExclusive"/>.
		/// <para>Если значение в переменной <paramref name="location"/> было уменьшено, метод возвратит True, иначе — False.</para>
		/// </summary>
		/// <param name="location">Переменная, значение которой уменьшается.</param>
		/// <param name="minExclusive">Ограничитель значения в переменной <paramref name="location"/>.</param>
		/// <param name="result">
		/// Переменная для сохранения результата операции.
		/// <para>Если значение в переменной <paramref name="location"/> уменьшено, то в данную переменную будет сохранен результат уменьшения, в противном случае будет сохранено текущее значение переменной <paramref name="location"/>.</para>
		/// </param>
		/// <returns>Значение <see cref="bool"/>.</returns>
		public static bool Decrement(ref int location, int minExclusive, out int result) {
			bool isDecremented;
			Decrement(location: ref location, minExclusive: minExclusive, result: out result, isDecremented: out isDecremented);
			return isDecremented;
		}

		/// <summary>
		/// Выполняет уменьшение значения в <paramref name="location"/> на единицу, при условии, что текущее значение в <paramref name="location"/> больше ограничителя <paramref name="minExclusive"/>.
		/// </summary>
		/// <param name="location">Переменная, значение которой уменьшается.</param>
		/// <param name="minExclusive">Ограничитель значения в переменной <paramref name="location"/>.</param>
		/// <param name="result">
		/// Переменная для сохранения результата операции уменьшения.
		/// <para>Если значение в переменной <paramref name="location"/> уменьшено, то в данную переменную будет сохранен результат уменьшения, в противном случае будет сохранено текущее значение переменной <paramref name="location"/>.</para>
		/// </param>
		/// <param name="isDecremented">
		/// Переменная для сохранения результата операции: значение в <paramref name="location"/> уменьшено или нет.
		/// </param>
		public static void Decrement(ref int location, int minExclusive, out int result, out bool isDecremented) {
			var current = vlt.Read(ref location);
			int currentBackup;
			for (; ; ) {
				if (current > minExclusive) {
					currentBackup = current;
					current = Interlocked.CompareExchange(ref location, current - 1, current);
					if (current == currentBackup) {
						isDecremented = true;
						result = current - 1;
						return;
					}
				}
				else {
					isDecremented = false;
					result = current;
					return;
				}
			}
		}

		/// <summary>
		/// Выполняет уменьшение значения в <paramref name="location"/> на единицу, при условии, что текущее значение в <paramref name="location"/> больше ограничителя <paramref name="minExclusive"/>.
		/// </summary>
		/// <param name="location">Переменная, значение которой уменьшается.</param>
		/// <param name="minExclusive">Ограничитель значения в переменной <paramref name="location"/>.</param>
		/// <param name="isDecremented">
		/// Переменная для сохранения результата операции: значение в <paramref name="location"/> уменьшено или нет.
		/// </param>
		public static int Decrement(ref int location, int minExclusive, out bool isDecremented) {
			int result;
			Decrement(location: ref location, minExclusive: minExclusive, result: out result, isDecremented: out isDecremented);
			return result;
		}

		/// <summary>
		/// Выполняет уменьшение значения в <paramref name="location"/> на единицу, при условии, что текущее значение в <paramref name="location"/> больше ограничителя <paramref name="minExclusive"/>.
		/// <para>Если значение в переменной <paramref name="location"/> было уменьшено, метод возвратит True, иначе — False.</para>
		/// </summary>
		/// <param name="location">Переменная, значение которой уменьшается.</param>
		/// <param name="minExclusive">Ограничитель значения в переменной <paramref name="location"/>.</param>
		/// <param name="result">
		/// Переменная для сохранения результата операции.
		/// <para>Если значение в переменной <paramref name="location"/> уменьшено, то в данную переменную будет сохранен результат уменьшения, в противном случае будет сохранено текущее значение переменной <paramref name="location"/>.</para>
		/// </param>
		/// <returns>Значение <see cref="bool"/>.</returns>
		public static bool Decrement(ref long location, long minExclusive, out long result) {
			var current = vlt.Read(ref location);
			long currentBackup;
			for (; ; ) {
				if (current > minExclusive) {
					currentBackup = current;
					current = Interlocked.CompareExchange(ref location, current - 1L, current);
					if (current == currentBackup) {
						result = current - 1L;
						return true;
					}
				}
				else {
					result = current;
					return false;
				}
			}
		}

		/// <summary>
		/// Выполняет уменьшение значения в <paramref name="location"/> на единицу, при условии, что текущее значение в <paramref name="location"/> больше ограничителя <paramref name="minExclusive"/>.
		/// <para>Если значение в переменной <paramref name="location"/> было уменьшено, метод возвратит True, иначе — False.</para>
		/// </summary>
		/// <param name="location">Переменная, значение которой уменьшается.</param>
		/// <param name="minExclusive">Ограничитель значения в переменной <paramref name="location"/>.</param>
		/// <returns>Значение <see cref="bool"/>.</returns>
		public static bool Decrement(ref int location, int minExclusive) {
			int result;
			return Decrement(location: ref location, minExclusive: minExclusive, result: out result);
		}

		public static void Increment(ref int location, int maxInclusive, out bool isIncremented)
			=> Increment(location: ref location, maxInclusive: maxInclusive, result: out var result, isIncremented: out isIncremented);

		public static void Increment(ref int location, int maxInclusive, out int result, out bool isIncremented) {
			int currentBackup;
			var current = vlt.Read(ref location);
			for (; ; ) {
				if (current < maxInclusive) {
					currentBackup = current;
					current = Interlocked.CompareExchange(location1: ref location, value: current + 1, comparand: current);
					if (current == currentBackup) {
						isIncremented = true;
						result = current + 1;
						return;
					}
				}
				else {
					isIncremented = false;
					result = current;
					return;
				}
			}
		}

		public static void Increment(ref long location, long maxInclusive, out long result, out bool isIncremented) {
			long currentBackup;
			var current = vlt.Read(location: ref location);
			for (; ; ) {
				if (current < maxInclusive) {
					currentBackup = current;
					current = Interlocked.CompareExchange(location1: ref location, value: current + 1L, comparand: current);
					if (current == currentBackup) {
						isIncremented = true;
						result = current + 1L;
						return;
					}
				}
				else {
					isIncremented = false;
					result = current;
					return;
				}
			}
		}

		public static bool IncrementBool(ref long location, long maxInclusive) {
			Increment(ref location, maxInclusive, out var result, out var isIncremented);
			return isIncremented;
		}

		// TODO: Put strings into the resources.
		//
		public static long Increment(ref long location, long maxInclusive, string locationName = default) {
			Increment(location: ref location, maxInclusive: maxInclusive, result: out var incrementResult, isIncremented: out var isIncremented);
			if (isIncremented)
				return incrementResult;
			else
				throw
					new OverflowException(
						message: $"Невозможно увеличить значение переменной, так как её значение достигло предела.{Environment.NewLine}\tПредел:{Environment.NewLine}\t\t{maxInclusive.ToString("d")}{(locationName is null ? string.Empty : $"{Environment.NewLine}\tИмя переменной:{Environment.NewLine}\t\t{locationName}")}");
		}

		public static bool IncrementBool(ref int location, int maxInclusive, out int result) {
			Increment(ref location, maxInclusive, out result, out var isIncremented);
			return isIncremented;
		}

		public static bool IncrementBool(ref int location, int maxInclusive) {
			bool isIncremented;
			int result;
			Increment(location: ref location, maxInclusive: maxInclusive, result: out result, isIncremented: out isIncremented);
			return isIncremented;
		}

		public static UpdateResult<T> Update<T>(ref T location, Transform<T> transform)
			where T : class {
			//
			return Update(location: ref location, transform: transform, isUpdated: out _);
		}

		public static UpdateResult<T> Update<T>(ref T location, Transform<T> transform, out bool isUpdated)
			where T : class {
			if (transform is null)
				throw new ArgumentNullException(paramName: nameof(transform));
			//
			var currentValue = Get(location: ref location);
			T exchangeResult;
			for (; ; ) {
				var newValue = transform(current: currentValue);
				if (ReferenceEquals(objA: newValue, objB: currentValue)) {
					// Новое значение идентично существующему.
					//
					isUpdated = false;
					return new UpdateResult<T>(current: newValue, original: newValue);
				}
				// Результат interlocked-обмена значения.
				//
				exchangeResult = Interlocked.CompareExchange(location1: ref location, value: newValue, comparand: currentValue);
				if (ReferenceEquals(exchangeResult, currentValue)) {
					// Обмен выполнен.
					//
					isUpdated = true;
					return new UpdateResult<T>(current: newValue, original: currentValue);
				}
				currentValue = exchangeResult;
			}
		}

		public static UpdateResult<T> Update<T>(ref T location, Transform2<T> transform)
			where T : class {
			//
			bool isUpdated;
			return Update(location: ref location, transform: transform, isUpdated: out isUpdated);
		}

		public static UpdateResult<T> Update<T>(ref T location, Transform2<T> transform, out bool isUpdated)
			where T : class {
			//
			if (transform is null)
				throw new ArgumentNullException(paramName: nameof(transform));
			//
			var currentValue = Get(ref location);
			T exchangeResult;
			var previousTransformResult = default(T);
			for (; ; ) {
				var newValue = transform(current: currentValue, previousTransformResult: previousTransformResult);
				if (ReferenceEquals(newValue, currentValue)) {
					// Новое значение идентично существующему.
					//
					isUpdated = false;
					return new UpdateResult<T>(current: newValue, original: newValue);
				}
				// Результат interlocked-обмена значения.
				//
				exchangeResult = Interlocked.CompareExchange(location1: ref location, value: newValue, comparand: currentValue);
				if (ReferenceEquals(exchangeResult, currentValue)) {
					// Обмен выполнен.
					//
					isUpdated = true;
					return new UpdateResult<T>(current: newValue, original: currentValue);
				}
				currentValue = exchangeResult;
				previousTransformResult = newValue;
			}
		}

		public static bool UpdateBool<T>(ref T location, T value, T comparand)
			where T : class
			=> ReferenceEquals(objA: comparand, objB: Interlocked.CompareExchange(location1: ref location, value: value, comparand: comparand));

		public static T UpdateIfNull<T>(ref T location, T value)
			where T : class
			=> Interlocked.CompareExchange(location1: ref location, value: value, comparand: null) ?? value;

		public static T UpdateIfNull<T>(ref T location, Func<T> factory)
			where T : class {
			factory.EnsureNotNull(nameof(factory));
			//
			var current = Get(location: ref location);
			if (current is null) {
				T factoried;
				return Interlocked.CompareExchange(location1: ref location, value: factoried = factory(), comparand: null) ?? factoried;
			}
			else
				return current;
		}

		public static T UpdateIfNull<T>(ref T location, OutParamFunc<T> factory)
			where T : class {
			factory.EnsureNotNull(nameof(factory));
			//
			var current = Get(location: ref location);
			if (current is null) {
				factory(param: out var factoried);
				return Interlocked.CompareExchange(location1: ref location, value: factoried, comparand: null) ?? factoried;
			}
			else
				return current;
		}

		public static bool UpdateIfNullBool<T>(ref T location, T value, out T current)
			where T : class {
			var exchangeResult = Interlocked.CompareExchange(ref location, value, null);
			if (exchangeResult is null) {
				current = value;
				return true;
			}
			else {
				current = exchangeResult;
				return false;
			}
		}

		public static bool UpdateIfNullBool<T>(ref T location, T value) where T : class
			=> Interlocked.CompareExchange(ref location, value, null) is null;

		public static void Update(ref long location, Transform<long> transform, out long current, out bool isUpdated) {
			if (transform is null)
				throw new ArgumentNullException(paramName: nameof(transform));
			//
			var currentValue = vlt.Read(ref location);
			long exchangeResult;
			for (; ; ) {
				var newValue = transform(current: currentValue);
				if (currentValue == newValue) {
					current = currentValue;
					isUpdated = false;
					break;
				}
				else {
					exchangeResult = Interlocked.CompareExchange(location1: ref location, value: newValue, comparand: currentValue);
					if (exchangeResult == currentValue) {
						// Обмен выполнен.
						//
						current = newValue;
						isUpdated = true;
						break;
					}
					currentValue = exchangeResult;
				}
			}
		}

		// TODO: Put strings into the resources.
		//
		public static T Initialize<T>(ref T location, OutParamFunc<T> factory)
			where T : class {
			factory.EnsureNotNull(nameof(factory));
			//
			var current = Get(location: ref location);
			if (current is null) {
				factory(param: out var factoried);
				if (factoried is null)
					throw new InvalidOperationException(message: $"Factory method has returned invalid result '{FormatStringUtilitiesCoreL0.GetNullValueText()}'.{Environment.NewLine}\tMethod:{Environment.NewLine}\t\t{factory}");
				var exchange = Interlocked.CompareExchange(location1: ref location, value: factoried, comparand: null);
				if (exchange is null)
					return factoried;
				else
					return exchange;
			}
			else
				return current;
		}

		// TODO: Put strings into the resources.
		//
		public static T Initialize<T>(ref T location, Func<T> factory)
			where T : class {
			factory.EnsureNotNull(nameof(factory));
			//
			var current = Get(location: ref location);
			if (current is null) {
				var factoried = factory();
				if (factoried is null)
					throw new InvalidOperationException(message: $"Factory method has returned invalid result '{FormatStringUtilitiesCoreL0.GetNullValueText()}'.{Environment.NewLine}\tMethod:{Environment.NewLine}\t\t{factory}");
				var exchange = Interlocked.CompareExchange(location1: ref location, value: factoried, comparand: null);
				if (exchange is null)
					return factoried;
				else
					return exchange;
			}
			else
				return current;
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static T Get<T>(ref T location) where T : class
			=> vlt.Read(ref location);

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static long Get(ref long location)
			=> vlt.Read(ref location);

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static int Get(ref int location)
			=> vlt.Read(ref location);

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static bool SetNullBool<T>(ref T location, T comparand)
			where T : class
			=> ReferenceEquals(comparand, Interlocked.CompareExchange(location1: ref location, value: null, comparand: comparand));

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static T SetNull<T>(ref T location, T comparand)
			where T : class
			=> Interlocked.CompareExchange(location1: ref location, value: null, comparand: comparand);

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static T SetNull<T>(ref T location)
			where T : class
			=> Interlocked.Exchange(location1: ref location, value: null);

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static T Set<T>(ref T location, T value)
			where T : class
			=> Interlocked.Exchange(location1: ref location, value: value);

	}

}