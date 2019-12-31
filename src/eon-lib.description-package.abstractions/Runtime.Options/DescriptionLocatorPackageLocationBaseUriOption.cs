using System;

namespace Eon.Runtime.Options {

	public sealed class DescriptionLocatorPackageLocationBaseUriOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value: <see cref="LocationBaseUri"/> = <see langword="null"/>.
		/// </summary>
		public static readonly DescriptionLocatorPackageLocationBaseUriOption Fallback;

		static DescriptionLocatorPackageLocationBaseUriOption() {
			Fallback = new DescriptionLocatorPackageLocationBaseUriOption(locationBaseUri: null);
			RuntimeOptions.Option<DescriptionLocatorPackageLocationBaseUriOption>.SetFallback(option: Fallback);
		}

		public static Uri Require()
			=> RuntimeOptions.Option<DescriptionLocatorPackageLocationBaseUriOption>.Require().LocationBaseUri;

		#endregion

		readonly Uri _locationBaseUri;

		public DescriptionLocatorPackageLocationBaseUriOption(Uri locationBaseUri) {
			_locationBaseUri = locationBaseUri;
		}

		public Uri LocationBaseUri
			=> _locationBaseUri;

	}

}