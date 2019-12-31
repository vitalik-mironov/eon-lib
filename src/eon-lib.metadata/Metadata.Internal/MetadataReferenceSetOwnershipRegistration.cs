using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Metadata.Internal {

	internal sealed class MetadataReferenceSetOwnershipRegistration
		:Disposable {

		MetadataBase _owner;

		MetadataReferenceSetBase _set;

		int _helpFlags;

		public MetadataReferenceSetOwnershipRegistration(MetadataBase owner, MetadataReferenceSetBase set) {
			owner.EnsureNotNull(nameof(owner));
			set.EnsureNotNull(nameof(set));
			//
			_helpFlags = 0;
			_owner = owner;
			_set = set;
			set.SetOwnershipRegistration(registration: this);
		}

		public MetadataBase Owner
			=> ReadDA(ref _owner);

		public MetadataReferenceSetBase Set
			=> ReadDA(ref _set);

		internal bool IsRegistrationCompleted
			=> (itrlck.Get(location: ref _helpFlags) & 0x1) == 0x1;

		internal void SetRegistrationCompleteFlag()
			=> itrlck.Or(location: ref _helpFlags, value: 1);

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose && IsRegistrationCompleted)
				_set?.Dispose();
			_set = null;
			_owner = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}