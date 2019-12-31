using System;
using System.Runtime.Serialization;

using Eon.Metadata;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.ComponentModel.Dependencies.Description {

	[DataContract]
	public class DependencyBindingDescription
		:DependencyBindingDescriptionBase, IDependencyBindingDescription {

		[DataMember(Order = 0, Name = nameof(DependencyTarget), IsRequired = true)]
		DependencyTarget _dependencyTarget;

		public DependencyBindingDescription(MetadataName name)
			: base(name) { }

		[JsonConstructor]
		protected DependencyBindingDescription(SerializationContext ctx)
			: base(ctx: ctx) { }

		public DependencyTarget DependencyTarget
			=> ReadDA(ref _dependencyTarget);

		IDependencyTarget IDependencyBindingDescription.DependencyTarget
			=> DependencyTarget;

		protected override void OnSetReadOnly(ReadOnlyStateTag previousState, ReadOnlyStateTag newState) {
			this.EnsureChangeToPermanentReadOnly(newState: newState);
			//
			base.OnSetReadOnly(previousState, newState);
			//
			if (newState.IsReadOnly)
				WriteDA(location: ref _dependencyTarget, value: ReadDA(ref _dependencyTarget)?.AsReadOnly());
		}

		// TODO: Put strings into the resources.
		//
		protected override void OnValidate() {
			base.OnValidate();
			//
			var target = this.ArgProp(DependencyTarget, nameof(DependencyTarget)).EnsureNotNull().Value;
			//
			foreach (var dependencyId in DependencyIdSet) {
				if (!dependencyId.IsTargetMatch(target))
					throw
						new MetadataValidationException(
							message: $"Указанная цель байндинга (привязки) функциональной зависимости (свойство '{nameof(DependencyTarget)}') не соответствует одному из указанных идентификаторов зависимости (свойство '{nameof(DependencyIdSet)}').{Environment.NewLine}\tЦель байндинга:{target.FmtStr().GNLI2()}{Environment.NewLine}\tИдентификатор зависимости:{dependencyId.FmtStr().GNLI2()}",
							metadata: this);
			}
		}

		public sealed override IVh<IDependencyHandler2> GetDependencyHandler(object dependencyScopeContext = null) {
			EnsureValidated();
			//
			return
				DependencyTarget
				.GetDependencyHandler(dependencyScopeContext: dependencyScopeContext);
		}

		protected override void Dispose(bool explicitDispose) {
			_dependencyTarget = null;
			base.Dispose(explicitDispose);
		}

	}

}