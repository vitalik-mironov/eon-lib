using System;
using System.IO;
using System.Threading.Tasks;

using Eon.Context;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.IO {

	public class SubjectFileMaterializer<TSubject>
		:SubjectStreamMaterializerBase<TSubject>
		where TSubject : class {

		public SubjectFileMaterializer() { }

		protected sealed override async Task<IVh<StreamInfo>> GetSourceStreamAsync(ISubjectLoadContext ctx) {
			var fileLoadContext = ctx.Arg(nameof(ctx)).EnsureOfType<ISubjectLoadContext, ISubjectFileLoadContext>().Value;
			//
			return await GetSourceStreamAsync(fileLoadContext).ConfigureAwait(false);
		}

		// TODO: Put strings into the resources.
		//
		protected virtual Task<IVh<StreamInfo>> GetSourceStreamAsync(ISubjectFileLoadContext ctx) {
			var taskProxy = default(TaskCompletionSource<IVh<StreamInfo>>);
			try {
				taskProxy = new TaskCompletionSource<IVh<StreamInfo>>();
				//
				ctx.EnsureNotNull(nameof(ctx));
				//
				Uri readUri;
				try {
					readUri = ctx.BaseUri.Arg($"{nameof(ctx)}.{nameof(ctx.BaseUri)}").EnsureAbsolute().EnsureScheme(UriUtilities.UriSchemeFile).EnsureLoopbackOrUnc().Value;
				}
				catch (Exception exception) {
					throw
						new ArgumentException(
							message: $"Указанный контекст загрузки имеет недопустимый базовый URI загрузки (свойство '{nameof(ctx)}.{nameof(ctx.BaseUri)}').{Environment.NewLine}\tИмя параметра:{Environment.NewLine}{nameof(ctx).IndentLines2()}{Environment.NewLine}\tКонтекст загрузки (параметр):{Environment.NewLine}{ctx.FmtStr().G().IndentLines2()}",
							innerException: exception);
				}
				//
				var file = new FileInfo(readUri.LocalPath);
				if (file.Exists)
					taskProxy
						.SetResult(
							result:
								new StreamInfo(
									stream: new FileStream(path: file.FullName, mode: FileMode.Open, access: FileAccess.Read, share: FileShare.Read),
									ownsStream: true)
								.ToValueHolder(ownsValue: true));
				else
					throw
						new FileNotFoundException(
							message: FormatXResource(locator: typeof(FileNotFoundException), subpath: null, args: new string[ ] { file.FullName }),
							fileName: file.FullName);
				//
				return taskProxy.Task;
			}
			catch (Exception exception) {
				if (taskProxy?.TrySetException(exception) == true)
					return taskProxy.Task;
				else
					return Task.FromException<IVh<StreamInfo>>(exception);
			}
		}

		protected sealed override async Task<IVh<StreamInfo>> RecognizeMediaFormatAsync(ISubjectLoadContext ctx, Stream stream) {
			var fileLoadContext = ctx.Arg(nameof(ctx)).EnsureOfType<ISubjectLoadContext, ISubjectFileLoadContext>().Value;
			//
			return await RecognizeMediaFormatAsync(fileLoadContext, stream).ConfigureAwait(false);
		}

		protected virtual Task<IVh<StreamInfo>> RecognizeMediaFormatAsync(ISubjectFileLoadContext ctx, Stream stream)
			=> base.RecognizeMediaFormatAsync(ctx, stream);

		protected sealed override async Task<TSubject> DoMaterializeAsync(ISubjectLoadContext ctx, StreamInfo stream) {
			var fileLoadCtx = ctx.Arg(nameof(ctx)).EnsureOfType<ISubjectLoadContext, ISubjectFileLoadContext>().Value;
			//
			return await DoMaterializeAsync(context: fileLoadCtx, stream: stream).ConfigureAwait(false);
		}

		protected virtual Task<TSubject> DoMaterializeAsync(ISubjectFileLoadContext context, StreamInfo stream)
			=> base.DoMaterializeAsync(context, stream);

	}

}