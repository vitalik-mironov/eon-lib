using System.Collections.Generic;
using System.Runtime.Serialization;

using Eon.Description;
using Eon.Linq;
using Eon.Metadata;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.ComponentModel.Dependencies.Description {

	[DataContract]
	public class DependenciesBindingsGroupDescription
		:DescriptionBase, IDependenciesBindingsGroupDescription {

		[DataMember(Order = 1, Name = nameof(Bindings), IsRequired = true)]
		DependencyBinding[ ] _bindings;

		[DataMember(Order = 2, Name = nameof(BindingsDescriptions), IsRequired = true)]
		MetadataReferenceSet<IDependencyBindingDescriptionBase> _bindingsDescriptions;

		public DependenciesBindingsGroupDescription(Metadata.MetadataName name)
			: base(name) { }

		[JsonConstructor]
		protected DependenciesBindingsGroupDescription(SerializationContext context)
			: base(context) { }

		public IEnumerable<DependencyBinding> Bindings
			=> EnumerateDA(ref _bindings);

		IEnumerable<IDependencyBinding> IDependenciesBindingsGroupDescription.Bindings
			=> Bindings;

		public IEnumerable<IDependencyBindingDescriptionBase> BindingsDescriptions
			=> ReadDA(ref _bindingsDescriptions).Resolve();

		protected override void OnSetReadOnly(ReadOnlyStateTag previousState, ReadOnlyStateTag newState) {
			this.EnsureChangeToPermanentReadOnly(newState);
			//
			base.OnSetReadOnly(previousState, newState);
			//
			if (newState.IsReadOnly)
				ReadDA(ref _bindings).Update((bindings, i) => WriteDA(ref bindings[ i ], bindings[ i ]?.AsReadOnly()));
		}

		protected override void OnValidate() {
			base.OnValidate();
			//
			this
				.ArgProp(Bindings, nameof(Bindings))
				.EnsureValid()
				.Execute();
			//
			ReadDA(ref _bindingsDescriptions).EnsureReachable();
		}

		protected override void OnDeserialized(StreamingContext context) {
			base.OnDeserialized(context);
			//
			ReadDA(ref _bindingsDescriptions)
				.Fluent().NullCond(bindingsDescriptionsRefSet => RegisterReferenceSet(bindingsDescriptionsRefSet, true));
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_bindings.ClearArray();
				_bindingsDescriptions?.Dispose();
			}
			_bindings = null;
			_bindingsDescriptions = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}