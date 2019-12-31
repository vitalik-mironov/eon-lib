using System;

using Eon.Description;

namespace Eon.Context {

	public class DescriptionPackageLoadContext
		:MetadataLoadContextBase, IDescriptionPackageLoadContext {

		DescriptionPackageLocator _locator;

		public DescriptionPackageLoadContext(
			DescriptionPackageLocator locator,
			Uri loadUri,
			bool skipLinkedMetadata,
			string mediaType = default,
			Uri siteOrigin = default,
			XFullCorrelationId fullCorrelationId = default,
			ArgumentPlaceholder<XCorrelationId> correlationId = default,
			object localTag = default,
			IContext outerCtx = default)
			: base(skipLinkedMetadata: skipLinkedMetadata, baseUri: loadUri, mediaType: mediaType, siteOrigin: siteOrigin, fullCorrelationId: fullCorrelationId, correlationId: correlationId, localTag: localTag, outerCtx: outerCtx) {
			//
			locator.EnsureNotNull(nameof(locator));
			//
			_locator = locator;
		}

		public DescriptionPackageLocator Locator
			=> ReadDA(ref _locator);

		// TODO: Put strings into the resources.
		//
		public override string ToString()
			=> base.ToString() + $"{Environment.NewLine}Locator:{_locator.FmtStr().GNLI()}";

		protected override void Dispose(bool explicitDispose) {
			_locator = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}