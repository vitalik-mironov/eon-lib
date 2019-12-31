using System;

namespace Eon.Description {

	public sealed class DescriptionSummary
		:IDescriptionSummary {

		readonly Guid _guid;

		readonly string _fullName;

		readonly DescriptionPackageIdentity _packageIdentity;

		readonly Uri _packageSiteOrigin;

		readonly string _displayName;

		public DescriptionSummary(Guid guid, string fullName, string displayName, DescriptionPackageIdentity packageIdentity, Uri packageSiteOrigin) {
			guid.Arg(nameof(guid)).EnsureNotEmpty();
			fullName.EnsureNotNull(nameof(fullName)).EnsureNotEmpty();
			displayName.EnsureNotNull(nameof(displayName)).EnsureNotEmpty();
			//
			_guid = guid;
			_fullName = fullName;
			_displayName = displayName;
			_packageIdentity = packageIdentity;
			_packageSiteOrigin = packageSiteOrigin;
		}

		public Guid Guid
			=> _guid;

		public string FullName
			=> _fullName;

		public string DisplayName
			=> _displayName;

		public DescriptionPackageIdentity PackageIdentity
			=> _packageIdentity;

		public Uri PackageSiteOrigin
			=> _packageSiteOrigin;

	}

}