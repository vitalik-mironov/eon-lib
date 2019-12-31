using Eon.Metadata;

namespace Eon.Runtime.Options {

	public sealed class DescriptionLocatorPackageNameOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value: 'default'.
		/// </summary>
		public static readonly DescriptionLocatorPackageNameOption Fallback;

		static DescriptionLocatorPackageNameOption() {
			Fallback = new DescriptionLocatorPackageNameOption(name: new MetadataName(name: "default"));
			RuntimeOptions.Option<DescriptionLocatorPackageNameOption>.SetFallback(option: Fallback);
		}

		public static MetadataName Require()
			=> RuntimeOptions.Option<DescriptionLocatorPackageNameOption>.Require().Name;

		#endregion

		readonly MetadataName _name;

		public DescriptionLocatorPackageNameOption(MetadataName name) {
			name.EnsureNotNull(nameof(name));
			//
			_name = name;
		}

		public MetadataName Name
			=> _name;

	}

}