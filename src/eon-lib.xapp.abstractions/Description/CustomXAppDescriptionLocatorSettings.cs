using System.Runtime.Serialization;
using Eon.Runtime.Options;
using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Description {

	[DataContract]
	public sealed class CustomXAppDescriptionLocatorSettings
		:AsReadOnlyValidatableBase, ISettings, IAsReadOnly<CustomXAppDescriptionLocatorSettings> {

		#region Nested types

		public static CustomXAppDescriptionLocatorSettings GetDefault()
			=> new CustomXAppDescriptionLocatorSettings(locator: CustomXAppDescriptionLocatorOption.Require());

		#endregion

		[DataMember(Order = 0, Name = nameof(IsDisabled), IsRequired = false)]
		bool _isDisabled;

		[DataMember(Order = 1, Name = nameof(LocatorString), IsRequired = true)]
		string _locatorString;

		DescriptionLocator _locator;

		public CustomXAppDescriptionLocatorSettings(DescriptionLocator locator, bool isReadOnly)
			: base(isReadOnly: isReadOnly, isValid: isReadOnly) {
			locator.EnsureNotNull(nameof(locator));
			//
			_locator = isReadOnly ? locator : null;
			_locatorString = locator.Serialize();
		}

		public CustomXAppDescriptionLocatorSettings(DescriptionLocator locator)
			: base(isReadOnly: true, isValid: true) {
			locator.EnsureNotNull(nameof(locator));
			//
			_locator = locator;
			_locatorString = locator.Serialize();
		}

		public CustomXAppDescriptionLocatorSettings()
			: this(isDisabled: false) { }

		public CustomXAppDescriptionLocatorSettings(bool isDisabled = default, string locatorString = default, bool isReadOnly = default)
			: base(isReadOnly: isReadOnly) {
			_isDisabled = isDisabled;
			_locatorString = locatorString;
		}

		public CustomXAppDescriptionLocatorSettings(CustomXAppDescriptionLocatorSettings other, bool isReadOnly = default)
			: this(isDisabled: other.EnsureNotNull(nameof(other)).Value.IsDisabled, locatorString: other.LocatorString, isReadOnly: isReadOnly) {
			if (other.IsReadOnly && isReadOnly)
				_locator = itrlck.Get(location: ref other._locator);
		}

		public bool IsDisabled {
			get => _isDisabled;
			set {
				EnsureNotReadOnly();
				_isDisabled = value;
			}
		}

		public string LocatorString {
			get => _locatorString;
			set {
				EnsureNotReadOnly();
				_locatorString = value;
			}
		}

		protected sealed override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase copy) {
			P_CreateReadOnlyCopy(copy: out var locCopy);
			copy = locCopy;
		}

		void P_CreateReadOnlyCopy(out CustomXAppDescriptionLocatorSettings copy)
			=> copy = new CustomXAppDescriptionLocatorSettings(other: this, isReadOnly: true);

		protected override void OnValidate() {
			var locator = default(DescriptionLocator);
			LocatorString
				.ArgProp(nameof(LocatorString))
				.EnsureNotNull()
				.EnsureValid(validator: locValue => DescriptionLocator.Parse(data: locValue, locator: out locator, useDefaults: false));
			if (IsReadOnly && !(locator is null))
				itrlck.Set(location: ref _locator, value: locator);
		}

		public DescriptionLocator GetLocator() {
			var locator = itrlck.Get(location: ref _locator);
			if (locator is null) {
				var readOnlyCopy = AsReadOnly();
				readOnlyCopy.Validate();
				locator = itrlck.Get(location: ref readOnlyCopy._locator);
				if (locator is null)
					DescriptionLocator.Parse(data: readOnlyCopy.LocatorString, locator: out locator, useDefaults: false);
			}
			return locator;
		}

		public new CustomXAppDescriptionLocatorSettings AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				P_CreateReadOnlyCopy(copy: out var copy);
				return copy;
			}
		}

	}

}