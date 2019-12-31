namespace Eon.Runtime.Options {

	public sealed class DescriptionLocatorPackagePublisherScopeIdOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value: <see cref="UriBasedIdentifier.Undefined"/>.
		/// </summary>
		public static readonly DescriptionLocatorPackagePublisherScopeIdOption Fallback;

		static DescriptionLocatorPackagePublisherScopeIdOption() {
			Fallback = new DescriptionLocatorPackagePublisherScopeIdOption(publisherScopeId: UriBasedIdentifier.Undefined);
			RuntimeOptions.Option<DescriptionLocatorPackagePublisherScopeIdOption>.SetFallback(option: Fallback);
		}

		public static UriBasedIdentifier Require()
			=> RuntimeOptions.Option<DescriptionLocatorPackagePublisherScopeIdOption>.Require().PublisherScopeId;

		#endregion

		readonly UriBasedIdentifier _publisherScopeId;

		public DescriptionLocatorPackagePublisherScopeIdOption(UriBasedIdentifier publisherScopeId) {
			publisherScopeId.EnsureNotNull(nameof(publisherScopeId));
			//
			_publisherScopeId = publisherScopeId;
		}

		public UriBasedIdentifier PublisherScopeId
			=> _publisherScopeId;

	}

}