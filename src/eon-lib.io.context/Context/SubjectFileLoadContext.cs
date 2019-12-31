using System;
using System.IO;

using Eon.Context;

namespace Eon.Context {
#pragma warning disable CS3001 // Argument type is not CLS-compliant

	public class SubjectFileLoadContext
		:SubjectLoadContextBase, ISubjectFileLoadContext {

		FileInfo _file;

		public SubjectFileLoadContext(
			string localFilePath,
			string mediaType = default,
			XFullCorrelationId fullCorrelationId = default,
			ArgumentPlaceholder<XCorrelationId> correlationId = default,
			object localTag = default,
			IContext ambientContext = default)
			: this(
					fileUri: new Uri(uriString: localFilePath.Arg(nameof(localFilePath)).EnsureNotNullOrWhiteSpace(), uriKind: UriKind.Absolute),
					mediaType: mediaType,
					fullCorrelationId: fullCorrelationId,
					correlationId: correlationId,
					localTag: localTag,
					ambientContext: ambientContext) { }

		public SubjectFileLoadContext(
			FileInfo localFile,
			string mediaType = default,
			XFullCorrelationId fullCorrelationId = default,
			ArgumentPlaceholder<XCorrelationId> correlationId = default,
			object localTag = default,
			IContext ambientContext = default)
			: this(
				 fileUri: new Uri(localFile.EnsureNotNull(nameof(localFile)).Value.FullName, UriKind.Absolute),
				 mediaType: mediaType,
				 fullCorrelationId: fullCorrelationId,
				 correlationId: correlationId,
				 localTag: localTag,
				 ambientContext: ambientContext) {
			//
			_file = localFile;
		}

		public SubjectFileLoadContext(
			Uri fileUri,
			string mediaType = default,
			XFullCorrelationId fullCorrelationId = default,
			ArgumentPlaceholder<XCorrelationId> correlationId = default,
			object localTag = default,
			IContext ambientContext = default)
			: base(
					baseUri: fileUri.Arg(nameof(fileUri)).EnsureFileScheme().EnsureLoopbackOrUnc(),
					mediaType: mediaType,
					fullCorrelationId: fullCorrelationId,
					correlationId: correlationId,
					localTag: localTag,
					outerCtx: ambientContext) { }

		public FileInfo File
			=> UpdDAIfNull(location: ref _file, factory: () => new FileInfo(fileName: BaseUri.LocalPath));

		protected override void Dispose(bool explicitDispose) {
			_file = null;
			//
			base.Dispose(explicitDispose);
		}

	}

#pragma warning restore CS3001 // Argument type is not CLS-compliant
}