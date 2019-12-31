using System.Runtime.Serialization;

using Eon.Description;
using Eon.Metadata;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.ComponentModel.Dependencies.Description {

	[DataContract]
	public class DependencyResolutionSettingsDescription
		:DescriptionBase, IDependencyResolutionSettingsDescription {

		[DataMember(Order = 0, Name = nameof(Settings), IsRequired = true)]
		DependencyResolutionSettings _settings;

		public DependencyResolutionSettingsDescription(MetadataName name)
			: base(name) { }

		[JsonConstructor]
		protected DependencyResolutionSettingsDescription(SerializationContext context)
			: base(context) { }

		public IDependencyResolutionSettings Settings
			=> ReadDA(ref _settings);

		protected override void OnSetReadOnly(ReadOnlyStateTag previousState, ReadOnlyStateTag newState) {
			this.EnsureChangeToPermanentReadOnly(newState: newState);
			//
			base.OnSetReadOnly(previousState, newState);
			//
			if (newState.IsReadOnly)
				WriteDA(ref _settings, ReadDA(ref _settings)?.AsReadOnly());
		}

		protected override void OnValidate() {
			base.OnValidate();
			//
			this.ArgProp(Settings, nameof(Settings)).EnsureNotNull().EnsureValid();
		}

		protected override void Dispose(bool explicitDispose) {
			_settings = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}