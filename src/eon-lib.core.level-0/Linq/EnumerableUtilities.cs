using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Eon.Linq {

	public static class EnumerableUtilities {

		#region Nested types

		#endregion

		public static LinkedList<T> ToLinkedList<T>(this IEnumerable<T> source) {
			if (source is null)
				throw new ArgumentNullException(paramName: nameof(source));
			//
			return new LinkedList<T>(collection: source);
		}

		/// <summary>
		/// Возвращает пустую последовательность <see cref="IEnumerable"/>.
		/// </summary>
		/// <returns>Объект <see cref="IEnumerable"/>.</returns>
		public static IEnumerable EmptySequence()
			=> Enumerable.Empty<object>();

		/// <summary>
		/// Возвращает пустую последовательность <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="T">Тип элемента последовательности.</typeparam>
		/// <param name="typeExpression">
		/// Выражение, определяющее тип элемента последовательности.
		/// <para>Данный параметр никак не используется при выполнении, а служит лишь подсказкой для редактора кода в определении типа элемента <typeparamref name="T"/>.</para>
		/// <para>Может быть <see langword="null"/>.</para>
		/// </param>
		/// <returns>Объект <seealso cref="IEnumerable{T}"/>.</returns>
#pragma warning disable IDE0060 // Remove unused parameter
		public static IEnumerable<T> EmptySequence<T>(Expression<Func<T>> typeExpression = null)
#pragma warning restore IDE0060 // Remove unused parameter
			=> Enumerable.Empty<T>();

		/// <summary>
		/// Возвращает пустую последовательность <see cref="IEnumerable{T}"/>.
		/// <para>Метод полезен для создания пустой последовательности элементов анонимного типа, который указывается посредством параметра <paramref name="element"/>.</para>
		/// </summary>
		/// <typeparam name="T">Тип элемента последовательности.</typeparam>
		/// <param name="element">
		/// Элемент последовательности.
		/// <para>Данный параметр никак не используется при выполнении (указанный элемент, само собой, не будет входить в возвращаемую методом последовательность), а служит лишь подсказкой для предположения типа элемента <typeparamref name="T"/>.</para>
		/// </param>
		/// <returns>Объект <see cref="IEnumerable{T}"/>.</returns>
#pragma warning disable IDE0060 // Remove unused parameter
		public static IEnumerable<T> EmptySequence<T>(this T element)
#pragma warning restore IDE0060 // Remove unused parameter
			=> Enumerable.Empty<T>();

		public static IEnumerable<T> Sequence<T>(this T seed, Func<T, bool> @break, Func<T, T> next) {
			if (@break is null)
				throw new ArgumentNullException(paramName: nameof(@break));
			else if (next is null)
				throw new ArgumentNullException(paramName: nameof(next));
			//
			for (var currentValue = seed; !@break(currentValue); currentValue = next(currentValue))
				yield return currentValue;
		}

		public static IEnumerable<T> Sequence<T>(Func<int, T> factory, int length)
			=> CreateSequence(factory: factory, length: length);

		/// <summary>
		/// Возвращает последовательность заданной длины, каждый элемент которой определяется параметром <paramref name="item"/>.
		/// </summary>
		/// <typeparam name="T">Тип элемента последовательности.</typeparam>
		/// <param name="item">
		/// Элемент последовательности.
		/// <para>Может быть null.</para>
		/// </param>
		/// <param name="length">
		/// Длина последовательности.
		/// <para>Не может быть менее 0.</para>
		/// </param>
		/// <returns>Объект <see cref="IEnumerable{T}"/>.</returns>
		public static IEnumerable<T> Sequence<T>(this T item, int length) {
			if (length < 0)
				throw new ArgumentOutOfRangeException(paramName: nameof(length), message: $"Cannot be less than zero.");
			//
			return Enumerable.Repeat(element: item, count: length);
		}

		/// <summary>
		/// Возвращает последовательность из одного элемента.
		/// </summary>
		/// <typeparam name="T">Тип элемента последовательности.</typeparam>
		/// <param name="item">
		/// Элемент последовательности.
		/// <para>Может быть <see langword="null"/>.</para>
		/// </param>
		/// <returns>Объект <see cref="IEnumerable{T}"/>.</returns>
		public static IEnumerable<T> Sequence<T>(this T item)
			=> Enumerable.Repeat(item, 1);

		/// <summary>
		/// Создает последовательность заданной длины из элементов, возвращаемых методом <paramref name="factory"/>.
		/// </summary>
		/// <typeparam name="T">Тип элемента последовательности.</typeparam>
		/// <param name="factory">
		/// Метод, возвращающий элемент последовательности для заданной позиции.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="length">
		/// Длина последовательности.
		/// <para>Не может быть менее 0.</para>
		/// </param>
		/// <returns>Объект <see cref="IEnumerable{T}"/>.</returns>
		// TODO: Put strings into the resources.
		//
		public static IEnumerable<T> CreateSequence<T>(Func<int, T> factory, int length) {
			if (factory is null)
				throw new ArgumentNullException(paramName: nameof(factory));
			else if (length < 0)
				throw new ArgumentOutOfRangeException(paramName: nameof(length), message: $"Cannot be less than zero.");
			//
			if (length == 0)
				yield break;
			else
				for (var i = 0; i < length; i++)
					yield return factory(i);
		}

		/// <summary>
		/// Создает новую последовательность из указанного массива.
		/// </summary>
		/// <typeparam name="T">Тип элемента последовательности.</typeparam>
		/// <param name="source">
		/// Массив элементов, из которого создается последовательность.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <returns>Объект <see cref="IEnumerable{T}"/>.</returns>
		public static IEnumerable<T> ToEnumerable<T>(this T[ ] source) {
			if (source is null)
				throw new ArgumentNullException(paramName: nameof(source));
			//
			foreach (var element in source)
				yield return element;
		}

		public static TSource NullIfEmpty<TSource>(this TSource source)
			where TSource : class, IEnumerable {
			if (source is null)
				return null;
			else {
				var enumerator = source.GetEnumerator();
				return enumerator.MoveNext() ? source : null;
			}
		}

		/// <summary>
		/// Возвращает пустую последовательность, если <paramref name="source"/> есть <see langword="null"/>.
		/// </summary>
		/// <typeparam name="T">Тип элемента последовательности.</typeparam>
		/// <param name="source">Входной параметр, представляющий последовательность.</param>
		/// <returns>Объект <see cref="IEnumerable{T}"/>.</returns>
		public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
			=> source ?? Enumerable.Empty<T>();

		/// <summary>
		/// Выполняет операцию <paramref name="action"/> над каждым элементом последовательности.
		/// </summary>
		/// <typeparam name="T">Тип элемента последовательности.</typeparam>
		/// <param name="source">Последовательность.</param>
		/// <param name="action">Делегат, выполняющий операцию на элементом последовательности.</param>
		/// <returns>Объект <see cref="IEnumerable{T}"/>.</returns>
		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action) {
			if (source is null)
				throw new ArgumentNullException(paramName: nameof(source));
			else if (action is null)
				throw new ArgumentNullException(paramName: nameof(action));
			//
			foreach (var element in source) {
				action(element);
				yield return element;
			}
		}

		/// <summary>
		/// Выполняет операцию <paramref name="action"/> над каждым элементом последовательности.
		/// </summary>
		/// <typeparam name="T">Тип элемента последовательности.</typeparam>
		/// <param name="source">Последовательность.</param>
		/// <param name="action">Делегат, выполняющий операцию на элементом последовательности.</param>
		public static void Observe<T>(this IEnumerable<T> source, Action<T, int> action) {
			if (source is null)
				throw new ArgumentNullException(paramName: nameof(source));
			else if (action is null)
				throw new ArgumentNullException(paramName: nameof(action));
			//
			var indexCounter = 0;
			foreach (var element in source) {
				checked { indexCounter++; }
				action(arg1: element, arg2: indexCounter);
			}
		}

		/// <summary>
		/// Выполняет операцию <paramref name="action"/> над каждым элементом последовательности.
		/// </summary>
		/// <typeparam name="T">Тип элемента последовательности.</typeparam>
		/// <param name="source">Последовательность.</param>
		/// <param name="action">Делегат, выполняющий операцию на элементом последовательности.</param>
		public static void Observe<T>(this IEnumerable<T> source, Action<T> action) {
			if (source is null)
				throw new ArgumentNullException(paramName: nameof(source));
			else if (action is null)
				throw new ArgumentNullException(paramName: nameof(action));
			//
			foreach (var element in source)
				action(obj: element);
		}

		/// <summary>
		/// Фильтрует последовательность от <see langword="null"/>-элементов.
		/// </summary>
		/// <typeparam name="T">
		/// Тип элемента последовательности.
		/// </typeparam>
		/// <param name="source">
		/// Исходная последовательность, на которую накладывается фильтра.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <returns>Объект <seealso cref="IEnumerable{T}"/>.</returns>
		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<T> SkipNull<T>(this IEnumerable<T> source)
			=> (source ?? throw new ArgumentNullException(paramName: nameof(source))).Where(locItem => locItem != null);

		public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
			if (source is null)
				throw new ArgumentNullException(paramName: nameof(source));
			else if (predicate is null)
				throw new ArgumentNullException(paramName: nameof(predicate));
			//
			var indexCounter = -1;
			foreach (var value in source) {
				checked { indexCounter++; }
				if (predicate(value))
					return indexCounter;
			}
			return -1;
		}

		// TODO: Put strings into the resources.
		//
		public static void CopyTo<T>(this IEnumerable<T> source, T[ ] array, int arrayIndex) {
			if (source is null)
				throw new ArgumentNullException(paramName: nameof(source));
			else if (array is null)
				throw new ArgumentNullException(paramName: nameof(array));
			var lowerBound = array.GetLowerBound(dimension: 0);
			var upperBound = array.GetUpperBound(dimension: 0);
			if (upperBound < lowerBound)
				throw new ArgumentException(paramName: nameof(array), message: "Cannot be an empty array.");
			else if (arrayIndex < lowerBound)
				throw
					new ArgumentOutOfRangeException(paramName: nameof(arrayIndex), message: $"Cannot be less than array lower bound.{Environment.NewLine}\tValue:{Environment.NewLine}\t\t{arrayIndex:d}{Environment.NewLine}\tLower bound:{Environment.NewLine}\t\t{lowerBound:d}");
			else if (arrayIndex > upperBound)
				throw
					new ArgumentOutOfRangeException(paramName: nameof(arrayIndex), message: $"Cannot be greater than array upper bound.{Environment.NewLine}\tValue:{Environment.NewLine}\t\t{arrayIndex:d}{Environment.NewLine}\tUpper bound:{Environment.NewLine}\t\t{upperBound:d}");
			//
			var nextArrayIndex = arrayIndex;
			foreach (var sourceItem in source) {
				array[ nextArrayIndex ] = sourceItem;
				if (nextArrayIndex == upperBound)
					break;
				nextArrayIndex++;
			}
		}

		public static bool All<T>(this IEnumerable<T> source, Func<T, int, bool> predicate, string sourceArgName = null) {
			sourceArgName = sourceArgName ?? nameof(source);
			if (source is null)
				throw new ArgumentNullException(paramName: sourceArgName);
			else if (predicate is null)
				throw new ArgumentNullException(paramName: nameof(predicate));
			else {
				var result = true;
				var elementIndexCounter = -1;
				foreach (var element in source) {
					result = predicate(element, ++elementIndexCounter);
					if (!result)
						break;
				}
				return result;
			}
		}

		public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T item)
			=> Concat(source: source, item: item, length: 1);

		public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T item, int length) {
			if (source is null)
				throw new ArgumentNullException(paramName: nameof(source));
			//
			return source.Concat(second: item.Sequence(length: length));
		}

	}

}