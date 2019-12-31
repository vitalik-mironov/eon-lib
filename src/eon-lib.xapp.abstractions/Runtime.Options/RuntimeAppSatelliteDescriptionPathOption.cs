using Eon.Metadata;

namespace Eon.Runtime.Options {

	public sealed class RuntimeAppSatelliteDescriptionPathOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value: 'runtime-app.app'.
		/// </summary>
		public static readonly RuntimeAppSatelliteDescriptionPathOption Fallback;

		static RuntimeAppSatelliteDescriptionPathOption() {
			Fallback = new RuntimeAppSatelliteDescriptionPathOption(name: new MetadataPathName(path: "runtime-app.app"));
			RuntimeOptions.Option<RuntimeAppSatelliteDescriptionPathOption>.SetFallback(option: Fallback);
		}

		public static MetadataPathName Require()
			=> RuntimeOptions.Option<RuntimeAppSatelliteDescriptionPathOption>.Require().Name;

		#endregion

		readonly MetadataPathName _path;

		public RuntimeAppSatelliteDescriptionPathOption(MetadataPathName name) {
			name.EnsureNotNull(nameof(name));
			//
			_path = name;
		}

		public MetadataPathName Name
			=> _path;

	}

}