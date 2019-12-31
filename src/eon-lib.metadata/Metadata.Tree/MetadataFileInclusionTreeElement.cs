using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;

using Eon.Context;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

using static Eon.Net.Mime.MediaTypeNameUtilities;
using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Metadata.Tree {

	[DataContract]
	public class MetadataFileInclusionTreeElement
		:MetadataInclusionTreeElementBase {

		public MetadataFileInclusionTreeElement(IMetadataTreeNode node, ReadOnlyStateTag readOnlyState = default)
			: base(node: node, readOnlyState: readOnlyState) { }

		public MetadataFileInclusionTreeElement(ReadOnlyStateTag readOnlyState = default)
			: this(node: null, readOnlyState: readOnlyState) { }

		[JsonConstructor]
		protected MetadataFileInclusionTreeElement(SerializationContext ctx)
			: base(ctx: ctx) { }

		protected override void OnValidateLocationUri(Uri locationUri) {
			base.OnValidateLocationUri(locationUri);
			//
			string validationErrorMessage;
			if (!P_IsLocationUriValid(locationUri, out validationErrorMessage))
				throw new ArgumentOutOfRangeException(paramName: nameof(locationUri), message: validationErrorMessage);
		}

		bool P_IsLocationUriValid(Uri locationUri, out string validationErrorMessage) {
			locationUri.EnsureNotNull(nameof(locationUri));
			//
			if (locationUri.IsAbsoluteUri) {
				if (!locationUri.Scheme.EqualsOrdinalCI(UriUtilities.UriSchemeFile)) {
					validationErrorMessage = FormatXResource(typeof(Uri), "NotValidScheme/Expected", locationUri, UriUtilities.UriSchemeFile);
					return false;
				}
				else if (!(locationUri.IsLoopback || locationUri.IsUnc)) {
					validationErrorMessage = FormatXResource(typeof(Uri), "NorLoopbackNorUnc", locationUri);
					return false;
				}
			}
			//
			validationErrorMessage = null;
			return true;
		}

		// TODO: Put strings into the resources.
		// TODO_HIGH: Use SubjectMaterializer component to load metadata.
		//
		protected override async Task<IVh<IMetadata>> DoLoadMetadataAsync(IMetadataLoadContext loadCtx) {
			loadCtx.EnsureNotNull(nameof(loadCtx));
			//
			loadCtx.ThrowIfCancellationRequested();
			await Task.CompletedTask;
			return doLoad();
			//
			IVh<IMetadata> doLoad() {
				var locDeserializedObjectAsIDisposable = default(IDisposable);
				try {
					var locFormatMediaTypeName = FormatMediaTypeName;
					var locLocationUri = LocationUri;
					//
					string locLoadFilePath;
					if (locLocationUri.IsAbsoluteUri)
						locLoadFilePath = locLocationUri.LocalPath;
					else {
						var locBaseUri = loadCtx.BaseUri;
						string locBaseUriValidationErrorMessage;
						if (!locBaseUri.IsAbsoluteUri)
							throw
								new EonException(
									message: $"Базовый URI контекста загрузки метаданных не может быть использован для разрешения полного пути к файлу загрузки метаданных '{locLocationUri}'.{Environment.NewLine}{FormatXResource(typeof(Uri), "NotAbsoluteUri", locBaseUri.FmtStr().G())}{Environment.NewLine}\tКонтекст загрузки:{loadCtx.FmtStr().GNLI()}");
						else if (!P_IsLocationUriValid(locBaseUri, out locBaseUriValidationErrorMessage))
							throw
								new EonException(
									message: $"Базовый URI контекста загрузки метаданных не может быть использован для разрешения полного пути к файлу загрузки метаданных '{locLocationUri}'.{Environment.NewLine}{locBaseUriValidationErrorMessage}{Environment.NewLine}\tКонтекст загрузки:{loadCtx.FmtStr().GNLI2()}");
						locLoadFilePath = Path.Combine(Path.GetDirectoryName(locBaseUri.LocalPath), locLocationUri.ToString());
					}
					var locLoadFile = new FileInfo(locLoadFilePath);
					if (!locLoadFile.Exists)
						throw new FileNotFoundException(FormatXResource(locator: typeof(FileNotFoundException), subpath: null, args: new[ ] { locLoadFile.FullName }), locLoadFile.FullName);
					//
					IMetadata locLoadedMetadata;
					object locDeserializedObject;
					if (IsXmlMediaType(locFormatMediaTypeName)) {
						try {
							using (var locFileStream = locLoadFile.OpenRead())
							using (var locXmlReader = XmlReader.Create(locFileStream, loadCtx.CreateXmlReaderSettings())) {
								var locSerializer = loadCtx.CreateXmlObjectSerializer(typeof(object));
								locDeserializedObjectAsIDisposable = (locDeserializedObject = locSerializer.ReadObject(reader: locXmlReader, verifyObjectName: false)) as IDisposable;
							}
							if (locDeserializedObject is IMetadata locMetadata) {
								if (locMetadata.IsAutoName && MetadataName.TryParse(value: Path.GetFileNameWithoutExtension(path: locLoadFile.Name), result: out var metadataName))
									locMetadata.Name = metadataName;
								locLoadedMetadata = locMetadata;
							}
							else if (!(locDeserializedObject is null))
								throw new EonException(message: $"Десериализованный объект имеет тип '{locDeserializedObject.GetType()}', который не совместим с типом '{typeof(IMetadata)}'.");
							else
								locLoadedMetadata = null;
						}
						catch (Exception exception) {
							throw new EonException(message: $"Ошибка загрузки объекта из файла.{Environment.NewLine}\tФайл:{locLoadFile.FullName.FmtStr().GNLI2()}.", innerException: exception);
						}
					}
					else
						throw new NotSupportedException(message: $"Формат, указанный в элементе, не поддерживается.{Environment.NewLine}\tЭлемент:{this.FmtStr().GNLI2()}{Environment.NewLine}\tФормат:{locFormatMediaTypeName.FmtStr().GNLI2()}");
					return locLoadedMetadata.ToValueHolder(ownsValue: true);
				}
				catch (Exception exception) {
					locDeserializedObjectAsIDisposable?.Dispose(exception: exception);
					throw;
				}
			}
		}

	}

}