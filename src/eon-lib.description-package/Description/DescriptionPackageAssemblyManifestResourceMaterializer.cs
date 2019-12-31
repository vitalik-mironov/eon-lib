using System;
using System.IO;
using System.Threading.Tasks;

using Eon.Context;
using Eon.IO;
using Eon.Net.Mime;
using Eon.Resources;

namespace Eon.Description {

	public class DescriptionPackageAssemblyManifestResourceMaterializer
		:SubjectStreamMaterializerBase<IDescriptionPackage> {

		public DescriptionPackageAssemblyManifestResourceMaterializer() { }

		protected override async Task<IVh<StreamInfo>> GetSourceStreamAsync(ISubjectLoadContext ctx) {
			ctx.EnsureNotNull(nameof(ctx));
			//
			await Task.CompletedTask;
			//
			Stream sourceStream = default;
			try {
				sourceStream = ResourceUtilities.RequireManifestResourceStream(uri: ctx.BaseUri.ArgProp($"{nameof(ctx)}.{nameof(ctx.BaseUri)}"), resourceName: out var resourceName);
				string mediaType;
				if (resourceName.EndsWith(value: DescriptionPackageConstants.PackageXmlFormatFileExtension, comparisonType: StringComparison.OrdinalIgnoreCase))
					mediaType = MediaTypeNameUtilities.AppXml;
				else if (resourceName.EndsWith(value: DescriptionPackageConstants.PackageJsonFormatFileExtension, comparisonType: StringComparison.OrdinalIgnoreCase))
					mediaType = MediaTypeNameUtilities.AppJson;
				else
					mediaType = default;
				return new StreamInfo(stream: sourceStream, ownsStream: true, contentMediaType: mediaType).ToValueHolder(ownsValue: true);
			}
			catch {
				sourceStream?.Dispose();
				throw;
			}
		}

	}

}