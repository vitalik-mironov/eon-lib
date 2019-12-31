using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Eon.Linq;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Collections {

	public sealed class ListReadOnlyWrap<T>
		:IReadOnlyList<T>, IList<T> {

		#region Static members

		public static readonly ListReadOnlyWrap<T> Empty = new ListReadOnlyWrap<T>();

		static readonly IEqualityComparer<T> __DefaultEqualityComparer = System.Collections.Generic.EqualityComparer<T>.Default;

		public static ListReadOnlyWrap<T> WrapOrEmpty(IList<T> list) {
			if (list is null)
				throw new ArgumentNullException(paramName: nameof(list));
			else if (list.Count == 0)
				return Empty;
			else
				return new ListReadOnlyWrap<T>(list: list);
		}

		#endregion

		readonly IList<T> _readWriteUnderlyingList;

		readonly IReadOnlyList<T> _readOnlyUnderlyingList;

		readonly Func<int> _countDelegate;

		readonly Func<T, bool> _containsDelegate;

		readonly Action<T[ ], int> _copyToDelegate;

		readonly Func<IEnumerator<T>> _getEnumeratorDelegate;

		readonly Func<T, int> _indexOfDelegate;

		readonly Func<int, T> _itemGetter;

		public ListReadOnlyWrap() {
			_readWriteUnderlyingList = new List<T>(capacity: 0);
			//
			P_CtorInitializerForReadWriteUnderlyingList(
				countDelegate: ref _countDelegate,
				containsDelegate: ref _containsDelegate,
				copyToDelegate: ref _copyToDelegate,
				getEnumeratorDelegate: ref _getEnumeratorDelegate,
				indexOfDelegate: ref _indexOfDelegate,
				itemGetter: ref _itemGetter);
		}

		public ListReadOnlyWrap(IEnumerable<T> collection) {
			collection.EnsureNotNull(nameof(collection));
			//
			_readWriteUnderlyingList = new List<T>(collection: collection);
			//
			P_CtorInitializerForReadWriteUnderlyingList(
				countDelegate: ref _countDelegate,
				containsDelegate: ref _containsDelegate,
				copyToDelegate: ref _copyToDelegate,
				getEnumeratorDelegate: ref _getEnumeratorDelegate,
				indexOfDelegate: ref _indexOfDelegate,
				itemGetter: ref _itemGetter);
		}

		public ListReadOnlyWrap(IList<T> list) {
			list.EnsureNotNull(nameof(list));
			//
			_readWriteUnderlyingList = list;
			//
			P_CtorInitializerForReadWriteUnderlyingList(countDelegate: ref _countDelegate, containsDelegate: ref _containsDelegate, copyToDelegate: ref _copyToDelegate, getEnumeratorDelegate: ref _getEnumeratorDelegate, indexOfDelegate: ref _indexOfDelegate, itemGetter: ref _itemGetter);
		}

		public ListReadOnlyWrap(IReadOnlyList<T> readOnlyList) {
			readOnlyList.EnsureNotNull(nameof(readOnlyList));
			//
			_readOnlyUnderlyingList = readOnlyList;
			//
			P_CtorInitializerForReadOnlyUnderlyingList(
				countDelegate: ref _countDelegate,
				containsDelegate: ref _containsDelegate,
				copyToDelegate: ref _copyToDelegate,
				getEnumeratorDelegate: ref _getEnumeratorDelegate,
				indexOfDelegate: ref _indexOfDelegate,
				itemGetter: ref _itemGetter);
		}

		void P_CtorInitializerForReadWriteUnderlyingList(
			ref Func<int> countDelegate,
			ref Func<T, bool> containsDelegate,
			ref Action<T[ ], int> copyToDelegate,
			ref Func<IEnumerator<T>> getEnumeratorDelegate,
			ref Func<T, int> indexOfDelegate,
			ref Func<int, T> itemGetter) {
			//
			countDelegate = () => _readWriteUnderlyingList.Count;
			containsDelegate = _readWriteUnderlyingList.Contains;
			copyToDelegate = _readWriteUnderlyingList.CopyTo;
			getEnumeratorDelegate = _readWriteUnderlyingList.GetEnumerator;
			indexOfDelegate = _readWriteUnderlyingList.IndexOf;
			itemGetter = (locIndex) => _readWriteUnderlyingList[ locIndex ];
		}

		void P_CtorInitializerForReadOnlyUnderlyingList(
			ref Func<int> countDelegate,
			ref Func<T, bool> containsDelegate,
			ref Action<T[ ], int> copyToDelegate,
			ref Func<IEnumerator<T>> getEnumeratorDelegate,
			ref Func<T, int> indexOfDelegate,
			ref Func<int, T> itemGetter) {
			//
			countDelegate = () => _readOnlyUnderlyingList.Count;
			containsDelegate = locItem => _readOnlyUnderlyingList.Contains(value: locItem);
			copyToDelegate =
				(array, arrayIndex) => {
					// TODO: Put strings into the resources.
					//
					array.EnsureNotNull(nameof(array));
					if (array.GetLowerBound(0) < 0)
						throw
							new ArgumentException(
								paramName: nameof(array),
								message: "Нижняя граница массива выходит за допустимый предел. Допустимый предел: '0'.");
					arrayIndex
						.Arg(nameof(arrayIndex))
						.EnsureNotLessThanZero();
					//
					var sourceCount = _readOnlyUnderlyingList.Count;
					var destinationLength = array.Length;
					for (var sourceIndex = 0; sourceIndex < sourceCount; sourceIndex++) {
						var destinationIndex = sourceIndex + arrayIndex;
						if (destinationIndex < destinationLength)
							array[ destinationIndex ] = _readOnlyUnderlyingList[ sourceIndex ];
						else
							throw
								new ArgumentException(
									paramName: nameof(array),
									message: FormatXResource(typeof(Array), "TooSmall"));
					}
				};
			getEnumeratorDelegate = _readOnlyUnderlyingList.GetEnumerator;
			indexOfDelegate = locItemX => _readOnlyUnderlyingList.IndexOf(predicate: locItemY => __DefaultEqualityComparer.Equals(x: locItemX, y: locItemY));
			itemGetter = (locIndex) => _readOnlyUnderlyingList[ locIndex ];
		}

		public T this[ int index ] {
			get => _itemGetter(index);
			set => throw new InvalidOperationException(FormatXResource(typeof(InvalidOperationException), "CanNotModifyObjectDueReadOnly", this));
		}

		public int Count
			=> _countDelegate();

		public bool IsReadOnly
			=> true;

		public void Add(T item) => throw new InvalidOperationException(FormatXResource(typeof(InvalidOperationException), "CanNotModifyObjectDueReadOnly", this));

		public void Clear() => throw new InvalidOperationException(FormatXResource(typeof(InvalidOperationException), "CanNotModifyObjectDueReadOnly", this));

		public bool Contains(T item)
			=> _containsDelegate(item);

		public void CopyTo(T[ ] array, int arrayIndex)
			=> _copyToDelegate(array, arrayIndex);

		public IEnumerator<T> GetEnumerator()
			=> _getEnumeratorDelegate();

		public int IndexOf(T item)
			=> _indexOfDelegate(item);

		public void Insert(int index, T item) { throw new InvalidOperationException(FormatXResource(typeof(InvalidOperationException), "CanNotModifyObjectDueReadOnly", this)); }

		public bool Remove(T item) { throw new InvalidOperationException(FormatXResource(typeof(InvalidOperationException), "CanNotModifyObjectDueReadOnly", this)); }

		public void RemoveAt(int index) {
			index.Arg(nameof(index)).EnsureNotLessThanZero();
			//
			throw new InvalidOperationException(FormatXResource(typeof(InvalidOperationException), "CanNotModifyObjectDueReadOnly", this));
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

	}

}