using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Description;
using Eon.IO;
using Eon.Net.Mime;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Metadata.Tree {

	[DataContract]
	public class MetadataTreeNodeDirectoryInclusion
		:MetadataTreeNodeInclusionBase {

		public MetadataTreeNodeDirectoryInclusion()
			: base() { }

		[JsonConstructor]
		protected MetadataTreeNodeDirectoryInclusion(SerializationContext ctx)
			: base(ctx: ctx) { }

		protected override void OnValidateLocationUri(Uri locationUri) {
			base.OnValidateLocationUri(locationUri);
			//
			locationUri
				.Arg(nameof(locationUri))
				.EnsureFileScheme()
				.EnsureLoopbackOrUnc();
		}

		public override async Task<IEnumerable<IMetadataTreeNode>> LoadNodesAsync(IMetadataLoadContext loadCtx) {
			loadCtx.EnsureNotNull(nameof(loadCtx));
			//
			loadCtx.ThrowIfCancellationRequested();
			return await LoadNodesAsync(loadCtx: loadCtx, directory: GetLocationDirectory(loadCtx)).ConfigureAwait(false);
		}

		public virtual async Task<IEnumerable<IMetadataTreeNode>> LoadNodesAsync(IMetadataLoadContext loadCtx, DirectoryInfo directory, IMetadataTreeNode parentNode = default) {
			await Task.CompletedTask;
			return P_LoadNodes(loadCtx: loadCtx, directory: directory, parentNode: parentNode, continuation: false);
		}

		// TODO_HIGH: Расширения файлов (+ их медиа типы), рассматриваемых для загрузки должны браться из контекста.
		//
		protected virtual bool ShouldIncludeFile(IMetadataLoadContext context, FileInfo file, out string mediaType) {
			file.EnsureNotNull(nameof(file));
			//
			if (file.Extension.EqualsOrdinalCI(DescriptionPackageConstants.DescriptionXmlFormatFileExtension)) {
				mediaType = MediaTypeNameUtilities.AppXml;
				return true;
			}
			else if (file.Extension.EqualsOrdinalCI(DescriptionPackageConstants.DescriptionJsonFormatFileExtension)) {
				mediaType = MediaTypeNameUtilities.AppJson;
				return true;
			}
			else {
				mediaType = null;
				return false;
			}
		}

		// TODO: Put strings into the resources.
		//
		IEnumerable<IMetadataTreeNode> P_LoadNodes(IMetadataLoadContext loadCtx, DirectoryInfo directory, IMetadataTreeNode parentNode = default, bool continuation = default) {
			loadCtx.EnsureNotNull(nameof(loadCtx));
			directory.EnsureNotNull(nameof(directory));
			//
			loadCtx.ThrowIfCancellationRequested();
			var buffer = continuation ? null : new List<IMetadataTreeNode>();
			var fileNameAndNodeMap = new Dictionary<string, IMetadataTreeNode>(FileSystemAccessUtilities.DefaultPathComparer);
			//
			var files = directory.GetFiles(searchPattern: "*", searchOption: SearchOption.TopDirectoryOnly);
			for (var i = 0; i < files.Length; i++) {
				loadCtx.ThrowIfCancellationRequested();
				var file = files[ i ];
				if (ShouldIncludeFile(context: loadCtx, file: file, mediaType: out var fileFormatMediaType)) {
					var metadataElement =
						new MetadataFileInclusionTreeElement() {
							LocationUri = new Uri(uriString: $"{UriUtilities.UriSchemeFile}://{file.FullName}", uriKind: UriKind.Absolute),
							FormatMediaTypeName = fileFormatMediaType
						};
					var node = new MetadataTreeNode(parent: parentNode, caption: file.Name, metadataElement: metadataElement);
					buffer?.Add(node);
					fileNameAndNodeMap.Add(Path.GetFileNameWithoutExtension(file.Name), node);
				}
			}
			//
			var subdirectories = directory.GetDirectories(searchPattern: "*", searchOption: SearchOption.TopDirectoryOnly);
			for (var i = 0; i < subdirectories.Length; i++) {
				loadCtx.ThrowIfCancellationRequested();
				var subdirectory = subdirectories[ i ];
				if (!fileNameAndNodeMap.TryGetValue(subdirectory.Name, out var subdirectoryParentNode)) {
					MetadataName name;
					try {
						name = (MetadataName)subdirectory.Name;
					}
					catch (Exception exception) {
						throw new EonException(message: $"Directory name can't be converted to metadata name (type '{typeof(MetadataName)}').{Environment.NewLine}\tDirectory:{subdirectory.FmtStr().GNLI2()}", innerException: exception);
					}
					var metadataElement = new EmbeddedMetadataTreeElement(embeddedMetadata: new Namespace(name: name), ownsEmbeddedMetadata: true);
					var node = new MetadataTreeNode(parent: parentNode, caption: metadataElement.EmbeddedMetadata.Name, metadataElement: metadataElement);
					buffer?.Add(node);
					subdirectoryParentNode = node;
				}
				//
				P_LoadNodes(loadCtx: loadCtx, directory: subdirectory, parentNode: subdirectoryParentNode, continuation: true);
			}
			//
			return buffer ?? Enumerable.Empty<IMetadataTreeNode>();
		}

		// TODO: Put strings into the resources.
		// TODO_HIGH: Реализовать поддержку переменных среды (%var%).
		//
		protected virtual DirectoryInfo GetLocationDirectory(IMetadataLoadContext loadCtx) {
			loadCtx.EnsureNotNull(nameof(loadCtx));
			//
			var locationUri = LocationUri;
			if (locationUri.IsAbsoluteUri)
				return new DirectoryInfo(locationUri.LocalPath);
			else {
				var path = locationUri.ExpandEnvironmentVariables();
				if (Path.IsPathRooted(path))
					return new DirectoryInfo(path);
				else {
					Uri baseUri;
					try {
						baseUri = loadCtx.BaseUri;
						baseUri.Arg($"{nameof(loadCtx)}.{nameof(loadCtx.BaseUri)}").EnsureFileScheme().EnsureLoopbackOrUnc();
					}
					catch (Exception exception) {
						throw
							new EonException(
								message: $"Базовый URI контекста загрузки метаданных не может быть использован для разрешения полного пути к требуемой директории расположения метаданных.{Environment.NewLine}\tКонтекст загрузки:{loadCtx.FmtStr().GNLI2()}{Environment.NewLine}\tТребуемая директория: '{locationUri.FmtStr().G()}'.",
								innerException: exception);
					}
					return new DirectoryInfo(Path.Combine(path1: Path.GetDirectoryName(baseUri.LocalPath), path2: path));
				}
			}
		}

		public override bool HasChildComponent(IMetadataTreeStructureComponent component) {
			component.EnsureNotNull(nameof(component));
			//
			return false;
		}

		protected override void GetChildComponents(out IEnumerable<IMetadataTreeStructureComponent> components)
			=> components = Enumerable.Empty<IMetadataTreeStructureComponent>();

		protected sealed override void PopulateCopy(CopyArgs args, MetadataTreeNodeInclusionBase copy) {
			var locCopy =
				copy
				.EnsureNotNull(nameof(copy))
				.EnsureOfType<MetadataTreeNodeInclusionBase, MetadataTreeNodeDirectoryInclusion>()
				.Value;
			//
			PopulateCopy(args, locCopy);
		}

		protected virtual void PopulateCopy(CopyArgs args, MetadataTreeNodeDirectoryInclusion copy) {
			copy.EnsureNotNull(nameof(copy));
			//
		}

	}

}