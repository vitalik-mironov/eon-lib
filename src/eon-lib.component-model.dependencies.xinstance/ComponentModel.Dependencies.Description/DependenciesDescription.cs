using System.Collections.Generic;
using System.Runtime.Serialization;

using Eon.Description;
using Eon.Metadata;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.ComponentModel.Dependencies.Description {

	[DataContract]
	public class DependenciesDescription
		:DescriptionBase, IDependenciesDescription {

		[DataMember(Order = 0, Name = nameof(BindingsGroups), IsRequired = true)]
		MetadataReferenceSet<IDependenciesBindingsGroupDescription> _dependenciesBindingsGroups;

		protected DependenciesDescription(MetadataName name)
			: base(name) { }

		[JsonConstructor]
		protected DependenciesDescription(SerializationContext context)
			: base(context) { }

		public IEnumerable<IDependenciesBindingsGroupDescription> BindingsGroups
			=> ReadDA(ref _dependenciesBindingsGroups).Resolve();

		protected override void OnValidate() {
			base.OnValidate();
			//
			ReadDA(ref _dependenciesBindingsGroups).EnsureReachable();
		}

		protected override void OnDeserialized(StreamingContext context) {
			base.OnDeserialized(context);
			//
			ReadDA(ref _dependenciesBindingsGroups).Fluent().NullCond(action: locReferenceSet => RegisterReferenceSet(referenceSet: locReferenceSet, useDeserializationContext: true));
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose)
				_dependenciesBindingsGroups?.Dispose();
			_dependenciesBindingsGroups = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}