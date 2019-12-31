using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Eon.Collections;
using Eon.Collections.Trees;
using Eon.Context;
using Eon.IO;
using Eon.Linq;
using Eon.Metadata;
using Eon.Metadata.Tree;
using Eon.Resources;
using Eon.Runtime.Options;
using Eon.Threading;
using Eon.Threading.Tasks;

using static Eon.DisposableUtilities;
using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Description {
	using IDescriptionPackageMaterializer = ISubjectMaterializer<IDescriptionPackage>;

	public class DefaultDescriptionPackageService
		:DisposeNotifying, IDescriptionPackageService {

		#region Nested types

		sealed class P_GeneratedPackageLoadContext
			:DescriptionPackageLoadContext {

			internal P_GeneratedPackageLoadContext(DescriptionPackageLocator locator)
				: base(locator: locator, siteOrigin: DescriptionPackageUtilities.UndefinedSiteOrigin, loadUri: DescriptionPackageUtilities.InMemoryGeneratedPackageBaseUri, skipLinkedMetadata: true) { }

		}

		sealed class P_LoadedPackageState
			:DisposeNotifying {

			DefaultDescriptionPackageService _service;

			DescriptionPackageLocator _locator;

			CancellationTokenSource _disposeCts;

			AsyncOperator<IDescriptionPackage> _loader;

			internal P_LoadedPackageState(DefaultDescriptionPackageService service, DescriptionPackageLocator locator) {
				service.EnsureNotNull(nameof(service));
				locator.EnsureNotNull(nameof(locator));
				//
				_service = service;
				_locator = locator;
				_disposeCts = new CancellationTokenSource();
				_loader = new AsyncOperator<IDescriptionPackage>(asyncFactory: P_LoadNewPackageInstanceAsync, ownsResult: true);
			}

			internal P_LoadedPackageState(DefaultDescriptionPackageService service, IDescriptionPackage package, DescriptionPackageLocator locator, bool ownsPackage = false) {
				service.EnsureNotNull(nameof(service));
				package.EnsureNotNull(nameof(package));
				locator.EnsureNotNull(nameof(locator));
				//
				_service = service;
				_locator = locator;
				_disposeCts = null;
				_loader = new AsyncOperator<IDescriptionPackage>(result: package, ownsResult: ownsPackage);
			}

			public DescriptionPackageLocator Locator
				=> ReadDA(ref _locator);

			async Task<IDescriptionPackage> P_LoadNewPackageInstanceAsync(IContext ctx = default) {
				var service = ReadDA(ref _service);
				var locator = ReadDA(ref _locator);
				var loadCtx = default(IDescriptionPackageLoadContext);
				var package = default(IDescriptionPackage);
				Exception caughtException = default;
				try {
					loadCtx = service.CreateLoadContext(locator: locator, outerCtx: ctx);
					package = await service.MaterializeAsync(loadCtx: loadCtx).ConfigureAwait(false);
					await service.LoadMaterializedAsync(package: package, loadCtx: loadCtx).ConfigureAwait(false);
					return package;
				}
				catch (Exception exception) {
					caughtException = exception;
					throw;
				}
				finally {
					if (!(caughtException is null))
						package?.Dispose(caughtException);
					loadCtx?.Dispose(caughtException);
				}
			}

			public async Task<IDescriptionPackage> LoadAsync(IContext ctx = default) {
				var ctxCt = ctx.Ct();
				ctxCt.ThrowExceptionIfCancellationRequested();
				var disposeCts = ReadDA(location: ref _disposeCts);
				var disposeCt = disposeCts.Token;
				var loader = ReadDA(location: ref _loader, considerDisposeRequest: true);
				var linkedCts = default(CancellationTokenSource);
				try {
					var linkedCt = CancellationTokenUtilities.SingleOrLinked(ct1: ctxCt, ct2: disposeCt, linkedCts: out linkedCts);
					using (var locCtx = ContextUtilities.Create(outerCtx: ctx, ct: linkedCt))
						return await loader.ExecuteAsync(ctx: locCtx).ConfigureAwait(false);
				}
				finally {
					linkedCts?.Dispose();
				}
			}

			public IDescriptionPackage GetPackageIfLoaded() {
				var loader = ReadDA(ref _loader);
				if (loader.IsCompleted)
					return loader.Result;
				else
					return null;
			}

			protected override void FireBeforeDispose(bool explicitDispose) {
				if (explicitDispose) {
					var disposeCts = TryReadDA(ref _disposeCts);
					if (disposeCts?.IsCancellationRequested == false)
						try { disposeCts.Cancel(throwOnFirstException: false); }
						catch (ObjectDisposedException) { }
				}
				//
				base.FireBeforeDispose(explicitDispose);
			}

			protected override void Dispose(bool explicitDispose) {
				if (explicitDispose) {
					_loader?.Dispose();
					_disposeCts?.Dispose();
				}
				_service = null;
				_locator = null;
				_disposeCts = null;
				_loader = null;
				//
				base.Dispose(explicitDispose);
			}

		}

		#endregion

		PrimitiveSpinLock _loadedPackagesSpinLock;

		Dictionary<DescriptionPackageLocator, P_LoadedPackageState> _loadedPackages;

		public DefaultDescriptionPackageService() {
			_loadedPackagesSpinLock = new PrimitiveSpinLock();
			_loadedPackages = new Dictionary<DescriptionPackageLocator, P_LoadedPackageState>(comparer: DescriptionPackageLocator.PackageNameEqualityComparer);
		}

		public virtual DescriptionPackageLocator CreatePackageLocator(IDescriptionPackage package) {
			package.EnsureNotNull(nameof(package));
			//
			var packageIdentity = package.Identity.ArgProp($"{nameof(package)}.{nameof(package.Identity)}").EnsureNotNull().AsReadOnly().EnsureValid().Value;
			return new DescriptionPackageLocator(packageName: packageIdentity.Name, packagePublisherScopeId: packageIdentity.PublisherScopeId);
		}

		// TODO: Put strings into the resources.
		//
		public virtual IDescriptionPackageLoadContext CreateLoadContext(DescriptionPackageLocator locator, IContext outerCtx = default) {
			locator.EnsureNotNull(nameof(locator));
			//
			var locationBaseUri = locator.PackageLocationBaseUri;
			if (locationBaseUri is null || !locationBaseUri.IsAbsoluteUri || locationBaseUri.IsFileUri().Value) {
				// По умолчанию полагается, что базовое расположение пакета — файл.
				//
				return new DescriptionPackageFileLoadContext(locator: locator, fileUri: GetFileLoadSourceUri(locator: locator), skipLinkedMetadata: true, outerCtx: outerCtx);
			}
			else if (locationBaseUri.IsAssemblyManifestResourceUri().Value) {
				// Расположение пакета — встроенный ресурс .net-сборки.
				//
				return new DescriptionPackageLoadContext(locator: locator, loadUri: GetAssemblyManifestResourceLoadSourceUri(locator: locator), skipLinkedMetadata: true, outerCtx: outerCtx);
			}
			else
				throw new NotSupportedException(message: $"Specified package location base URI is not supported by this component{Environment.NewLine}\tURI:{locationBaseUri.FmtStr().GNLI2()}{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}").SetErrorCode(code: GeneralErrorCodes.Operation.NotSupported);
		}

		protected virtual Uri GetAssemblyManifestResourceLoadSourceUri(DescriptionPackageLocator locator) {
			P_ThrowIfUnsupportedLocatorParts(locator: locator.Arg(nameof(locator)));
			var locationBaseUri = locator.PackageLocationBaseUri.ArgProp($"{nameof(locator)}.{nameof(locator.PackageLocationBaseUri)}").EnsureNotNull().EnsureAbsolute().EnsureScheme(scheme: UriUtilities.UriSchemeAsemblyManifestResource).Value;
			//
			var packageName = locator.PackageName;
			var satelliteName = locator.SatellitePackageName;
			ResourceUtilities.DeconstructManifestResourceUri(uri: locationBaseUri.ArgProp($"{nameof(locator)}.{nameof(locator.PackageLocationBaseUri)}"), assemblyName: out var packageAssemblyName);
			var packageResourceName = $"{packageName.Value}";
			if (!(satelliteName is null))
				packageResourceName += $".{satelliteName.Value}";
			packageResourceName += DescriptionPackageUtilities.PackageXmlFormatFileExtension;
			return ResourceUtilities.ConstructManifestResourceUri(assemblyName: packageAssemblyName, name: packageResourceName);
		}

		protected virtual Uri GetConfigurationLoadSourceUri(DescriptionPackageLocator locator) {
			P_ThrowIfUnsupportedLocatorParts(locator: locator.Arg(nameof(locator)));
			//
			throw new NotSupportedException().SetErrorCode(code: GeneralErrorCodes.Operation.NotImplemented);
		}

		// TODO: Put strings into the resources.
		//
		protected virtual Uri GetFileLoadSourceUri(DescriptionPackageLocator locator) {
			P_ThrowIfUnsupportedLocatorParts(locator: locator.Arg(nameof(locator)));
			var locationBaseUri = locator.PackageLocationBaseUri;
			if (locationBaseUri?.IsAbsoluteUri == true)
				locationBaseUri.ArgProp($"{nameof(locator)}.{nameof(locator.PackageLocationBaseUri)}").EnsureFileScheme().EnsureLoopbackOrUnc();
			//
			var dotDirectoryName = DotDirectoryNameOption.Require();
			var packageName = locator.PackageName;
			var satelliteName = locator.SatellitePackageName;
			string basePath;
			if (locationBaseUri is null)
				basePath = Path.Combine(path1: AppContext.BaseDirectory, path2: dotDirectoryName);
			else if (locationBaseUri.IsAbsoluteUri) {
				basePath = locationBaseUri.LocalPath;
				if (!Path.IsPathRooted(basePath))
					throw
						new ArgumentException(
							message: $"Результат приведения указанного URI к формату локального пути не содержит корневой каталог (директорию).{Environment.NewLine}\tУказанный URI:{locationBaseUri.FmtStr().GNLI2()}{Environment.NewLine}\tРезультат приведения:{basePath.FmtStr().GNLI2()}");
				else
					basePath = Path.Combine(path1: basePath, path2: dotDirectoryName);
			}
			else {
				basePath = locationBaseUri.ExpandEnvironmentVariables();
				if (Path.IsPathRooted(basePath))
					basePath = Path.Combine(path1: basePath, path2: dotDirectoryName);
				else {
					basePath = basePath.TrimStartSingle(Path.DirectorySeparatorChar).TrimStartSingle(Path.AltDirectorySeparatorChar);
					basePath = Path.Combine(path1: AppContext.BaseDirectory, path2: basePath, path3: dotDirectoryName);
				}
			}
			basePath = Path.Combine(path1: basePath, path2: packageName.Value);
			if (!(satelliteName is null))
				basePath = Path.Combine(path1: basePath, path2: DescriptionPackageUtilities.SatellitePackageLocationQualifier + satelliteName.Value);
			basePath =
				Path.Combine(
					path1: basePath,
					path2: DescriptionPackageUtilities.PackageFileNameWithoutExtension + DescriptionPackageUtilities.PackageXmlFormatFileExtension);
			return new Uri(uriString: basePath, uriKind: UriKind.Absolute);
		}

		// TODO: Put strings into the resources.
		//
		void P_ThrowIfUnsupportedLocatorParts(ArgumentUtilitiesHandle<DescriptionPackageLocator> locator) {
			locator.EnsureNotNull();
			//
			if (!locator.Value.PackagePublisherScopeId.IsNullOrUndefined())
				throw
					new ArgumentOutOfRangeException(
						paramName: $"{locator.Name}.{nameof(locator.Value.PackagePublisherScopeId)}",
						message: $"Specified value of publisher (see '{nameof(locator.Value.PackagePublisherScopeId)}') not supported.{Environment.NewLine}\tExpected value:{UriBasedIdentifier.Undefined.FmtStr().GNLI2()}{Environment.NewLine}\tSpecified:{locator.Value.PackagePublisherScopeId.FmtStr().GNLI2()}");
			else if (locator.Value.PackageVersion != null)
				throw
					new ArgumentOutOfRangeException(
						paramName: $"{locator.Name}.{nameof(locator.Value.PackageVersion)}",
						message: $"Specified value of version (see '{nameof(locator.Value.PackageVersion)}') not supported.{Environment.NewLine}\tExpected value:{default(Version).FmtStr().GNLI2()}{Environment.NewLine}\tSpecified value:{locator.Value.PackageVersion.FmtStr().GNLI2()}");
		}

		// TODO: Put strings into the resources.
		//
		public virtual IDescriptionPackageMaterializer CreateMaterializer(IDescriptionPackageLoadContext ctx) {
			ctx.EnsureNotNull(nameof(ctx));
			//
			var contextBaseUri = ctx.BaseUri;
			if (contextBaseUri.IsAssemblyManifestResourceUri().Value)
				return new DescriptionPackageAssemblyManifestResourceMaterializer();
			else if (contextBaseUri.IsFileUri().Value)
				return new DescriptionPackageFileMaterializer();
			else
				throw new NotSupportedException(message: $"Specified context base URI (see property '{nameof(ctx)}.{nameof(ctx.BaseUri)}') is not supported by this component.{Environment.NewLine}\tURI:{contextBaseUri.FmtStr().GNLI2()}{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}").SetErrorCode(code: GeneralErrorCodes.Operation.NotSupported);
		}

		public async Task<IDescriptionPackage> LoadAsync(DescriptionPackageLocator locator, IContext ctx = default) {
			locator.EnsureNotNull(nameof(locator));
			//
			try {
				var newState = default(P_LoadedPackageState);
				var state =
					ReadDA(ref _loadedPackages)
					.GetOrAdd(
						spinLock: ReadDA(ref _loadedPackagesSpinLock, considerDisposeRequest: true),
						dictionaryOwner: this,
						key: locator,
						factory:
							locKey => {
								newState = new P_LoadedPackageState(service: this, locator: locKey);
								return newState;
							},
						unclaimedValue: (locKey, locValue) => locValue?.Dispose());
				if (ReferenceEquals(objA: state, objB: newState))
					state.AfterDisposed += P_EH_LoadedPackageState_AfterDisposed;
				return await state.LoadAsync(ctx: ctx).ConfigureAwait(false);
			}
			catch (Exception exception) when (!(exception is DescriptionPackageLoadException)) {
				throw new DescriptionPackageLoadException(packageLocator: locator, innerException: exception);
			}
		}

		// TODO: Put strings into the resources.
		//
		protected virtual async Task<IDescriptionPackage> MaterializeAsync(IDescriptionPackageLoadContext loadCtx) {
			loadCtx.EnsureNotNull(nameof(loadCtx));
			//
			var materializer = default(IDescriptionPackageMaterializer);
			var caughtException = default(Exception);
			try {
				materializer = CreateMaterializer(ctx: loadCtx);
				//
				var package = await materializer.MaterializeAsync(ctx: loadCtx).Unwrap().ConfigureAwait(false);
				if (package is null)
					throw new EonException(message: $"Description package not materialized. Materializer has returned invalid value '{package.FmtStr().G()}'.{Environment.NewLine}\tMaterializer:{materializer.FmtStr().GNLI2()}{Environment.NewLine}\tLoad context:{loadCtx.FmtStr().GNLI2()}");
				else
					return package;
			}
			catch (Exception exception) {
				caughtException = exception;
				throw;
			}
			finally {
				materializer?.Dispose(caughtException);
			}
		}

		protected virtual async Task LoadMaterializedAsync(IDescriptionPackage package, IDescriptionPackageLoadContext loadCtx) {
			package.EnsureNotNull(nameof(package));
			loadCtx.EnsureNotNull(nameof(loadCtx));
			//
			loadCtx.ThrowIfCancellationRequested();
			await LoadAndLinkInclusionsAsync(package: package, loadCtx: loadCtx).ConfigureAwait(false);
			await LoadAndLinkMetadataElementsAsync(package: package, loadCtx: loadCtx).ConfigureAwait(false);
			package.SetReadOnly(isReadOnly: true, isPermanent: true);
			EnsureReadOnly(package: package);
			Validate(package: package, loadCtx: loadCtx);
		}

		protected virtual async Task LoadAndLinkInclusionsAsync(IDescriptionPackage package, IDescriptionPackageLoadContext loadCtx) {
			package.EnsureNotNull(nameof(package));
			loadCtx.EnsureNotNull(nameof(loadCtx));
			//
			loadCtx.ThrowIfCancellationRequested();
			IList<IMetadataTreeNode[ ]> allLoadedNodes = new List<IMetadataTreeNode[ ]>();
			try {
				var processQueue = package.Tree.TreeNode<IMetadataTreeNode>().SelfAndDescendants(y => y.Children).Where(i => i.Inclusions.Count > 0).ToList();
				for (var i = 0; i < processQueue.Count; i++) {
					loadCtx.ThrowIfCancellationRequested();
					//
					var currentNode = processQueue[ i ];
					foreach (var currentNodeInclusion in currentNode.Inclusions) {
						var loadedNodesArray = default(IMetadataTreeNode[ ]);
						try {
							loadedNodesArray = (await currentNodeInclusion.LoadNodesAsync(loadCtx: loadCtx).ConfigureAwait(false)).SkipNull().ToArray();
							processQueue.AddRange(loadedNodesArray.Where(y => y.Inclusions.Count > 0));
							for (var k = 0; k < loadedNodesArray.Length; k++) {
								if (k % 1024 == 0)
									loadCtx.ThrowIfCancellationRequested();
								currentNode.Children.AddComponent(loadedNodesArray[ k ]);
							}
						}
						catch (Exception exception) {
							DisposeMany(exception: exception, disposables: loadedNodesArray);
							throw;
						}
						//
						allLoadedNodes.Add(loadedNodesArray);
					}
				}
			}
			catch (Exception exception) {
				DisposeManyDeep(exception: exception, disposables: allLoadedNodes);
				throw;
			}
		}

		// TODO: Put strings into the resources.
		//
		protected virtual async Task LoadAndLinkMetadataElementsAsync(IDescriptionPackage package, IDescriptionPackageLoadContext loadCtx) {
			package.EnsureNotNull(nameof(package));
			loadCtx.EnsureNotNull(nameof(loadCtx));
			//
			loadCtx.ThrowIfCancellationRequested();
			var allLoadedMetadata = new List<IVh<IMetadata>>();
			try {
				var processQueue = package.Tree.AllMetadataElements.Where(i => i.Metadata is null).ToList();
				for (var i = 0; i < processQueue.Count; i++) {
					loadCtx.ThrowIfCancellationRequested();
					//
					var currentElement = processQueue[ i ];
					var loadedMetadata = await currentElement.LoadMetadataAsync(loadCtx: loadCtx).ConfigureAwait(false);
					allLoadedMetadata.Add(loadedMetadata);
					//
					if (loadedMetadata?.Value is null)
						throw new EonException(message: $"No metadata loaded by specified metadata tree element.{Environment.NewLine}\tTree element:{Environment.NewLine}{currentElement.FmtStr().GNLI2()}");
					currentElement.SetMetadata(link: new Link<IMetadataTreeElement, IVh<IMetadata>>(currentElement, loadedMetadata));
				}
			}
			catch (Exception exception) {
				allLoadedMetadata?.DisposeMany(exception);
				throw;
			}
		}

		// TODO: Put strings into the resources.
		//
		protected virtual void EnsureReadOnly(IDescriptionPackage package) {
			package.EnsureNotNull(nameof(package));
			//
			var allTreeStructureComponents = package.Tree.TreeNode<IMetadataTreeStructureComponent>().SelfAndDescendants(i => i.ChildComponents);
			var allMetadataElements = allTreeStructureComponents.OfType<IMetadataTreeNode>().Select(i => i.MetadataElement).SkipNull();
			var allMetadata = allMetadataElements.Select(i => i.Metadata).SkipNull();
			var allReadOnlyScopes = ((IReadOnlyScope)package).Sequence().Concat(allTreeStructureComponents).Concat(allMetadataElements).Concat(allMetadata);
			foreach (var readOnlyScope in allReadOnlyScopes) {
				if (!readOnlyScope.IsReadOnly)
					throw new EonException(message: $"Specified component of description package is not in read-only state as expected.{Environment.NewLine}\tPackage:{package.FmtStr().GNLI2()}{Environment.NewLine}\tComponent:{readOnlyScope.FmtStr().GNLI2()}");
			}
		}

		// TODO: Put strings into the resources.
		//
		protected virtual void Validate(IDescriptionPackage package, IDescriptionPackageLoadContext loadCtx) {
			package.EnsureNotNull(nameof(package));
			loadCtx.EnsureNotNull(nameof(loadCtx));
			//
			loadCtx.ThrowIfCancellationRequested();
			var allMetadata = package.AllMetadata.SkipNull().ToArray();
			// Проверка имён метаданных, их уникальности в пределах их уровня иерархии (родительский элемент - дочерний элемент).
			//
			var childNameDictionary = new Dictionary<IMetadata, HashSet<MetadataName>>(comparer: ReferenceEqualityComparer<IMetadata>.Instance);
			var allMetadataUnqiueness = new HashSet<IMetadata>(comparer: ReferenceEqualityComparer<IMetadata>.Instance);
			var rootMetadata = default(IMetadata);
			for (var i = 0; i < allMetadata.Length; i++) {
				loadCtx.ThrowIfCancellationRequested();
				//
				var current = allMetadata[ i ];
				if (!allMetadataUnqiueness.Add(current))
					throw new MetadataValidationException(message: $"Specified metadata is duplicated. Description package metadata set (see '{nameof(IDescriptionPackage)}.{nameof(IDescriptionPackage.AllMetadata)}') contains two identical instances of mentioned metadata.{Environment.NewLine}\tDescription package:{package.FmtStr().GNLI2()}", metadata: current);
				var currentParent = current.Parent;
				if (currentParent != null && !currentParent.Children.Any(locChild => ReferenceEquals(locChild, current)))
					throw new MetadataValidationException(message: $"Specified metadata is not in child set (see property '{nameof(IMetadata)}.{nameof(IMetadata.Children)}') of its parent metadata (see property '{nameof(IMetadata)}.{nameof(IMetadata.Parent)}').{Environment.NewLine}\tDescription package:{package.FmtStr().GNLI2()}{Environment.NewLine}\tParent metadata:{currentParent.FmtStr().GNLI2()}", metadata: current);
				var currentName = current.Name;
				if (currentName is null)
					throw new MetadataValidationException(message: $"Specified metadata have not a name.{Environment.NewLine}\tDescription package:{package.FmtStr().GNLI2()}", metadata: current);
				else if (currentParent is null) {
					if (rootMetadata is null)
						rootMetadata = current;
					else
						throw new MetadataValidationException(message: $"Specified metadata have not a parent metadata and that metadata is not root in the description package.{Environment.NewLine}\tDescription package:{package.FmtStr().GNLI2()}{Environment.NewLine}\tRoot metadata:{rootMetadata.FmtStr().GNLI2()}", metadata: current);
				}
				else {
					if (!childNameDictionary.TryGetValue(currentParent, out var childrenNames))
						childNameDictionary.Add(currentParent, childrenNames = new HashSet<MetadataName>(comparer: MetadataNameEqualityComparer.Instance));
					if (!childrenNames.Add(currentName))
						throw new MetadataValidationException(message: $"Specified metadata name is not unique within its hierarchy level.{Environment.NewLine}\tDescription package:{package.FmtStr().GNLI2()}{Environment.NewLine}\tParent metadata:{currentParent.FmtStr().GNLI2()}", metadata: current);
				}
			}
			if (rootMetadata is null)
				throw new MetadataValidationException(message: $"Description package have not a root metadata.{Environment.NewLine}\tDescription package:{package.FmtStr().GNLI2()}", metadata: null);
			// Самовалидация.
			//
			for (var i = 0; i < allMetadata.Length; i++) {
				loadCtx.ThrowIfCancellationRequested();
				allMetadata[ i ].Validate();
			}
			// Проверить установку признака прохождения процедуры самовалидации.
			//
			for (var i = 0; i < allMetadata.Length; i++) {
				if (!allMetadata[ i ].IsValidated)
					throw new MetadataValidationException(FormatXResource(typeof(DefaultDescriptionPackageService), "ExceptionMessages/MetadataNotMarkedValidated", allMetadata[ i ]), allMetadata[ i ]);
			}
		}

		public async Task<IDescriptionPackage> CreateAndLoadAsync(IEnumerable<IDescription> descriptions, bool ownsDescription, IContext ctx = default) {
			ctx.ThrowIfCancellationRequested();
			var packageIdentity = new DescriptionPackageIdentity(name: MetadataName.NewName(prefix: "dyn-created-package"), publisherScopeId: UriBasedIdentifier.Undefined, version: new Version(major: 1, minor: 0), isReadOnly: true);
			var package = default(IDescriptionPackage);
			var loadState = default(P_LoadedPackageState);
			var loadContext = default(P_GeneratedPackageLoadContext);
			var caughtException = default(Exception);
			try {
				package = DescriptionPackage.Create(identity: packageIdentity, siteOrigin: DescriptionPackageUtilities.InMemoryGeneratedPackageBaseUri, descriptions: descriptions, ownsDescriptions: ownsDescription);
				ctx.ThrowIfCancellationRequested();
				loadState = new P_LoadedPackageState(service: this, package: package, locator: CreatePackageLocator(package: package), ownsPackage: true);
				loadState.AfterDisposed += P_EH_LoadedPackageState_AfterDisposed;
				loadContext = new P_GeneratedPackageLoadContext(locator: loadState.Locator);
				await LoadMaterializedAsync(package: package, loadCtx: loadContext).ConfigureAwait(false);
				ReadDA(ref _loadedPackagesSpinLock).Invoke(() => ReadDA(ref _loadedPackages).Add(key: loadState.Locator, value: loadState));
				return package;
			}
			catch (Exception exception) {
				caughtException = exception;
				throw;
			}
			finally {
				if (!(caughtException != null)) {
					package?.Dispose(caughtException);
					loadState?.Dispose(caughtException);
				}
				loadContext?.Dispose(caughtException);
			}
		}

		public async Task<IDescriptionPackage> CreateAndLoadAsync(IDescription description, bool ownsDescription, IContext ctx = default)
			=> await CreateAndLoadAsync(descriptions: description.EnsureNotNull(nameof(description)).Value.Sequence(), ownsDescription: ownsDescription, ctx: ctx).ConfigureAwait(false);

		// TODO: Put strings into the resources.
		//
		public virtual async Task<TDescription> LoadDescriptionAsync<TDescription>(DescriptionLocator locator, IContext ctx = default)
			where TDescription : class, IDescription {
			//
			var package = await LoadAsync(locator: locator, ctx: ctx).ConfigureAwait(false);
			try {
				return package.RequireMetadata<TDescription>(fullName: locator.DescriptionPath);
			}
			catch (Exception exception) {
				throw new EonException(message: $"An error occurred while loading description (configuration) for the specified locator (reference).{Environment.NewLine}\tLocator (reference):{locator.FmtStr().GNLI2()}", innerException: exception);
			}
		}

		public virtual DescriptionPackageLocator CreateSatellitePackageLocator(DescriptionPackageLocator locator, MetadataName satelliteName) {
			locator.EnsureNotNull(nameof(locator));
			satelliteName.EnsureNotNull(nameof(satelliteName));
			//
			return new DescriptionPackageLocator(other: locator, satellitePackageName: satelliteName);
		}

		public virtual DescriptionLocator CreateSatelliteDescriptionLocator(DescriptionPackageLocator locator, MetadataName satelliteName, MetadataPathName satelliteDescriptionPath) {
			locator.EnsureNotNull(nameof(locator));
			satelliteName.EnsureNotNull(nameof(satelliteName));
			satelliteDescriptionPath.EnsureNotNull(nameof(satelliteDescriptionPath));
			//
			return new DescriptionLocator(other: locator, descriptionPath: satelliteDescriptionPath, satellitePackageName: satelliteName);
		}

		public IDescriptionPackage[ ] GetLoaded()
			=> ReadDA(ref _loadedPackagesSpinLock).Invoke(() => ReadDA(ref _loadedPackages).Values.Select(locItem => locItem.GetPackageIfLoaded()).SkipNull().ToArray());

		void P_EH_LoadedPackageState_AfterDisposed(object sender, DisposeEventArgs e) {
			var state = sender.EnsureNotNull(nameof(sender)).EnsureOfType<P_LoadedPackageState>().Value;
			e.EnsureNotNull(nameof(e));
			//
			if (e.ExplicitDispose) {
				TryReadDA(ref _loadedPackagesSpinLock)
					?.Invoke(
						() => {
							var locLoadedPackages = TryReadDA(ref _loadedPackages);
							if (!(locLoadedPackages is null)) {
								foreach (var y in locLoadedPackages.Where(x => x.Value == state).ToArray())
									locLoadedPackages.Remove(y.Key);
							}
						});
			}
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_loadedPackagesSpinLock?.EnterAndExitLock();
				_loadedPackages?.Values.Observe(y => y.Dispose());
				_loadedPackages?.Clear();
			}
			_loadedPackages = null;
			_loadedPackagesSpinLock = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}