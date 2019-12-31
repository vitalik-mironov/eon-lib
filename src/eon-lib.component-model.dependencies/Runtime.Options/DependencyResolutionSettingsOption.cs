using Eon.ComponentModel.Dependencies;

namespace Eon.Runtime.Options {

	public sealed class DependencyResolutionSettingsOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value:
		/// <para><see cref="IDependencyResolutionSettings.IsAdvancedLoggingEnabled"/>: <see langword="false"/></para>
		/// <para><see cref="IDependencyResolutionSettings.UsageCountLimitPerHandler"/>: <see langword="1"/></para>
		/// </summary>
		public static readonly DependencyResolutionSettingsOption Fallback;

		static DependencyResolutionSettingsOption() {
			Fallback = new DependencyResolutionSettingsOption(settings: new DependencyResolutionSettings(usageCountLimitPerHandler: 1, isAdvancedLoggingEnabled: false, isReadOnly: true));
			RuntimeOptions.Option<DependencyResolutionSettingsOption>.SetFallback(option: Fallback);
		}

		public static IDependencyResolutionSettings Require()
			=> RuntimeOptions.Option<DependencyResolutionSettingsOption>.Require().Settings;

		#endregion

		IDependencyResolutionSettings _settings;

		public DependencyResolutionSettingsOption(IDependencyResolutionSettings settings) {
			settings.EnsureNotNull(nameof(settings)).EnsureReadOnly().EnsureNotDisabled().EnsureValid();
			//
			_settings = settings;
		}

		public IDependencyResolutionSettings Settings
			=> _settings;

	}

}