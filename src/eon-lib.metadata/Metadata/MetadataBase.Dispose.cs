namespace Eon.Metadata {

	public partial class MetadataBase {

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				var referenceSetRegistry = _referenceSetRegistry;
				var referenceSetRegistrySpinLock = _referenceSetRegistrySpinLock;
				if (!(referenceSetRegistrySpinLock is null))
					referenceSetRegistrySpinLock.EnterAndExitLock();
				if (!(referenceSetRegistry is null)) {
					referenceSetRegistry.ForEach(action: locItem => locItem?.Dispose());
					referenceSetRegistry.Clear();
				}
				//
				_dependencySupport?.Dispose();
			}
			_treeElementLink = null;
			_name = null;
			_referenceSetRegistry = null;
			_referenceSetRegistrySpinLock = null;
			_dependencySupport = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}