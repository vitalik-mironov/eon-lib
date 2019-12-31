using System;
using System.Collections.Generic;

namespace Eon.Metadata {

	public sealed class MetadataNameEqualityComparer
		:IEqualityComparer<MetadataName> {

		public static readonly MetadataNameEqualityComparer Instance = new MetadataNameEqualityComparer();

		public MetadataNameEqualityComparer() { }

		bool IEqualityComparer<MetadataName>.Equals(MetadataName x, MetadataName y) 
			=> MetadataName.Equals(x, y);

		int IEqualityComparer<MetadataName>.GetHashCode(MetadataName obj) {
			if (obj is null)
				throw new ArgumentNullException(nameof(obj));
			else
				return obj.GetHashCode();
		}

	}

}