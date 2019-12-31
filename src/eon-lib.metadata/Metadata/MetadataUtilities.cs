using System;
using System.Diagnostics;

namespace Eon.Metadata {

	public static class MetadataUtilities {

		[Obsolete(message: "Use '" + nameof(ArgumentUtilities) + "' classes instead.", error: false)]
		public static MetadataPropertyHandle<TMetadata, TProperty> Property<TMetadata, TProperty>(this TMetadata metadata, TProperty propertyValue, string propertyName)
			where TMetadata : class, IMetadata
			=> new MetadataPropertyHandle<TMetadata, TProperty>(metadata: metadata, propertyName: propertyName, propertyValue: propertyValue);

		public static bool IsSelfOrDescendantOf(this IMetadata descendant, IMetadata ancestor) {
			descendant.EnsureNotNull(nameof(descendant));
			ancestor.EnsureNotNull(nameof(ancestor));
			//
			if (ReferenceEquals(ancestor, descendant))
				return true;
			else {
				var parent = descendant.Parent;
				for (; !(parent is null);) {
					if (ReferenceEquals(parent, ancestor))
						return true;
					else
						parent = parent.Parent;
				}
				return false;
			}
		}

	}

}