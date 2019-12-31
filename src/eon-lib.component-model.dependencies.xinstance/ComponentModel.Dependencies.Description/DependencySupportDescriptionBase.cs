using System.Runtime.Serialization;

using Eon.Description;
using Eon.Metadata;
using Eon.Runtime.Serialization;

namespace Eon.ComponentModel.Dependencies.Description {

	[DataContract]
	public abstract class DependencySupportDescriptionBase
		:DescriptionBase, IDependencySupportDescription {

		[DataMember(Order = 0, Name = nameof(Dependencies), IsRequired = false, EmitDefaultValue = false)]
		MetadataReference<IDependenciesDescription> _dependencies;

		protected DependencySupportDescriptionBase(MetadataName name)
			: base(name) { }

		protected DependencySupportDescriptionBase(SerializationContext ctx)
			: base(ctx: ctx) { }

		public IDependenciesDescription Dependencies
			=> ReadDA(ref _dependencies).Resolve(this);

		protected override void OnValidate() {
			base.OnValidate();
			//
			ReadDA(ref _dependencies).EnsureReachable(this);
		}

		protected override void Dispose(bool explicitDispose) {
			_dependencies = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}