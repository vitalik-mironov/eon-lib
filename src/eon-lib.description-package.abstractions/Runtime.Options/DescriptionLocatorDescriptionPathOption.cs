using Eon.Metadata;

namespace Eon.Runtime.Options {

	public sealed class DescriptionLocatorDescriptionPathOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value: 'default'.
		/// </summary>
		public static readonly DescriptionLocatorDescriptionPathOption Fallback;

		static DescriptionLocatorDescriptionPathOption() {
			Fallback = new DescriptionLocatorDescriptionPathOption(path: new MetadataPathName(path: "default"));
			RuntimeOptions.Option<DescriptionLocatorDescriptionPathOption>.SetFallback(option: Fallback);
		}

		public static MetadataPathName Require()
			=> RuntimeOptions.Option<DescriptionLocatorDescriptionPathOption>.Require().Path;

		#endregion

		readonly MetadataPathName _path;

		public DescriptionLocatorDescriptionPathOption(MetadataPathName path) {
			path.EnsureNotNull(nameof(path));
			//
			_path = path;
		}

		public MetadataPathName Path
			=> _path;

	}

}