using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Eon.Linq;

namespace Eon.Metadata {

	public sealed class MetadataPathNameRefResolver
		:Disposable, IMetadataReferenceResolver {

		#region Static members

		static readonly MetadataPathNameRefResolver __Instance = new MetadataPathNameRefResolver();

		public static MetadataPathNameRefResolver Instance
			=> __Instance;

		#endregion

		MetadataPathNameRefResolver() { }

		public bool Resolve(IMetadata @base, IMetadataReference reference, out IMetadata metadata) {
			@base.EnsureNotNull(nameof(@base));
			var pathNameReference =
				reference
				.EnsureNotNull(nameof(reference))
				.EnsureOfType<IMetadataReference, IMetadataPathNameReference>()
				.Value;
			//
			var requiredMetadataType = pathNameReference.TargetMetadataType;
			var name = pathNameReference.Name;
			var firstNameComponent = true;
			var currentSearchPlace = @base;
			var locMetadata = default(IMetadata);
			foreach (var currentComponentName in name.Components) {
				if (firstNameComponent) {
					locMetadata =
						currentSearchPlace
						.Sequence()
						.Concat(currentSearchPlace.Parent.Sequence())
						.Concat(currentSearchPlace.Siblings)
						.Concat(currentSearchPlace.Children)
						.Concat(currentSearchPlace.Ancestors.Skip(1))
						.Where(i => MetadataName.Equals(i?.Name, currentComponentName))
						.FirstOrDefault();
					firstNameComponent = false;
				}
				else
					locMetadata =
						currentSearchPlace
						.Children
						.Where(i => MetadataName.Equals(i.Name, currentComponentName))
						.FirstOrDefault();
				if (locMetadata == null)
					break;
				else
					currentSearchPlace = locMetadata;
			}
			if (locMetadata == null) {
				metadata = null;
				return false;
			}
			else if (requiredMetadataType.IsAssignableFrom(locMetadata.GetType())) {
				metadata = locMetadata;
				return true;
			}
			else
				throw new MetadataNotCompliantReferenceException(reference, locMetadata);
		}

	}

}