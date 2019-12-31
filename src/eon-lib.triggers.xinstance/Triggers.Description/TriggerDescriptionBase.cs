using System.Runtime.Serialization;

using Eon.Description;
using Eon.Metadata;
using Eon.Runtime.Serialization;

namespace Eon.Triggers.Description {

	[DataContract]
	public abstract class TriggerDescriptionBase
		:DescriptionBase, ITriggerDescription {

		[DataMember(Order = 1, Name = nameof(SignalOnActivation), IsRequired = true)]
		bool _signalOnActivation;

		[DataMember(Order = 2, Name = nameof(SignalOnFirstActivationOnly), IsRequired = true)]
		bool _signalOnFirstActivationOnly;

		protected TriggerDescriptionBase(MetadataName name)
			: base(name) { }

		protected TriggerDescriptionBase(SerializationContext ctx)
			: base(ctx: ctx) { }

		public bool SignalOnActivation {
			get => ReadDA(ref _signalOnActivation);
			set {
				EnsureNotReadOnly();
				_signalOnActivation = value;
			}
		}

		public bool SignalOnFirstActivationOnly {
			get => ReadDA(ref _signalOnFirstActivationOnly);
			set {
				EnsureNotReadOnly();
				_signalOnFirstActivationOnly = value;
			}
		}

		// TODO: Put strings into the resources.
		//
		protected override void OnValidate() {
			base.OnValidate();
			//
			if (SignalOnFirstActivationOnly && !SignalOnActivation)
				throw
					new MetadataValidationException(
						message: $"Недопустимое значение '{bool.TrueString}' свойства '{nameof(SignalOnFirstActivationOnly)}'. Значение свойства не может быть '{bool.TrueString}', когда значение свойства '{nameof(SignalOnActivation)}' равно '{bool.FalseString}'.",
						metadata: this);
		}

	}

}