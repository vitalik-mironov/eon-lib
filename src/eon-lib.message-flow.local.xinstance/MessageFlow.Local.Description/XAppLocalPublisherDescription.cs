using System;
using System.Runtime.Serialization;

using Eon.Description;
using Eon.Description.Annotations;
using Eon.Metadata;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.MessageFlow.Local.Description {

	[DataContract]
	[XInstanceContract(ContractType = typeof(XAppLocalPublisher<IXAppLocalPublisherDescription>), Sealed = false)]
	public class XAppLocalPublisherDescription
		:DescriptionBase, IXAppLocalPublisherDescription {

		[DataMember(Order = 0, Name = nameof(UseDefaultSettings), IsRequired = true)]
		bool _useDefaultSettings;

		[DataMember(Order = 1, Name = nameof(CustomSettings), IsRequired = true)]
		LocalPublisherSettings _customSettings;

		public XAppLocalPublisherDescription(MetadataName name)
			: base(name: name) {
			_useDefaultSettings = true;
		}

		[JsonConstructor]
		protected XAppLocalPublisherDescription(SerializationContext ctx)
			: base(ctx: ctx) { }

		public bool UseDefaultSettings {
			get => ReadDA(ref _useDefaultSettings);
			set {
				EnsureNotReadOnly();
				_useDefaultSettings = value;
			}
		}

		public ILocalPublisherSettings CustomSettings {
			get => ReadDA(ref _customSettings);
			set {
				EnsureNotReadOnly();
				throw new NotSupportedException();
			}
		}

		protected override void OnSetReadOnly(ReadOnlyStateTag previousState, ReadOnlyStateTag newState) {
			this.EnsureChangeToPermanentReadOnly(newState: newState);
			//
			base.OnSetReadOnly(previousState, newState);
			//
			if (newState.IsReadOnly)
				WriteDA(location: ref _customSettings, value: ReadDA(location: ref _customSettings)?.AsReadOnly());
		}

		protected override void OnValidate() {
			base.OnValidate();
			//
			if (!UseDefaultSettings) {
				this
					.ArgProp(CustomSettings, nameof(CustomSettings))
					.EnsureNotNull()
					.EnsureValid();
			}
		}

		protected override void Dispose(bool explicitDispose) {
			_customSettings = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}