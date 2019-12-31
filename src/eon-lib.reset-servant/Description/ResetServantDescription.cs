using System;
using System.Runtime.Serialization;

using Eon.Description.Annotations;
using Eon.Metadata;
using Eon.Runtime.Serialization;
using Eon.Triggers.Description;

using Newtonsoft.Json;

namespace Eon.Description {

	[DataContract]
	[XInstanceContract(ContractType = typeof(ResetServant<IResetServantDescription>), Sealed = false)]
	public class ResetServantDescription
		:DescriptionBase, IResetServantDescription {

		[DataMember(Order = 0, Name = nameof(IsAutoActivationEnabled), IsRequired = true)]
		bool _isAutoActivationEnabled;

		[DataMember(Order = 1, Name = nameof(Component), IsRequired = true)]
		MetadataReference<IDescription> _component;

		[DataMember(Order = 2, Name = nameof(ResetTrigger), IsRequired = true)]
		MetadataReference<IAggregateTriggerDescription> _resetTrigger;

		[DataMember(Order = 3, Name = nameof(ResetFailureResponseCode), IsRequired = true)]
		ResetServantResetFailureResponseCode _resetFailureResponseCode;

		[DataMember(Order = 4, Name = nameof(ResetFailureProgram), IsRequired = true)]
		string _resetFailureProgram;

		[DataMember(Order = 5, Name = nameof(ResetFailureProgramArgs), IsRequired = true)]
		string _resetFailureProgramArgs;

		public ResetServantDescription(MetadataName name)
			: base(name) { }

		[JsonConstructor]
		protected ResetServantDescription(SerializationContext ctx)
			: base(ctx: ctx) { }

		public bool IsAutoActivationEnabled {
			get => ReadDA(ref _isAutoActivationEnabled);
			set {
				EnsureNotReadOnly();
				_isAutoActivationEnabled = value;
			}
		}

		public IDescription Component {
			get => ReadDA(ref _component).Resolve(this);
			set {
				EnsureNotReadOnly();
				throw new NotSupportedException();
			}
		}

		public IAggregateTriggerDescription ResetTrigger {
			get => ReadDA(ref _resetTrigger).Resolve(this);
			set {
				EnsureNotReadOnly();
				throw new NotSupportedException();
			}
		}

		public ResetServantResetFailureResponseCode ResetFailureResponseCode {
			get => ReadDA(ref _resetFailureResponseCode);
			set {
				EnsureNotReadOnly();
				_resetFailureResponseCode = value;
			}
		}

		public string ResetFailureProgram {
			get => ReadDA(ref _resetFailureProgram);
			set {
				EnsureNotReadOnly();
				WriteDA(ref _resetFailureProgram, value);
			}
		}

		public string ResetFailureProgramArgs {
			get => ReadDA(ref _resetFailureProgramArgs);
			set {
				EnsureNotReadOnly();
				WriteDA(ref _resetFailureProgramArgs, value);
			}
		}

		// TODO: Put strings into the resources.
		//
		protected override void OnValidate() {
			base.OnValidate();
			//
			this.ArgProp(Component, nameof(Component)).EnsureNotNull();
			this.ArgProp(ResetTrigger, nameof(ResetTrigger)).EnsureNotNull();
			this.ArgProp(ResetFailureProgram, nameof(ResetFailureProgram)).EnsureHasMaxLength(maxLength: 260); // — max. allowed length of the path (on Windows).
			this.ArgProp(ResetFailureProgramArgs, nameof(ResetFailureProgramArgs)).EnsureHasMaxLength(maxLength: 32699 - (ResetFailureProgram?.Length ?? 0)); // — max. allowed length of the command line (including path) (on Windows 7+).
			switch (ResetFailureResponseCode) {
				case ResetServantResetFailureResponseCode.RunProgram:
				case ResetServantResetFailureResponseCode.RunProgramThenShutdownApp:
					this.ArgProp(ResetFailureProgram, nameof(ResetFailureProgram)).EnsureNotNull().EnsureNotEmptyOrWhiteSpace();
					break;
				case ResetServantResetFailureResponseCode.None:
					break;
				default:
					throw
						new MetadataValidationException(
							message: $"Указано недопустимое значение свойства '{nameof(ResetFailureResponseCode)}'.{Environment.NewLine}Указанный код корректирующего действия не поддерживается.{Environment.NewLine}\tКод:{ResetFailureResponseCode.FmtStr().GNLI2()}",
							metadata: this);
			}
		}

		protected override void Dispose(bool explicitDispose) {
			_component = null;
			_resetFailureProgram = null;
			_resetFailureProgramArgs = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}