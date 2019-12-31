using System;
using System.Runtime.Serialization;

using Eon.Description.Annotations;
using Eon.Metadata;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Triggers.Description {

	[DataContract]
	[XInstanceContract(ContractType = typeof(FrequencyLimitTrigger<IFrequencyLimitTriggerDescription>), Sealed = false)]
	public class FrequencyLimitTriggerDescription
		:AggregateTriggerDescription, IFrequencyLimitTriggerDescription {

		[DataMember(Order = 0, Name = nameof(SignalFrequencyLimit), IsRequired = true)]
		TimeSpan _signalFrequencyLimit;

		public FrequencyLimitTriggerDescription(MetadataName name)
			: base(name) { }

		[JsonConstructor]
		protected FrequencyLimitTriggerDescription(SerializationContext ctx)
			: base(ctx) { }

		public TimeSpan SignalFrequencyLimit {
			get => ReadDA(ref _signalFrequencyLimit);
			set {
				EnsureNotReadOnly();
				_signalFrequencyLimit = value;
			}
		}

		protected override void OnValidate() {
			base.OnValidate();
			//
			this.ArgProp(SignalFrequencyLimit, nameof(SignalFrequencyLimit)).EnsureNotLessThan(operand: TimeSpan.Zero);
		}

	}

}