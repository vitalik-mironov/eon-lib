using System.Runtime.Serialization;

namespace Eon.ComponentModel.Dependencies {

	[DataContract]
	public abstract class DependencyTarget
		:AsReadOnlyValidatableBase, IDependencyTarget {

		protected DependencyTarget(bool isReadOnly = false)
			: base(isReadOnly: isReadOnly) { }

		public abstract IVh<IDependencyHandler2> GetDependencyHandler(object dependencyScopeContext = null);

		protected sealed override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locReadOnlyCopy);
			readOnlyCopy = locReadOnlyCopy;
		}

		protected abstract void CreateReadOnlyCopy(out DependencyTarget readOnlyCopy);

		public new DependencyTarget AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				DependencyTarget readOnlyCopy;
				CreateReadOnlyCopy(out readOnlyCopy);
				return readOnlyCopy;
			}
		}

		IDependencyTarget IAsReadOnlyMethod<IDependencyTarget>.AsReadOnly()
			=> AsReadOnly();

	}

}