using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Eon.Description.Annotations;
using Eon.Metadata;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Description {

	[DataContract]
	[XInstanceContract(ContractType = typeof(XAppInitializationList<IXAppInitializationListDescription>), Sealed = false)]
	public class XAppInitializationListDescription
		:DescriptionBase, IXAppInitializationListDescription {

		[DataMember(Order = 0, Name = nameof(InitializableItems) + "RefSet", IsRequired = true)]
		MetadataReferenceSet<IDescription> _initializableItemsRefSet;

		public XAppInitializationListDescription(MetadataName name)
			: base(name: name) { }

		[JsonConstructor]
		protected XAppInitializationListDescription(SerializationContext ctx)
			: base(ctx: ctx) { }

		public IEnumerable<IDescription> InitializableItems {
			get => ReadDA(ref _initializableItemsRefSet).Resolve();
			set {
				EnsureNotReadOnly();
				throw new NotSupportedException().SetErrorCode(code: GeneralErrorCodes.Operation.NotSupported);
			}
		}

		protected override void OnDeserialized(StreamingContext context) {
			base.OnDeserialized(context);
			//
			ReadDA(ref _initializableItemsRefSet).Fluent().NullCond(locSet => RegisterReferenceSet(referenceSet: locSet, useDeserializationContext: true));
		}

		protected override void OnValidate() {
			base.OnValidate();
			//
			ReadDA(ref _initializableItemsRefSet).EnsureReachable();
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose)
				_initializableItemsRefSet?.Dispose();
			_initializableItemsRefSet = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}