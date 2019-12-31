using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Eon.Collections;

namespace Eon {

	public static class ArrayUtilities {

		#region Nested types

		[DebuggerDisplay("Count = {Count}")]
		sealed class P_ArrayReadOnlyCollectionWrapper<T>
			:ICollection<T>, ICountableEnumerable<T> {

			#region Static & constant members

			static readonly IEnumerable<T> __EmptyEnumerable = Enumerable.Empty<T>();

			static readonly EqualityComparer<T> __EqComparer = EqualityComparer<T>.Default;

			#endregion

			readonly Func<T, bool> _contains;

			readonly Action<T[ ], int> _copyTo;

			readonly Func<int> _count;

			readonly Func<bool> _isEmpty;

			readonly Func<T> _last;

			readonly Func<T> _first;

			readonly Func<int, T> _getByIndex;

			readonly Func<IEnumerator<T>> _getEnumerator;

			readonly T[ ] _underlyingArray;

			// TODO: Put exception messages into the resources.
			//
			internal P_ArrayReadOnlyCollectionWrapper(T[ ] underlyingArray) {
				_underlyingArray = underlyingArray;
				if (underlyingArray.IsNullOrEmpty()) {
					_contains = i => false;
					_copyTo = (d, i) => { };
					_count = () => 0;
					_isEmpty = () => true;
					_last = () => throw new InvalidOperationException("There is no any element in the sequence.");
					_first = () => throw new InvalidOperationException("There is no any element in the sequence.");
					_getByIndex = locIndex => throw new InvalidOperationException("There is no any element in the sequence.");
					_getEnumerator = () => __EmptyEnumerable.GetEnumerator();
				}
				else {
					_contains = locSearchItem => IndexOf(array: _underlyingArray, value: locSearchItem).HasValue;
					_copyTo = (d, i) => ((ICollection<T>)_underlyingArray).CopyTo(d, i);
					_count = () => _underlyingArray.Length;
					_isEmpty = () => _underlyingArray.Length < 1;
					_last =
						() => {
							try {
								return _underlyingArray[ _underlyingArray.GetUpperBound(0) ];
							}
							catch (IndexOutOfRangeException) {
								throw new InvalidOperationException("There is no any element in the sequence.");
							}
						};
					_first =
						() => {
							try {
								return _underlyingArray[ _underlyingArray.GetLowerBound(0) ];
							}
							catch (IndexOutOfRangeException) {
								throw new InvalidOperationException("There is no any element in the sequence.");
							}
						};
					_getByIndex = locIndex => _underlyingArray[ locIndex ];
					_getEnumerator = () => _underlyingArray.AsEnumerable().GetEnumerator();
				}
			}

			public bool IsEmpty
				=> _isEmpty();

			public int Count
				=> _count();

			public bool IsReadOnly
				=> true;

			public T Last
				=> _last();

			public T First
				=> _first();

			// TODO: Put strings into the resources.
			//
			public T this[ int index ] {
				get => _getByIndex(arg: index);
				set => throw new InvalidOperationException("It is impossible to clear collection. Collection is read only.");
			}

			// TODO: Put exception messages into the resources.
			//
			public void Add(T item)
				=> throw new InvalidOperationException("It is impossible to add element into collection. Collection is read only.");

			// TODO: Put exception messages into the resources.
			//
			public void Clear()
				=> throw new InvalidOperationException("It is impossible to clear collection. Collection is read only.");

			public bool Contains(T item)
				=> _contains(item);

			public void CopyTo(T[ ] array, int arrayIndex)
				=> _copyTo(array, arrayIndex);

			// TODO: Put exception messages into the resources.
			//
			public bool Remove(T item)
				=> throw new InvalidOperationException("It is impossible to remove element from collection. Collection is read only.");

			public IEnumerator<T> GetEnumerator()
				=> _getEnumerator();

			IEnumerator IEnumerable.GetEnumerator()
				=> GetEnumerator();

		}

		public delegate void ArrayItemUpdater<T>(int index, T source, out T updated);

		public delegate void ArrayItemUpdater2<T>(int index, ref T item);

		#endregion

		public static void AssertOneDimensionalArray(this Array array)
			=> array.EnsureNotNull(nameof(array)).EnsureOneDimensional();

		public static int? IndexOf<T>(this T[ ] array, Func<T, int, bool> predicate) {
			array.EnsureNotNull(nameof(array));
			predicate.EnsureNotNull(nameof(predicate));
			//
			var lowerBound = array.GetLowerBound(0);
			var upperBound = array.GetUpperBound(0);
			for (var i = lowerBound; i <= upperBound; i++) {
				if (predicate(array[ i ], i))
					return i;
			}
			return null;
		}

		public static int? IndexOf<T>(this T[ ] array, T value) {
			array.EnsureNotNull(nameof(array));
			//
			var eqComparer = EqualityComparer<T>.Default;
			var lowerBound = array.GetLowerBound(0);
			var upperBound = array.GetUpperBound(0);
			for (var i = lowerBound; i <= upperBound; i++) {
				if (eqComparer.Equals(x: array[ i ], y: value))
					return i;
			}
			return null;
		}

		public static void ForEach<T>(this T[ ] array, Action<T, int> iterationBody) {
			array.EnsureNotNull(nameof(array));
			iterationBody.EnsureNotNull(nameof(iterationBody));
			//
			var lowerBound = array.GetLowerBound(0);
			var arrayLength = array.Length;
			for (var offset = 0; offset < arrayLength; offset++) {
				var index = lowerBound + offset;
				iterationBody(array[ index ], index);
			}
		}

		public static void ForEach<T>(this T[ ] array, Action<T, bool, int> iterationBody) {
			array.EnsureNotNull(nameof(array));
			iterationBody.EnsureNotNull(nameof(iterationBody));
			//
			var lowerBound = array.GetLowerBound(0);
			var arrayLength = array.Length;
			for (var offset = 0; offset < arrayLength; offset++) {
				var index = lowerBound + offset;
				iterationBody(arg1: array[ index ], arg2: offset == 0, arg3: index);
			}
		}

		public static void ForEach<T>(this T[ ] array, Func<T, int, bool> iterationBody) {
			array.EnsureNotNull(nameof(array));
			iterationBody.EnsureNotNull(nameof(iterationBody));
			//
			var lowerBound = array.GetLowerBound(0);
			var arrayLength = array.Length;
			for (var offset = 0; offset < arrayLength; offset++) {
				var index = lowerBound + offset;
				if (iterationBody(array[ index ], index))
					break;
			}
		}

		public static bool IsNullOrEmpty(this Array array)
			=> array == null || array.Length < 1;

		public static ICollection<T> AsReadOnlyCollection<T>(this T[ ] array)
			=> new P_ArrayReadOnlyCollectionWrapper<T>(array);

		/// <summary>
		/// Wraps the specified <paramref name="array"/> into <see cref="ICountableEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="T">Array item type.</typeparam>
		/// <param name="array">
		/// Source array to be wrapped.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		public static ICountableEnumerable<T> AsCountableEnumerable<T>(this T[ ] array)
			=> new P_ArrayReadOnlyCollectionWrapper<T>(array);

		public static void ClearArray<T>(this T[ ] array) where T : class {
			if (array is null || array.Length < 1)
				return;
			else
				Array.Clear(array: array, index: array.GetLowerBound(0), length: array.Length);
		}

		public static T[ ] ConcatArray<T>(this T[ ] a, T[ ] b) {
			if (a is null && b is null)
				return null;
			else {
				int aLength, bLength;
				if (a is null || (aLength = a.GetLength(0)) < 1)
					return (T[ ])b?.Clone();
				else if (b is null || (bLength = b.GetLength(0)) < 1)
					return (T[ ])a?.Clone();
				else {
					var result = (T[ ])a.Clone();
					Array.Resize(array: ref result, newSize: aLength + bLength);
					Array.Copy(sourceArray: a, sourceIndex: a.GetLowerBound(0), destinationArray: result, destinationIndex: a.GetLowerBound(0), length: aLength);
					Array.Copy(sourceArray: b, sourceIndex: b.GetLowerBound(0), destinationArray: result, destinationIndex: a.GetLowerBound(0) + aLength, length: bLength);
					return result;
				}
			}
		}

		public static T[ ] ConcatArray<T>(T element, T[ ] array)
			=> ConcatArray(new T[ ] { element }, array);

		public static T[ ] ConcatArray<T>(T[ ] array, T element)
			=> ConcatArray(array, new T[ ] { element });

		public static T[ ] Sort<T>(this T[ ] array) {
			array.EnsureNotNull(nameof(array));
			//
			Array.Sort(array: array);
			return array;
		}

		public static T[ ] Reverse<T>(this T[ ] array) {
			if (array is null || array.Length < 2)
				return array;
			else {
				Array.Reverse(array);
				return array;
			}
		}

		/// <summary>
		/// Выполняет обновление всех элементов одномерного массива.
		/// <para>Метод возвращает тот же объект массива, что указан параметром <paramref name="array"/>.</para>
		/// </summary>
		/// <typeparam name="T">Тип элемента массива.</typeparam>
		/// <param name="array">
		/// Массив.
		/// <para>Может быть null.</para>
		/// </param>
		/// <param name="updater">
		/// Функция-делегат, выполняющая обновление элемента массива.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <returns>Исходный массив <paramref name="array"/> с обновленными элементами.</returns>
		public static T[ ] Update<T>(this T[ ] array, Func<T, int, T> updater) {
			updater.EnsureNotNull(nameof(updater));
			//
			if (!(array is null)) {
				var lowerBound = array.GetLowerBound(0);
				var upperBound = array.GetUpperBound(0);
				for (var i = lowerBound; i <= upperBound; i++)
					array[ i ] = updater(array[ i ], i);
			}
			return array;
		}

		public static void Update<T>(this T[ ] array, Action<T[ ], int> updater) {
			updater.EnsureNotNull(nameof(updater));
			//
			if (!(array is null)) {
				var lowerBound = array.GetLowerBound(0);
				var upperBound = array.GetUpperBound(0);
				for (var i = lowerBound; i <= upperBound; i++)
					updater(array, i);
			}
		}

		public static void Update<T>(this T[ ] array, ArrayItemUpdater2<T> updater) {
			updater.EnsureNotNull(nameof(updater));
			//
			if (!(array is null)) {
				var lowerBound = array.GetLowerBound(0);
				var upperBound = array.GetUpperBound(0);
				for (var i = lowerBound; i <= upperBound; i++)
					updater(index: i, item: ref array[ i ]);
			}
		}

		public static void Update<T>(this T[ ] array, ArrayItemUpdater<T> updater) {
			updater.EnsureNotNull(nameof(updater));
			//
			if (!(array is null)) {
				var lowerBound = array.GetLowerBound(0);
				var upperBound = array.GetUpperBound(0);
				for (var i = lowerBound; i <= upperBound; i++) {
					updater(index: i, source: array[ i ], updated: out var updated);
					array[ i ] = updated;
				}
			}
		}

		public static ArrayPartitionBoundary PartitionBoundary(this Array array) {
			array.EnsureNotNull(nameof(array)).EnsureOneDimensional();
			//
			return ArrayPartitionBoundary.Get(arrayLength: array.Length);
		}

		public static ArrayPartitionBoundary PartitionBoundary(this Array array, int offset, int count) {
			array.EnsureNotNull(nameof(array)).EnsureOneDimensional();
			//
			return ArrayPartitionBoundary.Get(arrayLength: array.Length, offset: offset, count: count);
		}

	}

}