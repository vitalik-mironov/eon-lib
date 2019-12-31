using System;
using System.Collections.Generic;
using System.Linq;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Metadata {

	public static class MetadataReferenceUtilities {

		public static void EnsureReachable(this IMetadataReference reference, IMetadata @base) {
			if (!(reference is null))
				// TODO: Implement logic without factual resolution.
				//
				Resolve(reference, @base);
		}

		public static void EnsureReachable<TMetadata>(this IMetadataReference<TMetadata> reference, IMetadata @base)
			where TMetadata : class, IMetadata {
			if (!(reference is null))
				// TODO: Implement logic without factual resolution.
				//
				Resolve(reference, @base);
		}

		public static void EnsureReachable<TMetadata>(this MetadataReferenceSet<TMetadata> set)
			where TMetadata : class, IMetadata {
			if (set is null)
				return;
			else
				// TODO: Implement logic without factual resolution.
				//
				foreach (var resolvedMetadata in set.Resolve())
					continue;
		}

		public static IEnumerable<TMetadata> Resolve<TMetadata>(this MetadataReferenceSet<TMetadata> set)
				where TMetadata : class, IMetadata {
			if (set is null)
				return Enumerable.Empty<TMetadata>();
			else
				return set.Resolve();
		}

		static IMetadata P_Resolve(this IMetadataReference reference, IMetadata @base) {
			@base.EnsureNotNull(nameof(@base));
			//
			IMetadata result;
			if (reference == null) {
				result = null;
				@base.EnsureNotDisposeState();
			}
			else {
				var resolver = reference.GetResolver();
				if (resolver == null)
					throw new MetadataReferenceException(reference, @base, FormatXResource(typeof(MetadataReference), "NoPreferredResolver"));
				try {
					IMetadata resolvedMetadata;
					if (resolver.Resolve(@base, reference, out resolvedMetadata))
						result = resolvedMetadata;
					else
						throw
							new MetadataReferenceNotResolvedException(reference, @base);
					// TODO: Validate compliance.
					//
				}
				catch (Exception firstException) when (!(firstException is MetadataReferenceException)) {
					throw new MetadataReferenceNotResolvedException(reference: reference, @base: @base, innerException: firstException);
				}
			}
			return result;
		}

		public static IMetadata Resolve(this IMetadataReference reference, IMetadata @base)
			=> P_Resolve(reference, @base);

		public static TMetadata Resolve<TMetadata>(this IMetadataReference<TMetadata> reference, IMetadata @base)
			where TMetadata : class, IMetadata {
			@base.EnsureNotNull(nameof(@base));
			//
			var resolvedMetadata = P_Resolve(reference: reference, @base: @base);
			if (resolvedMetadata == null)
				return null;
			else {
				var result = resolvedMetadata as TMetadata;
				if (result == null)
					throw
						new MetadataNotCompliantReferenceException(reference, resolvedMetadata);
				else
					return result;
			}
		}

	}

}