using System.IO;
using System.Threading.Tasks;

using Eon.Context;
using Eon.IO;
using Eon.Net.Mime;

namespace Eon.Description {

	public class DescriptionPackageFileMaterializer
		:SubjectFileMaterializer<IDescriptionPackage> {

		public DescriptionPackageFileMaterializer() { }

		protected override async Task<IVh<StreamInfo>> RecognizeMediaFormatAsync(ISubjectFileLoadContext ctx, Stream stream) {
			ctx.EnsureNotNull(nameof(ctx));
			//
			if (ctx.File.Extension.EqualsOrdinalCI(DescriptionPackageConstants.PackageJsonFormatFileExtension))
				return new StreamInfo(stream: stream, ownsStream: false, contentMediaType: MediaTypeNameUtilities.AppJson).ToValueHolder();
			else if (ctx.File.Extension.EqualsOrdinalCI(DescriptionPackageConstants.PackageXmlFormatFileExtension))
				return new StreamInfo(stream: stream, ownsStream: false, contentMediaType: MediaTypeNameUtilities.AppXml).ToValueHolder();
			else
				return await base.RecognizeMediaFormatAsync(ctx, stream).ConfigureAwait(false);
		}

	}

}