using System;
using System.Collections.Generic;

namespace Eon.Data.Persistence {

	public readonly struct PersistenceEntityReferenceKeyTypeDescriptor
		:IEquatable<PersistenceEntityReferenceKeyTypeDescriptor> {

		#region Nested types

		sealed class P_EqualityComparer
			:IEqualityComparer<PersistenceEntityReferenceKeyTypeDescriptor> {

			#region Static members

			public static readonly P_EqualityComparer Instance = new P_EqualityComparer();

			#endregion

			P_EqualityComparer() { }

			public bool Equals(PersistenceEntityReferenceKeyTypeDescriptor a, PersistenceEntityReferenceKeyTypeDescriptor b)
				=> a.Equals(other: b);

			public int GetHashCode(PersistenceEntityReferenceKeyTypeDescriptor value)
				=> value.GetHashCode();

		}

		#endregion

		#region Static members

		public static readonly IEqualityComparer<PersistenceEntityReferenceKeyTypeDescriptor> EqualityComparer = P_EqualityComparer.Instance;

		public static bool operator ==(PersistenceEntityReferenceKeyTypeDescriptor a, PersistenceEntityReferenceKeyTypeDescriptor b)
			=> a.Equals(other: b);

		public static bool operator !=(PersistenceEntityReferenceKeyTypeDescriptor a, PersistenceEntityReferenceKeyTypeDescriptor b)
			=> !a.Equals(other: b);

		public static implicit operator PersistenceEntityReferenceKeyTypeDescriptor((Type referenceKeyType, Type entityType) value)
			=> new PersistenceEntityReferenceKeyTypeDescriptor(entityType: value.entityType, referenceKeyType: value.referenceKeyType);

		#endregion

		public readonly Type EntityType;

		public readonly Type ReferenceKeyType;

		public PersistenceEntityReferenceKeyTypeDescriptor(Type entityType, Type referenceKeyType) {
			EntityType = entityType;
			ReferenceKeyType = referenceKeyType;
		}

		public override bool Equals(object other) {
			if (other is PersistenceEntityReferenceKeyTypeDescriptor info)
				return Equals(other: info);
			else
				return false;
		}

		public bool Equals(PersistenceEntityReferenceKeyTypeDescriptor other) {
			if (other.EntityType is null) {
				if (EntityType is null)
					return true;
				else
					return false;
			}
			else if (EntityType is null)
				return false;
			else
				return EntityType.Equals(o: other.EntityType) && ReferenceKeyType.Equals(o: other.ReferenceKeyType);
		}

		public override int GetHashCode()
			=> (EntityType?.GetHashCode() ?? 0) ^ (ReferenceKeyType?.GetHashCode() ?? 0);

	}

}