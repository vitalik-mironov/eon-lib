using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

using Eon.Collections.Trees;
using Eon.Linq;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

using static Eon.DisposableUtilities;

namespace Eon.Metadata.Tree {

	[DataContract]
	public class MetadataTreeNode
		:MetadataTreeStructureComponentBase, IMetadataTreeNode {

		[DataMember(Order = 0, Name = nameof(MetadataElement), IsRequired = true)]
		IMetadataTreeElement _metadataElement;

		ILink<IMetadataTreeNode, IMetadataTreeElement> _metadataElementLink;

		MetadataTreeNodeContainer _children;
		[DataMember(Order = 1, Name = "Nodes", IsRequired = false, EmitDefaultValue = false)]
		MetadataTreeNodeContainer P_Nodes_DataMember {
			get {
				var children = ReadDA(ref _children);
				return children?.Count > 0 ? children : null;
			}
			set { WriteDA(ref _children, value); }
		}

		MetadataTreeNodeInclusionContainer _inclusions;
		[DataMember(Order = 2, Name = nameof(Inclusions), IsRequired = false, EmitDefaultValue = false)]
		MetadataTreeNodeInclusionContainer P_Inclusions_DataMember {
			get {
				var inclusions = ReadDA(ref _inclusions);
				return inclusions?.Count > 0 ? inclusions : null;
			}
			set { WriteDA(ref _inclusions, value); }
		}

		public MetadataTreeNode(string caption = null, IMetadataTreeElement metadataElement = null, ReadOnlyStateTag readOnlyState = null)
			: this(parent: null, caption: caption, metadataElement: metadataElement, readOnlyState: readOnlyState) { }

		public MetadataTreeNode(IMetadataTreeNode parent, string caption = null, IMetadataTreeElement metadataElement = default, ReadOnlyStateTag readOnlyState = default)
			: base(caption: caption, readOnlyState: readOnlyState) {
			//
			_children = new MetadataTreeNodeContainer(this);
			_inclusions = new MetadataTreeNodeInclusionContainer(this);
			//
			if (metadataElement != null)
				SetMetadataElement(link: new Link<MetadataTreeNode, IMetadataTreeElement>(this, metadataElement));
			if (parent != null)
				parent.Children.AddComponent(this);
		}

		[JsonConstructor]
		protected MetadataTreeNode(SerializationContext ctx)
			: base(ctx: ctx) { }

		public new IMetadataTreeNode Root
			=> (IMetadataTreeNode)base.Root;

		public new IMetadataTreeNode Parent
			=> (IMetadataTreeNode)base.Parent;

		public ILink<IMetadataTreeNode, IMetadataTreeElement> MetadataElementLink
			=> ReadDA(ref _metadataElementLink);

		public IMetadataTreeElement MetadataElement
			=> MetadataElementLink?.Target;

		// TODO: Put strings into the resources.
		//
		public void SetMetadataElement(ILink<IMetadataTreeNode, IMetadataTreeElement> link) {
			link.EnsureNotNull(nameof(link));
			//
			var existingLink = ReadDA(ref _metadataElementLink);
			if (!ReferenceEquals(link, existingLink)) {
				// Проверка линка.
				//
				var linkNode = link.Source;
				var linkElement = link.Target;
				if (linkNode == null)
					throw new ArgumentException("Для указанной связи не установлен узел дерева метаданных.", nameof(link));
				else if (linkElement == null)
					throw new ArgumentException("Для указанной связи не установлен элемент дерева метаданных.", nameof(link));
				else if (!ReferenceEquals(linkNode, this))
					throw new ArgumentException($"Узел дерева метаданных '{linkNode}', установленный в указанной связи, не соответствует данному узлу '{this}'.", nameof(link));
				//
				this.EnsureNotReadOnly();
				//
				OnSettingMetadataElement(link);
				if ((existingLink = WriteDA(ref _metadataElementLink, link, null)) == null) {
					try {
						linkElement.SetNode(link);
						WriteDA(ref _metadataElement, linkElement);
					}
					catch {
						Interlocked.CompareExchange(ref _metadataElementLink, existingLink, link);
						throw;
					}
				}
				else if (!ReferenceEquals(existingLink, link))
					throw new EonException($"Изменение существующей связи узла дерева метаданных с элементом дерева метаданных не допускается. Прежде существующая связь должна быть явно удалена методом '{nameof(RemoveMetadataElement)}'.");
			}
		}

		protected virtual void OnSettingMetadataElement(ILink<IMetadataTreeNode, IMetadataTreeElement> link) {
			link.EnsureNotNull(nameof(link));
			//
		}

		public void RemoveMetadataElement() {
			var existingLink = ReadDA(ref _metadataElementLink);
			if (existingLink != null) {
				this.EnsureNotReadOnly();
				OnRemovingMetadataElement(existingLink);
				ILink<IMetadataTreeNode, IMetadataTreeElement> compareExchangeResult = null;
				try {
					compareExchangeResult = Interlocked.CompareExchange(ref _metadataElementLink, null, existingLink);
					if (ReferenceEquals(compareExchangeResult, existingLink))
						existingLink.Target.RemoveNode();
				}
				catch {
					Interlocked.CompareExchange(ref _metadataElementLink, compareExchangeResult, null);
					throw;
				}
			}
		}

		protected virtual void OnRemovingMetadataElement(ILink<IMetadataTreeNode, IMetadataTreeElement> link) {
			link.EnsureNotNull(nameof(link));
			//
		}

		public IMetadataTreeStructureContainer<IMetadataTreeNode, IMetadataTreeNode> Children
			=> ReadDA(ref _children);

		public IMetadataTreeStructureContainer<IMetadataTreeNode, IMetadataTreeNodeInclusion> Inclusions
			=> ReadDA(ref _inclusions);

		protected sealed override void PopulateCopy(CopyableReadOnlyScopeBase.CopyArgs args, MetadataTreeStructureComponentBase copy) {
			var locArgs =
				args
				.Arg(nameof(args))
				.EnsureOfType<CopyableReadOnlyScopeBase.CopyArgs, CopyArgs>()
				.Value;
			var locCopy =
				copy
				.EnsureNotNull(nameof(copy))
				.EnsureOfType<MetadataTreeStructureComponentBase, MetadataTreeNode>()
				.Value;
			//
			locCopy._children = new MetadataTreeNodeContainer(locCopy);
			locCopy._inclusions = new MetadataTreeNodeInclusionContainer(locCopy);
			locCopy._metadataElementLink = null;
			//
			PopulateCopy(locArgs, locCopy);
		}

		protected virtual void PopulateCopy(CopyArgs args, MetadataTreeNode copy) {
			copy.EnsureNotNull(nameof(copy));
			//
			var copyMetadataElement = default(IMetadataTreeElement);
			var copyChildrenBuffer = new List<IMetadataTreeNode>();
			var copyInclusionsBuffer = new List<IMetadataTreeNodeInclusion>();
			try {
				var copyChildren = copy.ReadDA(ref copy._children);
				foreach (var childOfThis in Children) {
					copyChildrenBuffer.Add(childOfThis.CreateCopy(readOnlyState: null));
					copyChildren.AddComponent(copyChildrenBuffer[ copyChildrenBuffer.Count - 1 ]);
				}
				//
				var copyInclusions = copy.ReadDA(ref copy._inclusions);
				foreach (var inclusionOfThis in Inclusions) {
					copyInclusionsBuffer.Add(inclusionOfThis.CreateCopy(readOnlyState: null));
					copyInclusions.AddComponent(copyInclusionsBuffer[ copyInclusionsBuffer.Count - 1 ]);
				}
				//
				copyMetadataElement = MetadataElement?.CreateCopy(readOnlyState: null);
				var copyMetadataElementLink = new Link<IMetadataTreeNode, IMetadataTreeElement>(copy, copyMetadataElement);
				copy.SetMetadataElement(copyMetadataElementLink);
				//
			}
			catch (Exception firstException) {
				DisposeManyDeep(firstException, copyMetadataElement, copyInclusionsBuffer, copyChildrenBuffer);
				throw;
			}
		}

		public new MetadataTreeNode CreateCopy(ReadOnlyStateTag readOnlyState = null)
			=> (MetadataTreeNode)CreateCopy(new CopyArgs(readOnlyState));

		IMetadataTreeNode IMetadataTreeNode.CreateCopy(ReadOnlyStateTag readOnlyState)
			=> CreateCopy(readOnlyState: readOnlyState);

		protected sealed override void OnSettingParent(ILink<IMetadataTreeStructureComponent, IMetadataTreeStructureComponent> link) {
			var locLink =
				link
				.Arg(nameof(link))
				.EnsureOfType<ILink<IMetadataTreeStructureComponent, IMetadataTreeStructureComponent>, ILink<IMetadataTreeNode, IMetadataTreeNode>>()
				.Value;
			//
			OnSettingParent(locLink);
		}

		protected virtual void OnSettingParent(ILink<IMetadataTreeNode, IMetadataTreeNode> link) {
			link.EnsureNotNull(nameof(link));
			//
		}

		protected sealed override void OnRemovingParent(ILink<IMetadataTreeStructureComponent, IMetadataTreeStructureComponent> link) {
			var locLink =
				link
				.Arg(nameof(link))
				.EnsureOfType<ILink<IMetadataTreeStructureComponent, IMetadataTreeStructureComponent>, ILink<IMetadataTreeNode, IMetadataTreeNode>>()
				.Value;
			//
			OnRemovingParent(locLink);
		}

		protected virtual void OnRemovingParent(ILink<IMetadataTreeNode, IMetadataTreeNode> link) {
			link.EnsureNotNull(nameof(link));
			//
		}

		public IMetadataTreeNode PreviousSibling
			=> Parent?.Children.GetBefore(this);

		public IMetadataTreeNode NextSibling
			=> Parent?.Children.GetAfter(this);

		public override bool HasChildComponent(IMetadataTreeStructureComponent component) {
			component.EnsureNotNull(nameof(component));
			//
			var componentAsNode = component as IMetadataTreeNode;
			var componentAsInclusion = component as IMetadataTreeNodeInclusion;
			return
				(componentAsNode != null && ReadDA(ref _children).Contains(componentAsNode))
				|| (componentAsInclusion != null && ReadDA(ref _inclusions).Contains(componentAsInclusion));
		}

		protected override void GetChildComponents(out IEnumerable<IMetadataTreeStructureComponent> components)
			=> components = Children.Concat<IMetadataTreeStructureComponent>(Inclusions);

		protected override void OnDeserialized(StreamingContext context) {
			base.OnDeserialized(context);
			//
			var children = ReadDA(ref _children);
			if (children == null)
				WriteDA(ref _children, children = new MetadataTreeNodeContainer(this));
			children.SetOwnerComponent(this);
			//
			var inclusions = ReadDA(ref _inclusions);
			if (inclusions == null)
				WriteDA(ref _inclusions, inclusions = new MetadataTreeNodeInclusionContainer(this));
			inclusions.SetOwnerComponent(this);
			//
			ReadDA(ref _metadataElement)
				.Fluent().NullCond(metadataElement => SetMetadataElement(new Link<IMetadataTreeNode, IMetadataTreeElement>(this, metadataElement)));
		}

		protected override void OnSetReadOnly(ReadOnlyStateTag previousState, ReadOnlyStateTag newState) {
			this.EnsureChangeToPermanentReadOnly(newState: newState);
			//
			base.OnSetReadOnly(previousState, newState);
			//
			IReadOnlyScope metadataElementAsReadOnlyScope = ReadDA(ref _metadataElement, considerDisposeRequest: true);
			metadataElementAsReadOnlyScope?.SetReadOnly(isReadOnly: newState.IsReadOnly, isPermanent: newState.IsPermanent);
			//
			foreach (var childNode in EnumerateDA(ReadDA(ref _children, considerDisposeRequest: true), considerDisposeRequest: true).OfType<IReadOnlyScope>())
				childNode.SetReadOnly(isReadOnly: newState.IsReadOnly, isPermanent: newState.IsPermanent);
			//
			foreach (var inclusion in EnumerateDA(ReadDA(ref _inclusions, considerDisposeRequest: true), considerDisposeRequest: true).OfType<IReadOnlyScope>())
				inclusion.SetReadOnly(isReadOnly: newState.IsReadOnly, isPermanent: newState.IsPermanent);
		}

		public override string ToString()
			=>
			base.ToString()
			+ $"{Environment.NewLine}Кол-во дочерних узлов: '{(_children?.Count).FmtStr().Decimal()}'"
			+ $"{Environment.NewLine}Кол-во включений узлов: '{(_inclusions?.Count).FmtStr().Decimal()}'"
			+ $"{Environment.NewLine}Элемент метаданных:{Environment.NewLine}{_metadataElement.FmtStr().G().IndentLines()}";

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_children?.Dispose();
				_inclusions?.Dispose();
				_metadataElementLink?.Target?.Dispose();
			}
			_children = null;
			_inclusions = null;
			_metadataElementLink = null;
			base.Dispose(explicitDispose);
		}

	}

}