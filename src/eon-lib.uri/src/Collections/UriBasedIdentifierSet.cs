using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Eon.Linq;
using Eon.Runtime.Serialization;

namespace Eon.Collections {

	[CollectionDataContract(ItemName = CollectionDataContractItemName)]
	public class UriBasedIdentifierSet
		:AsReadOnlyBase, ISet<UriBasedIdentifier> {

		public const string CollectionDataContractItemName = "Id";

		HashSet<UriBasedIdentifier> _underlyingSet;

		public UriBasedIdentifierSet()
			: this(collection: null) { }

		public UriBasedIdentifierSet(IEnumerable<UriBasedIdentifier> collection = null, IEqualityComparer<UriBasedIdentifier> comparer = null, bool isReadOnly = false)
			: base(isReadOnly: isReadOnly) {
			_underlyingSet = new HashSet<UriBasedIdentifier>(collection: collection.EmptyIfNull(), comparer: comparer);
		}

		public UriBasedIdentifierSet(ISet<UriBasedIdentifier> other, IEqualityComparer<UriBasedIdentifier> comparer = null, bool isReadOnly = false)
			: base(isReadOnly: isReadOnly) {
			other.EnsureNotNull(nameof(other));
			//
			_underlyingSet = new HashSet<UriBasedIdentifier>(collection: other, comparer: comparer ?? (other as HashSet<UriBasedIdentifier>)?.Comparer);
		}

		public UriBasedIdentifierSet(UriBasedIdentifierSet other, IEqualityComparer<UriBasedIdentifier> comparer = null, bool isReadOnly = false)
			: base(isReadOnly: isReadOnly) {
			other.EnsureNotNull(nameof(other));
			//
			_underlyingSet = new HashSet<UriBasedIdentifier>(collection: other, comparer: comparer ?? other._underlyingSet.Comparer);
		}

		protected UriBasedIdentifierSet(SerializationContext ctx, IEnumerable<UriBasedIdentifier> identifiers)
			: base(ctx: ctx) {
			var locIdentifiers = identifiers.EnsureNotNull(nameof(identifiers)).EnsureNoNullElements().Value;
			//
			_underlyingSet = new HashSet<UriBasedIdentifier>(collection: locIdentifiers);
		}

		protected override void CreateReadOnlyCopy(out AsReadOnlyBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locReadOnlyCopy);
			readOnlyCopy = locReadOnlyCopy;
		}

		protected virtual void CreateReadOnlyCopy(out UriBasedIdentifierSet readOnlyCopy)
			=> readOnlyCopy = new UriBasedIdentifierSet(other: this, isReadOnly: true);

		public new UriBasedIdentifierSet AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				CreateReadOnlyCopy(out var readOnlyCopy);
				return readOnlyCopy;
			}
		}

		public bool Add(UriBasedIdentifier item) {
			EnsureNotReadOnly();
			return _underlyingSet.Add(item);
		}

		public int Count
			=> _underlyingSet.Count;

		public void UnionWith(IEnumerable<UriBasedIdentifier> other) {
			EnsureNotReadOnly();
			_underlyingSet.UnionWith(other);
		}

		public void IntersectWith(IEnumerable<UriBasedIdentifier> other) {
			EnsureNotReadOnly();
			_underlyingSet.IntersectWith(other);
		}

		public void ExceptWith(IEnumerable<UriBasedIdentifier> other) {
			EnsureNotReadOnly();
			_underlyingSet.ExceptWith(other);
		}

		public void SymmetricExceptWith(IEnumerable<UriBasedIdentifier> other) {
			EnsureNotReadOnly();
			_underlyingSet.SymmetricExceptWith(other);
		}

		public bool IsSubsetOf(IEnumerable<UriBasedIdentifier> other)
			=> _underlyingSet.IsSubsetOf(other);

		public bool IsSupersetOf(IEnumerable<UriBasedIdentifier> other)
			=> _underlyingSet.IsSupersetOf(other);

		public bool IsProperSupersetOf(IEnumerable<UriBasedIdentifier> other)
			=> _underlyingSet.IsProperSupersetOf(other);

		public bool IsProperSubsetOf(IEnumerable<UriBasedIdentifier> other)
			=> _underlyingSet.IsProperSubsetOf(other);

		public bool Overlaps(IEnumerable<UriBasedIdentifier> other)
			=> _underlyingSet.Overlaps(other);

		public bool SetEquals(IEnumerable<UriBasedIdentifier> other)
			=> _underlyingSet.SetEquals(other);

		void ICollection<UriBasedIdentifier>.Add(UriBasedIdentifier item)
			=> Add(item);

		public void Clear() {
			EnsureNotReadOnly();
			_underlyingSet.Clear();
		}

		public bool Contains(UriBasedIdentifier item)
			=> _underlyingSet.Contains(item);

		public void CopyTo(UriBasedIdentifier[ ] array, int arrayIndex)
			=> _underlyingSet.CopyTo(array, arrayIndex);

		public bool Remove(UriBasedIdentifier item) {
			EnsureNotReadOnly();
			return _underlyingSet.Remove(item);
		}

		public IEnumerator<UriBasedIdentifier> GetEnumerator()
			=> _underlyingSet.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> ((IEnumerable)_underlyingSet).GetEnumerator();

	}

}