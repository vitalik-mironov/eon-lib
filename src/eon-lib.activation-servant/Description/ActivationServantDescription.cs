using System;
using System.Runtime.Serialization;

using Eon.Description.Annotations;
using Eon.Metadata;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Description {

	[DataContract]
	[XInstanceContract(ContractType = typeof(ActivationServant<IActivationServantDescription>), Sealed = false)]
	public class ActivationServantDescription
		:DescriptionBase, IActivationServantDescription {

		[DataMember(Order = 1, Name = nameof(IsAutoActivationEnabled), IsRequired = true)]
		bool _isAutoActivationEnabled;

		[DataMember(Order = 2, Name = nameof(Component), IsRequired = true)]
		MetadataReference<IDescription> _component;

		[DataMember(Order = 3, Name = nameof(RetryOptions), IsRequired = true)]
		RetrySettings _retryOptions;

		public ActivationServantDescription(MetadataName name)
			: base(name: name) { }

		[JsonConstructor]
		protected ActivationServantDescription(SerializationContext ctx)
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

		public RetrySettings RetryOptions {
			get => ReadDA(ref _retryOptions);
			set {
				EnsureNotReadOnly();
				WriteDA(ref _retryOptions, value);
			}
		}

		protected override void OnSetReadOnly(ReadOnlyStateTag previousState, ReadOnlyStateTag newState) {
			this.EnsureChangeToPermanentReadOnly(newState);
			//
			base.OnSetReadOnly(previousState, newState);
			//
			if (newState.IsReadOnly)
				WriteDA(location: ref _retryOptions, value: ReadDA(ref _retryOptions)?.AsReadOnly());
		}

		protected override void OnValidate() {
			base.OnValidate();
			//
			this.ArgProp(Component, nameof(Component)).EnsureNotNull();
			//
			this.ArgProp(RetryOptions, nameof(RetryOptions)).EnsureNotNull().EnsureValid();
		}

		protected override void Dispose(bool explicitDispose) {
			_component = null;
			_retryOptions = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}