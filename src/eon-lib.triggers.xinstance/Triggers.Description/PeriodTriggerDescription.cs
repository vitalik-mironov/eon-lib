using System;
using System.Runtime.Serialization;

using Eon.Description.Annotations;
using Eon.Metadata;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Triggers.Description {

	[DataContract]
	[XInstanceContract(ContractType = typeof(PeriodTrigger<IPeriodTriggerDescription>), Sealed = false)]
	public class PeriodTriggerDescription
		:TriggerDescriptionBase, IPeriodTriggerDescription {

		[DataMember(Order = 0, Name = nameof(Period), IsRequired = true)]
		TimeSpan _period;

		[DataMember(Order = 1, Name = nameof(PeriodVariance), IsRequired = true)]
		TimeSpan _periodVariance;

		public PeriodTriggerDescription(MetadataName name)
			: base(name) { }

		[JsonConstructor]
		protected PeriodTriggerDescription(SerializationContext ctx)
			: base(ctx: ctx) { }

		public TimeSpan Period {
			get => ReadDA(ref _period);
			set {
				EnsureNotReadOnly();
				_period = value;
			}
		}

		public TimeSpan PeriodVariance {
			get => ReadDA(ref _periodVariance);
			set {
				EnsureNotReadOnly();
				_periodVariance = value;
			}
		}

		// TODO: Put strings into the resources.
		//
		protected override void OnValidate() {
			base.OnValidate();
			//
			var period = Period;
			this.ArgProp(period, nameof(Period)).EnsureNotGreaterThan(operand: TriggerUtilities.PeriodMax).EnsureNotLessThan(operand: TriggerUtilities.PeriodMin).EnsureHasMaxScaleOfMilliSeconds();
			//
			var periodVariance = PeriodVariance;
			this.ArgProp(periodVariance, nameof(PeriodVariance)).EnsureAbsNotGreaterThan(operand: TriggerUtilities.PeriodMax).EnsureHasMaxScaleOfMilliSeconds();
			if (periodVariance.IsNegative() && period.Add(periodVariance) < TriggerUtilities.PeriodMin)
				throw
					new MetadataValidationException(
						$"Указано недопустимое значение '{periodVariance.ToString("c")}' свойства '{nameof(PeriodVariance)}', так как результат сложения указанного значения и значения свойства '{nameof(Period)}' меньше допустимого значения '{TriggerUtilities.PeriodMin.ToString("c")}'.", metadata: this);
			else if (periodVariance.IsPositive() && period.Add(periodVariance) > TriggerUtilities.PeriodMax)
				throw
					new MetadataValidationException(message: $"Указано недопустимое значение '{periodVariance.ToString("c")}' свойства '{nameof(PeriodVariance)}', так как результат сложения указанного значения и значения свойства '{nameof(Period)}' больше допустимого значения '{TriggerUtilities.PeriodMin.ToString("c")}'.", metadata: this);
		}

	}

}