using System;

namespace Eon.Context {

	public abstract class MetadataLoadContextBase
		:SubjectLoadContextBase, IMetadataLoadContext {

		readonly bool _skipLinkedMetadata;

		protected MetadataLoadContextBase(
			bool skipLinkedMetadata,
			Uri baseUri,
			string mediaType = default,
			Uri siteOrigin = default,
			XFullCorrelationId fullCorrelationId = default,
			ArgumentPlaceholder<XCorrelationId> correlationId = default,
			object localTag = default,
			IContext outerCtx = default)
			: base(baseUri: baseUri, mediaType: mediaType, siteOrigin: siteOrigin, fullCorrelationId: fullCorrelationId, correlationId: correlationId, localTag: localTag, outerCtx: outerCtx) {
			//
			_skipLinkedMetadata = skipLinkedMetadata;
		}

		public bool SkipLinkedMetadata
			=> _skipLinkedMetadata;

	}

}