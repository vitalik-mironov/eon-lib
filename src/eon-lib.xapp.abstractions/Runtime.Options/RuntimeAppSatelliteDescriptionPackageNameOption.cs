using Eon.Metadata;

namespace Eon.Runtime.Options {

	public sealed class RuntimeAppSatelliteDescriptionPackageNameOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value: 'runtime-app'.
		/// </summary>
		public static readonly RuntimeAppSatelliteDescriptionPackageNameOption Fallback;

		static RuntimeAppSatelliteDescriptionPackageNameOption() {
			Fallback = new RuntimeAppSatelliteDescriptionPackageNameOption(name: new MetadataName(name: "runtime-app"));
			RuntimeOptions.Option<RuntimeAppSatelliteDescriptionPackageNameOption>.SetFallback(option: Fallback);
		}

		public static MetadataName Require()
			=> RuntimeOptions.Option<RuntimeAppSatelliteDescriptionPackageNameOption>.Require().Name;

		#endregion

		readonly MetadataName _name;

		public RuntimeAppSatelliteDescriptionPackageNameOption(MetadataName name) {
			name.EnsureNotNull(nameof(name));
			//
			_name = name;
		}

		public MetadataName Name
			=> _name;

	}

}