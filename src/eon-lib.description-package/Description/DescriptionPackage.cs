using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Eon.Description.Tree;
using Eon.Linq;
using Eon.Metadata;
using Eon.Metadata.Tree;
using Eon.Runtime.Options;
using Eon.Xml.Schema;

using static Eon.DisposableUtilities;
using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Description {

	[DataContract(Namespace = EonXmlNamespaces.Description.Package)]
	public sealed class DescriptionPackage
		:ReadOnlyScopeBase, IDescriptionPackage {

		#region Static members

		public static DescriptionPackage Create(DescriptionPackageIdentity identity, Uri siteOrigin, IEnumerable<IDescription> descriptions, DescriptionTreeFactory treeFactory = default, bool ownsDescriptions = default) {
			identity = identity.EnsureNotNull(nameof(identity)).AsReadOnly().EnsureValid();
			siteOrigin.EnsureNotNull(nameof(siteOrigin));
			var locDescriptions = descriptions.EnsureNotNull(nameof(descriptions)).EnsureNoNullElements().Value;
			treeFactory.EnsureNotNull(nameof(treeFactory));
			//
			var package = default(DescriptionPackage);
			var tree = default(IDescriptionTree);
			try {
				package = new DescriptionPackage() { _identity = identity };
				package.SetSiteOrigin(siteOrigin);
				tree = (treeFactory ?? DescriptionTreeFactoryOption.Require())(rootNamespace: new Namespace(name: MetadataName.NewName()), ownsRootNamespace: true);
				foreach (var description in locDescriptions)
					tree.Children.AddComponent(component: new MetadataTreeNode(metadataElement: new EmbeddedMetadataTreeElement(embeddedMetadata: description, ownsEmbeddedMetadata: ownsDescriptions)));
				package.WriteDA(location: ref package._tree, value: tree);
				tree.SetPackage(package);
				return package;
			}
			catch (Exception exception) {
				DisposeMany(exception, tree, package);
				throw;
			}
		}

		#endregion

		[DataMember(Order = 0, Name = nameof(Identity), IsRequired = true)]
		DescriptionPackageIdentity _identity;

		DotNetStrongAssemblyNameReference[ ] _referencedDotNetAssemblies;
		[DataMember(Order = 1, Name = nameof(ReferencedDotNetAssemblies), IsRequired = false, EmitDefaultValue = true)]
		IEnumerable<DotNetStrongAssemblyNameReference> P_ReferencedDotNetAssemblies_DataMember {
			get {
				var referencedDotNetAssemblies = ReadDA(ref _referencedDotNetAssemblies);
				return referencedDotNetAssemblies.IsNullOrEmpty() ? null : referencedDotNetAssemblies;
			}
			set => WriteDA(ref _referencedDotNetAssemblies, value?.SkipNull().ToArray());
		}

		[DataMember(Order = 2, Name = nameof(DefaultDescriptionRef), IsRequired = false, EmitDefaultValue = true)]
		MetadataReference<IDescription> _defaultDescription;

		[DataMember(Order = 3, Name = nameof(Tree), IsRequired = true)]
		IDescriptionTree _tree;

		Uri _siteOrigin;

		DescriptionPackage() { }

		protected override IReadOnlyScope OuterReadOnlyScope
			=> null;

		public DescriptionPackageIdentity Identity
			=> ReadDA(ref _identity);

		public IEnumerable<DotNetStrongAssemblyNameReference> ReferencedDotNetAssemblies
			=> EnumerateDA(ref _referencedDotNetAssemblies);

		// TODO: Удалить. В местах использования реализовать альтернативу.
		//
		public MetadataReference<IDescription> DefaultDescriptionRef
			=> ReadDA(ref _defaultDescription);

		public IDescription DefaultDescription
			=> DefaultDescriptionRef?.Resolve(RootNamespace);

		public IDescriptionTree Tree {
			get {
				var tree = ReadDA(ref _tree);
				if (tree is null)
					throw new EonException($"This package have not a description tree.{Environment.NewLine}\tPackage:{this.FmtStr().GNLI2()}");
				else
					return tree;
			}
		}

		IDescriptionTree IDescriptionPackage.Tree
			=> Tree;

		public bool HasTree
			=> ReadDA(ref _tree) != null;

		public IMetadataNamespace RootNamespace
			=> Tree.EnsureHasMetadataElement().EnsureHasMetadataOfType<IMetadataNamespace>();

		public bool HasSiteOrigin
			=> ReadDA(ref _siteOrigin) != null;

		// TODO: Put exception messages into the resources.
		//
		public Uri SiteOrigin {
			get {
				var siteOrigin = ReadDA(ref _siteOrigin);
				if (siteOrigin is null)
					throw new EonException($"Site origin is missing.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
				else
					return siteOrigin;
			}
		}

		public IEnumerable<IMetadata> AllMetadata
			=> ReferencedDotNetAssemblies.Concat(RootNamespace.SelfAndDescendants);

		protected override void OnSetReadOnly(ReadOnlyStateTag previousState, ReadOnlyStateTag newState) {
			this.EnsureChangeToPermanentReadOnly(newState: newState);
			//
			base.OnSetReadOnly(previousState: previousState, newState: newState);
			//
			if (newState.IsReadOnly)
				WriteDA(location: ref _identity, value: ReadDA(ref _identity, considerDisposeRequest: true)?.AsReadOnly());
			//
			foreach (var item in EnumerateDA(ref _referencedDotNetAssemblies, considerDisposeRequest: true).OfType<IReadOnlyScope>())
				item.SetReadOnly(isReadOnly: newState.IsReadOnly, isPermanent: newState.IsPermanent);
			//
			IReadOnlyScope treeAsReadOnlyScope = ReadDA(ref _tree, considerDisposeRequest: true);
			treeAsReadOnlyScope?.SetReadOnly(isReadOnly: newState.IsReadOnly, isPermanent: newState.IsPermanent);
		}

		public T RequireMetadata<T>(MetadataPathName fullName)
			where T : class, IMetadata
			=> RequireMetadata(locator: new MetadataPathNameRef<T>(fullName));

		public TMetadata RequireMetadata<TMetadata>(MetadataPathNameRef<TMetadata> locator)
			where TMetadata : class, IMetadata
			=> locator.EnsureNotNull(nameof(locator)).Value.Resolve(@base: RootNamespace);

		// TODO: Put strings into the resources.
		//
		public void SetSiteOrigin(Uri siteOrigin) {
			siteOrigin.EnsureNotNull(nameof(siteOrigin)).EnsureAbsolute();
			//
			this.EnsureNotReadOnly();
			var original = WriteDA(location: ref _siteOrigin, value: siteOrigin, comparand: null);
			if (!(original is null || original == siteOrigin))
				throw new EonException($"Site origin can't be changed.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}.");
		}

		public override string ToString()
			=> _identity.FmtStr().G();

		protected override void OnDeserialized(StreamingContext context) {
			base.OnDeserialized(context);
			//
			IDescriptionTree tree;
			if (ReadDA(ref _identity) is null)
				throw new SerializationException(FormatXResource(typeof(DescriptionPackage), "PackageIdentityCanNotBeNull"));
			else if ((tree = ReadDA(ref _tree)) is null)
				throw new SerializationException(FormatXResource(typeof(DescriptionPackage), "DescriptionTreeCanNotBeNull"));
			//
			tree.SetPackage(this);
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_tree?.Dispose();
				_referencedDotNetAssemblies.DisposeAndClearArray();
			}
			_defaultDescription = null;
			_tree = null;
			_identity = null;
			_referencedDotNetAssemblies = null;
			_siteOrigin = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}