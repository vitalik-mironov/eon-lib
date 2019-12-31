using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.ComponentModel.Dependencies {

	[DataContract]
	public abstract class DependencyId
		:AsReadOnlyValidatableBase,	IDependencyId {

		[JsonConstructor]
		protected DependencyId(SerializationContext context) { }

		protected DependencyId(bool isReadOnly = false)
			: base(isReadOnly: isReadOnly) { }

		public abstract bool IsTargetMatch(IDependencyTarget target);

		public bool Equals(IDependencyId other)
			=> Equals(other as DependencyId);

		public virtual bool Equals(DependencyId other)
			=> this == other;

		public override bool Equals(object other)
			=> Equals(other as DependencyId);

		public override int GetHashCode()
			=> base.GetHashCode();

		protected sealed override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase readOnlyCopy) {
			DependencyId locReadOnlyCopy;
			CreateReadOnlyCopy(out locReadOnlyCopy);
			readOnlyCopy = locReadOnlyCopy;
		}

		protected abstract void CreateReadOnlyCopy(out DependencyId readOnlyCopy);

		public new DependencyId AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				DependencyId readOnlyCopy;
				CreateReadOnlyCopy(out readOnlyCopy);
				return readOnlyCopy;
			}
		}

		IDependencyId IAsReadOnlyMethod<IDependencyId>.AsReadOnly()
			=> AsReadOnly();

		protected override void OnValidate() { }

	}

}