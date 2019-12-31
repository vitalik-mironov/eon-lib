using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.ComponentModel.Dependencies {

	[DataContract]
	public class DependencyResolutionSettings
		:AsReadOnlyValidatableBase, IDependencyResolutionSettings, IAsReadOnly<DependencyResolutionSettings> {

		[DataMember(Order = 0, Name = nameof(UsageCountLimitPerHandler), IsRequired = true, EmitDefaultValue = true)]
		int _usageCountLimitPerHandler;

		[DataMember(Order = 1, Name = nameof(IsAdvancedLoggingEnabled), IsRequired = true, EmitDefaultValue = true)]
		bool _isAdvancedLoggingEnabled;

		public DependencyResolutionSettings()
			: this(usageCountLimitPerHandler: default) { }

		public DependencyResolutionSettings(int usageCountLimitPerHandler = default, bool isAdvancedLoggingEnabled = default, bool isReadOnly = default)
			: base(isReadOnly: isReadOnly) {
			//
			_usageCountLimitPerHandler = usageCountLimitPerHandler;
			_isAdvancedLoggingEnabled = isAdvancedLoggingEnabled;
		}

		public DependencyResolutionSettings(IDependencyResolutionSettings other, bool isReadOnly = false)
			: this(usageCountLimitPerHandler: other.EnsureNotNull(nameof(other)).Value.UsageCountLimitPerHandler, isAdvancedLoggingEnabled: other.IsAdvancedLoggingEnabled, isReadOnly: isReadOnly) { }

		[JsonConstructor]
		protected DependencyResolutionSettings(SerializationContext ctx)
			: base(ctx: ctx) { }

		bool IAbilityOption.IsDisabled
			=> false;

		public int UsageCountLimitPerHandler {
			get => _usageCountLimitPerHandler;
			set {
				EnsureNotReadOnly();
				_usageCountLimitPerHandler = value;
			}
		}

		public bool IsAdvancedLoggingEnabled {
			get => _isAdvancedLoggingEnabled;
			set {
				EnsureNotReadOnly();
				_isAdvancedLoggingEnabled = value;
			}
		}

		protected sealed override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase copy) {
			CreateReadOnlyCopy(out var locCopy);
			copy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out DependencyResolutionSettings copy)
			=> copy = new DependencyResolutionSettings(other: this, isReadOnly: true);

		public new DependencyResolutionSettings AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				CreateReadOnlyCopy(out var сopy);
				return сopy;
			}
		}

		IDependencyResolutionSettings IAsReadOnlyMethod<IDependencyResolutionSettings>.AsReadOnly()
			=> AsReadOnly();

		protected override void OnValidate()
			=> UsageCountLimitPerHandler.ArgProp(name: nameof(UsageCountLimitPerHandler)).EnsureNotLessThan(operand: 1);

	}

}