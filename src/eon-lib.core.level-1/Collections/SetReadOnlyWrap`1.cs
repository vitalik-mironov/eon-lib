using System;
using System.Collections;
using System.Collections.Generic;

namespace Eon.Collections {

	public sealed class SetReadOnlyWrap<T>
		:IReadOnlySet<T> {

		#region Static members

		public static readonly SetReadOnlyWrap<T> Empty = new SetReadOnlyWrap<T>();

		// TODO: Put strings into the resources.
		//
		static Exception P_SetReadOnlyException(SetReadOnlyWrap<T> set)
			=> new InvalidOperationException(message: $"Невозможно изменить набор. Набор только для чтения.{Environment.NewLine}\tНабор:{set.FmtStr().GNLI2()}");

		#endregion

		readonly ISet<T> _underlyingSet;

		public SetReadOnlyWrap() {
			_underlyingSet = new HashSet<T>();
		}

		public SetReadOnlyWrap(ISet<T> set) {
			set.EnsureNotNull(nameof(set));
			//
			_underlyingSet = set;
		}

		public int Count { get { return _underlyingSet.Count; } }

		public bool IsReadOnly { get { return true; } }

		public bool Contains(T item) { return _underlyingSet.Contains(item); }

		public void CopyTo(T[ ] array, int arrayIndex) { _underlyingSet.CopyTo(array, arrayIndex); }

		public IEnumerator<T> GetEnumerator() { return _underlyingSet.GetEnumerator(); }

		public bool IsProperSubsetOf(IEnumerable<T> other) { return _underlyingSet.IsProperSubsetOf(other); }

		public bool IsProperSupersetOf(IEnumerable<T> other) { return _underlyingSet.IsProperSupersetOf(other); }

		public bool IsSubsetOf(IEnumerable<T> other) { return _underlyingSet.IsSubsetOf(other); }

		public bool IsSupersetOf(IEnumerable<T> other) { return _underlyingSet.IsSupersetOf(other); }

		public bool Overlaps(IEnumerable<T> other) { return _underlyingSet.Overlaps(other); }

		public bool SetEquals(IEnumerable<T> other) { return _underlyingSet.SetEquals(other); }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public bool Add(T item) { throw P_SetReadOnlyException(this); }

		public void Clear() { throw P_SetReadOnlyException(this); }

		public void ExceptWith(IEnumerable<T> other) { throw P_SetReadOnlyException(this); }

		public void IntersectWith(IEnumerable<T> other) { throw P_SetReadOnlyException(this); }

		public bool Remove(T item) { throw P_SetReadOnlyException(this); }

		public void SymmetricExceptWith(IEnumerable<T> other) { throw P_SetReadOnlyException(this); }

		public void UnionWith(IEnumerable<T> other) { throw P_SetReadOnlyException(this); }

		void ICollection<T>.Add(T item) { throw P_SetReadOnlyException(this); }

	}

}