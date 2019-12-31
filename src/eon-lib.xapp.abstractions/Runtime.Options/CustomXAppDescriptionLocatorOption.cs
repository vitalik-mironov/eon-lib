using Eon.Description;
using Eon.Metadata;

namespace Eon.Runtime.Options {

	public sealed class CustomXAppDescriptionLocatorOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value:
		/// <para><see cref="DescriptionLocator.DescriptionPath"/> = 'custom-app.app';</para>
		/// <para><see cref="DescriptionPackageLocator.PackageName"/> = 'custom-app';</para>
		/// <para><see cref="DescriptionPackageLocator.PackagePublisherScopeId"/> = <see cref="UriBasedIdentifier.Undefined"/>.</para>
		/// </summary>
		public static readonly CustomXAppDescriptionLocatorOption Fallback;

		static CustomXAppDescriptionLocatorOption() {
			Fallback = new CustomXAppDescriptionLocatorOption(locator: new DescriptionLocator(descriptionPath: (MetadataPathName)"custom-app.app", packageName: (MetadataName)"custom-app", packagePublisherScopeId: UriBasedIdentifier.Undefined));
			RuntimeOptions.Option<CustomXAppDescriptionLocatorOption>.SetFallback(option: Fallback);
		}

		public static DescriptionLocator Require()
			=> RuntimeOptions.Option<CustomXAppDescriptionLocatorOption>.Require().Locator;

		#endregion

		readonly DescriptionLocator _locator;

		public CustomXAppDescriptionLocatorOption(DescriptionLocator locator) {
			locator.EnsureNotNull(nameof(locator));
			//
			_locator = locator;
		}

		public DescriptionLocator Locator
			=> _locator;

	}

}