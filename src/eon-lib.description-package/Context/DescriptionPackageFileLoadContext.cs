using System;
using System.IO;

using Eon.Description;

namespace Eon.Context {

	public class DescriptionPackageFileLoadContext
		:DescriptionPackageLoadContext, ISubjectFileLoadContext {

		FileInfo _file;

		public DescriptionPackageFileLoadContext(
			DescriptionPackageLocator locator,
			Uri fileUri,
			bool skipLinkedMetadata,
			string mediaType = default,
			Uri siteOrigin = default,
			XFullCorrelationId fullCorrelationId = default,
			ArgumentPlaceholder<XCorrelationId> correlationId = default,
			object localTag = default,
			IContext outerCtx = default)
			: base(
					locator: locator,
					loadUri: fileUri.Arg(nameof(fileUri)).EnsureAbsolute().EnsureFileScheme().EnsureLoopbackOrUnc().Value,
					skipLinkedMetadata: skipLinkedMetadata,
					mediaType: mediaType,
					siteOrigin: siteOrigin,
					fullCorrelationId: fullCorrelationId,
					correlationId: correlationId,
					localTag: localTag,
					outerCtx: outerCtx) { }

		public FileInfo File
			=> UpdDAIfNull(location: ref _file, factory: () => new FileInfo(fileName: BaseUri.LocalPath));

		protected override void Dispose(bool explicitDispose) {
			_file = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}