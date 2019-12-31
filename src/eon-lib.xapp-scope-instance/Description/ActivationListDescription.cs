using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Eon.Description.Annotations;
using Eon.Metadata;
using Eon.Runtime.Serialization;

namespace Eon.Description {

	[DataContract]
	[XInstanceContract(ContractType = typeof(ActivationList<IActivationListDescription>), Sealed = false)]
	public class ActivationListDescription
		:DescriptionBase, IActivationListDescription {

		[DataMember(Order = 0, Name = nameof(OmitItemsActivation), IsRequired = true)]
		bool _omitItemsActivation;

		[DataMember(Order = 1, Name = nameof(ActivatableItems), IsRequired = true)]
		MetadataReferenceSet<IDescription> _activatableItems;

		public ActivationListDescription(MetadataName name)
			: base(name) {
			_omitItemsActivation = false;
		}

		protected ActivationListDescription(SerializationContext ctx)
			: base(ctx: ctx) { }

		public bool OmitItemsActivation {
			get => ReadDA(ref _omitItemsActivation);
			set {
				EnsureNotReadOnly();
				_omitItemsActivation = value;
			}
		}

		public IEnumerable<IDescription> ActivatableItems {
			get => ReadDA(ref _activatableItems).Resolve();
			set {
				EnsureNotReadOnly();
				throw new NotSupportedException().SetErrorCode(code: GeneralErrorCodes.Operation.NotSupported);
			}
		}

		protected override void OnDeserialized(StreamingContext ctx) {
			base.OnDeserialized(ctx);
			//
			ReadDA(ref _activatableItems).Fluent().NullCond(locSet => RegisterReferenceSet(referenceSet: locSet, useDeserializationContext: true));
		}

		protected override void OnValidate() {
			base.OnValidate();
			//
			ReadDA(ref _activatableItems).EnsureReachable();
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose)
				_activatableItems?.Dispose();
			_activatableItems = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}