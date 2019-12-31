using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Eon.Description.Annotations;
using Eon.Metadata;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Triggers.Description {

	[DataContract]
	[XInstanceContract(ContractType = typeof(AggregateTrigger<IAggregateTriggerDescription>), Sealed = false)]
	public class AggregateTriggerDescription
		:TriggerDescriptionBase, IAggregateTriggerDescription {

		[DataMember(Order = 0, Name = nameof(Triggers), IsRequired = true)]
		MetadataReferenceSet<ITriggerDescription> _triggers;

		public AggregateTriggerDescription(MetadataName name)
			: base(name) { }

		[JsonConstructor]
		protected AggregateTriggerDescription(SerializationContext ctx)
			: base(ctx: ctx) { }

		public IEnumerable<ITriggerDescription> Triggers {
			get => EnumerateDA(value: ReadDA(ref _triggers).Resolve());
			set {
				EnsureNotReadOnly();
				throw new NotSupportedException().SetErrorCode(code: GeneralErrorCodes.Operation.NotSupported);
			}
		}

		protected override void OnValidate() {
			base.OnValidate();
			//
			this.ArgProp(ReadDA(ref _triggers), nameof(Triggers)).EnsureReachable();
		}

		protected override void OnDeserialized(StreamingContext context) {
			base.OnDeserialized(context);
			//
			ReadDA(ref _triggers).Fluent().NullCond(locTriggers => RegisterReferenceSet(locTriggers, true));
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_triggers?.Dispose();
			}
			_triggers = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}