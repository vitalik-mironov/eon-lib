using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Eon.Collections.Trees;
using Eon.Linq;
using Eon.Metadata;
using Eon.Metadata.Tree;
using Eon.Xml.Schema;

namespace Eon.Description.Tree {

	[DataContract(Namespace = EonXmlNamespaces.Description.Package)]
	public class DescriptionTree
		:MetadataTreeNode, IDescriptionTree {

		#region Static members

		static IMetadataTreeElement P_CreateRootNsElement(IMetadataNamespace rootNamespace, bool ownsRootNamespace) {
			rootNamespace.EnsureNotNull(nameof(rootNamespace));
			//
			return new EmbeddedMetadataTreeElement(rootNamespace, ownsRootNamespace, linkEmbeddedMetadata: true);
		}

		#endregion

		IDescriptionPackage _package;

		public DescriptionTree(IMetadataNamespace rootNamespace, bool ownsRootNamespace, ReadOnlyStateTag readOnlyState = default)
			: base(parent: null, caption: null, metadataElement: P_CreateRootNsElement(rootNamespace, ownsRootNamespace), readOnlyState: readOnlyState) { }

		public virtual IDescriptionPackage Package
			=> ReadDA(ref _package);

		public new DescriptionTree Root
			=> (DescriptionTree)base.Root;

		IDescriptionTree IDescriptionTree.Root
			=> Root;

		public virtual IEnumerable<IMetadataTreeElement> AllMetadataElements
			=> ((IMetadataTreeNode)this)
			.TreeNode()
			.SelfAndDescendants(i => i.Children)
			.Select(i => i.MetadataElement)
			.SkipNull();

		protected override IReadOnlyScope OuterReadOnlyScope
			=> Package;

		// TODO: Put strings into the resources.
		//
		protected override void OnSettingMetadataElement(ILink<IMetadataTreeNode, IMetadataTreeElement> link) {
			base.OnSettingMetadataElement(link);
			//
			var metadata = link.Target?.Metadata;
			if (metadata == null)
				throw new EonException($"Указанная связь '{link}' не указывает на объект метаданных.");
			else if (!(metadata is IMetadataNamespace))
				throw new EonException($"Указанная связь '{link}' указывает на объект метаданных типа '{metadata.GetType()}', который не совместим с требуемым типом '{typeof(IMetadataNamespace)}'.");
		}

		// TODO: Put strings into the resources.
		//
		protected override void OnSettingParent(ILink<IMetadataTreeNode, IMetadataTreeNode> link) {
			base.OnSettingParent(link);
			//
			throw new EonException($"Узел дерева метаданных, который имет тип '{GetType()}', не может быть дочерним узлом какого-либо другого узла и, соответственно, иметь родительский узел.");
		}

		// TODO: Put strings into the resources.
		//
		public virtual void SetPackage(IDescriptionPackage package) {
			package.EnsureNotNull(nameof(package));
			//
			var existingPackage = ReadDA(ref _package);
			if (!ReferenceEquals(existingPackage, package)) {
				var packageTree = package.Tree;
				if (packageTree is null)
					throw
						new ArgumentException(message: $"Указанный пакет не имеет дерева описаний.{Environment.NewLine}\tПакет:{package.FmtStr().GNLI2()}", paramName: nameof(package));
				else if (!ReferenceEquals(this, packageTree))
					throw new ArgumentException($"Дерево описаний '{packageTree}' указанного пакета '{package}' не соответствует данному дереву '{this}'.", nameof(package));
				//
				EnsureNotReadOnly();
				if ((existingPackage = WriteDA(ref _package, package, null)) != null && !ReferenceEquals(existingPackage, package))
					throw new EonException($"Пакет, которому принадлежит данное дерево описаний '{this}', не может быть изменен. Пакет устанавливается для дерева только единажды.");
			}
		}

		protected sealed override void PopulateCopy(CopyArgs args, MetadataTreeNode copy) {
			var locCopy =
				copy
				.EnsureNotNull(nameof(copy))
				.EnsureOfType<MetadataTreeNode, DescriptionTree>()
				.Value;
			//
			locCopy._package = null;
			//
			PopulateCopy(args, locCopy);
			//
			base.PopulateCopy(args, copy);
		}

		protected virtual void PopulateCopy(CopyArgs args, DescriptionTree copy) {
			copy.EnsureNotNull(nameof(copy));
			//
		}

		// TODO: Put strings into the resources.
		//
		protected override void OnDeserialized(StreamingContext context) {
			base.OnDeserialized(context);
			//
			var metadataElement = MetadataElement;
			if (metadataElement is null)
				throw
					new SerializationException(
						message: $"Узел дерева метаданных типа '{GetType()}' обязательно должен быть связан с элементом дерева (свойство '{nameof(MetadataElement)}'). Значение указанного свойства не может быть '{metadataElement.FmtStr().G()}'.");
		}

		protected override void Dispose(bool explicitDispose) {
			_package = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}