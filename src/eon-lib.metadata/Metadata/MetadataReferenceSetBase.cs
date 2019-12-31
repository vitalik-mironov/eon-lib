using System.Collections.Generic;
using System.Runtime.Serialization;

using Eon.Metadata.Internal;
using Eon.Runtime.Serialization;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Metadata {

	[DataContract(Name = "MetadataRefSetBase")]
	public abstract class MetadataReferenceSetBase
		:CopyableDisposableBase {

		MetadataReferenceSetOwnershipRegistration _ownershipRegistration;

		protected MetadataReferenceSetBase() { }

		protected MetadataReferenceSetBase(SerializationContext ctx)
			: base(context: ctx) { }

		public abstract IEnumerable<IMetadataReference> Refs { get; }

		internal void SetOwnershipRegistration(MetadataReferenceSetOwnershipRegistration registration) {
			registration.EnsureNotNull(nameof(registration));
			//
			var original = WriteDA(location: ref _ownershipRegistration, value: registration, comparand: null);
			if (!(original is null || ReferenceEquals(objA: registration, objB: original)))
				throw new EonException();
		}

		public bool IsOwnershipRegistered {
			get {
				var ownershipRegistration = ReadDA(ref _ownershipRegistration);
				return ownershipRegistration != null && ownershipRegistration.IsRegistrationCompleted;
			}
		}

		public MetadataBase OwnerMetadata {
			get {
				var ownershipRegistration = ReadDA(ref _ownershipRegistration);
				if (ownershipRegistration == null || !ownershipRegistration.IsRegistrationCompleted)
					throw new EonException(FormatXResource(typeof(MetadataReferenceSetBase), "OwnershipNotRegistered"));
				return ownershipRegistration.Owner;
			}
		}

		public new MetadataReferenceSetBase CreateCopy(object args)
			=> (MetadataReferenceSetBase)base.CreateCopy(args);

		protected override void Dispose(bool explicitDispose) {
			_ownershipRegistration = null;
			//
			base.Dispose(explicitDispose);
		}

		internal abstract void EnsureReferencesReachable();

	}

}