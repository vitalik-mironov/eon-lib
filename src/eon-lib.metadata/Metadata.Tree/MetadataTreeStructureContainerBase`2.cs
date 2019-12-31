using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Eon.Collections;
using Eon.Collections.Trees;
using Eon.Threading;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Metadata.Tree {

	public abstract class MetadataTreeStructureContainerBase<TOwner, TComponent>
		:Disposable, IMetadataTreeStructureContainer<TOwner, TComponent>
		where TOwner : class, IMetadataTreeStructureComponent
		where TComponent : class, IMetadataTreeStructureComponent {

		#region Nested types

		sealed class P_LinkTag {

			public readonly LinkedListNode<TComponent> InnerListNode;

			internal P_LinkTag(LinkedListNode<TComponent> innerListNode) {
				innerListNode.EnsureNotNull(nameof(innerListNode));
				//
				InnerListNode = innerListNode;
			}

		}

		sealed class P_Link
			:Link<TOwner, TComponent> {

			internal P_Link(TOwner parent, TComponent child, P_LinkTag tag)
				: base(source: parent, target: child, tag: tag.EnsureNotNull(nameof(tag)).Value) { }

			public new P_LinkTag Tag
				=> (P_LinkTag)base.Tag;

		}

		struct P_LinkOperationArgs {

			public TOwner OwnerComponent;

			public LinkedList<TComponent> InnerList;

			public PrimitiveSpinLock InnerListSpinLock;

			public ILink<TOwner, TComponent> Link;

			public TOwner LinkParent;

			public TComponent LinkChild;

			public ILink<IMetadataTreeStructureComponent, IMetadataTreeStructureComponent> LinkChildParentLink;

			public P_LinkTag LinkTag;

		}

		#endregion

		TOwner _ownerComponent;

		LinkedList<TComponent> _innerList;

		PrimitiveSpinLock _innerListSpinLock;

		protected MetadataTreeStructureContainerBase() {
			_ownerComponent = null;
			_innerListSpinLock = new PrimitiveSpinLock();
			_innerList = new LinkedList<TComponent>();
		}

		protected MetadataTreeStructureContainerBase(TOwner ownerComponent) {
			ownerComponent.EnsureNotNull(nameof(ownerComponent));
			//
			_ownerComponent = ownerComponent;
			_innerListSpinLock = new PrimitiveSpinLock();
			_innerList = new LinkedList<TComponent>();
		}

		public bool IsReadOnly
			=> ReadDA(ref _ownerComponent)?.IsReadOnly ?? false;

		public int Count
			=> ReadDA(ref _innerList).Count;

		public bool HasOwnerComponent
			=> ReadDA(ref _ownerComponent) != null;

		// TODO: Put strings into the resources.
		//
		public TOwner OwnerComponent {
			get {
				var result = ReadDA(ref _ownerComponent);
				if (result == null)
					throw new EonException($"Компонент-владелец у контейнера компонентов не установлен.{Environment.NewLine}\tКонтейнер:{Environment.NewLine}{this.FmtStr().G().IndentLines2()}");
				else
					return result;
			}
		}

		protected void EnsureNotReadOnly() {
			if (IsReadOnly)
				throw new EonException(FormatXResource(typeof(InvalidOperationException), "CanNotModifyObjectDueReadOnly", this));
		}

		// TODO: Put strings into the resources.
		//
		P_LinkOperationArgs P_ValidateLinkAndGetLinkOperationArgs(ILink<TOwner, TComponent> parentLink) {
			parentLink.EnsureNotNull(nameof(parentLink));
			//
			var operationArgs = default(P_LinkOperationArgs);
			operationArgs.Link = parentLink;
			operationArgs.LinkParent = parentLink.Source;
			operationArgs.LinkChild = parentLink.Target;
			operationArgs.LinkChildParentLink = operationArgs.LinkChild?.ParentLink;
			operationArgs.LinkTag = parentLink.Tag as P_LinkTag;
			operationArgs.OwnerComponent = OwnerComponent;
			operationArgs.InnerList = ReadDA(ref _innerList);
			operationArgs.InnerListSpinLock = ReadDA(ref _innerListSpinLock);
			if (operationArgs.LinkParent == null)
				throw new ArgumentException("Для указанной связи не установлен узел-родитель.", nameof(parentLink));
			else if (operationArgs.LinkChild == null)
				throw new ArgumentException("Для указанной связи не установлен дочерний узел.", nameof(parentLink));
			else if (!ReferenceEquals(operationArgs.LinkParent, OwnerComponent))
				throw new ArgumentException($"Узел-родитель '{operationArgs.LinkParent}' указанной связи не соответствует узлу-владельцу '{operationArgs.OwnerComponent}' данного контейнера узлов.", nameof(parentLink));
			else if (!ReferenceEquals(operationArgs.LinkChildParentLink, parentLink))
				throw new ArgumentException($"Связь родитель-потомок, указанная узлом-потомком '{operationArgs.LinkChild}' не соответствует указанной для данной операции связи.", nameof(parentLink));
			else if (operationArgs.LinkTag == null)
				throw new ArgumentException($"Для указанной связи не установлен требуемый для данной операции тэг. Возможно, эзмепляр указанной связи не был создан именно данным контейнером узлов.", nameof(parentLink));
			else if (!(ReferenceEquals(operationArgs.LinkTag.InnerListNode.Value, operationArgs.LinkChild) && (operationArgs.LinkTag.InnerListNode.List == null || ReferenceEquals(operationArgs.InnerList, operationArgs.LinkTag.InnerListNode.List))))
				throw new ArgumentException("Указанная связь имеет недопустимый тэг.", nameof(parentLink));
			else
				return operationArgs;
		}

		// TODO: Put strings into the resources.
		//
		public void SetOwnerComponent(TOwner ownerComponent) {
			ownerComponent.EnsureNotNull(nameof(ownerComponent));
			//
			var original = WriteDA(location: ref _ownerComponent, value: ownerComponent, comparand: null);
			if (original is null) {
				var innerListNode = ReadDA(ref _innerList).First;
				for (; innerListNode != null;) {
					innerListNode.Value.SetParent(new P_Link(ownerComponent, innerListNode.Value, new P_LinkTag(innerListNode)));
					innerListNode = innerListNode.Next;
				}
			}
			else if (!ReferenceEquals(original, ownerComponent))
				throw new EonException($"Изменение компонента-владельца у контейнера компонентов не допускается.{Environment.NewLine}\tКонтейнер:{Environment.NewLine}{ToString().IndentLines2()}");
		}

		bool P_Contains(ILink<TOwner, TComponent> link) {
			link.EnsureNotNull(nameof(link));
			//
			return ReferenceEquals(ReadDA(ref _innerList), (link.Tag as P_LinkTag)?.InnerListNode.List);
		}

		bool P_Contains(TComponent component, out ILink<TOwner, TComponent> link) {
			component.EnsureNotNull(nameof(component));
			//
			var itemParentLink = component.ParentLink;
			ILink<TOwner, TComponent> itemParentLinkStricted;
			if (itemParentLink == null || (itemParentLinkStricted = itemParentLink as ILink<TOwner, TComponent>) == null) {
				link = null;
				return false;
			}
			else if (P_Contains(itemParentLinkStricted)) {
				link = itemParentLinkStricted;
				return true;
			}
			else {
				link = null;
				return false;
			}
		}

		public bool Contains(TComponent component) {
			component.EnsureNotNull(nameof(component));
			//
			var link = component.ParentLink as ILink<TOwner, TComponent>;
			return link != null && P_Contains(link);
		}

		// TODO: Put strings into the resources.
		//
		void ICollection<TComponent>.Add(TComponent component) {
			var owner = ReadDA(ref _ownerComponent, considerDisposeRequest: true);
			if (owner is null) {
				if (component is null)
					throw
						new SerializationException(
							message: $"Контейнер не может содержать элемент '{component.FmtStr().G()}'.\tКонтейнер:{Environment.NewLine}{this.FmtStr().G().IndentLines2()}");
				else
					ReadDA(ref _innerList).AddLast(value: component);
			}
			else
				AddComponent(component: component);
		}

		public void AddLink(ILink<TOwner, TComponent> link) {
			link.EnsureNotNull(nameof(link));
			//
			var operationArgs = P_ValidateLinkAndGetLinkOperationArgs(link);
			if (operationArgs.LinkTag.InnerListNode.List is null) {
				EnsureNotReadOnly();
				operationArgs
					.InnerListSpinLock
					.Invoke(
						() => {
							EnsureNotReadOnly();
							//
							operationArgs.InnerList.AddLast(operationArgs.LinkTag.InnerListNode);
						});
			}
		}

		// TODO: Put strings into the resources.
		//
		public ILink<TOwner, TComponent> AddComponent(TComponent component) {
			component.EnsureNotNull(nameof(component));
			//
			ILink<TOwner, TComponent> link;
			if (P_Contains(component, out link))
				return link;
			else if (component.IsReadOnly)
				throw new ArgumentException($"Указанный компонент является недоступным для редактирования.\t{Environment.NewLine}\tКомпонент:{Environment.NewLine}{component.FmtStr().G().IndentLines2()}", nameof(component));
			else if (component.Parent != null)
				throw new ArgumentException($"Указанный компонент принадлежит другому компоненту-родителю.\t{Environment.NewLine}\tКомпонент:{Environment.NewLine}{component.FmtStr().G().IndentLines2()}{Environment.NewLine}\tКомпонент-родитель:{Environment.NewLine}{component.Parent.FmtStr().G().IndentLines2()}", nameof(component));
			else {
				var parent = OwnerComponent;
				if (parent.IsDescendantOf(component))
					throw new ArgumentException($"Указанный компонент уже входит в структуру компонента-владельца контейнера.{Environment.NewLine}\tКомпонент:{Environment.NewLine}{component.FmtStr().G().IndentLines2()}{Environment.NewLine}\tКонтейнер:{Environment.NewLine}{this.FmtStr().G().IndentLines2()}", nameof(component));
				//
				EnsureNotReadOnly();
				var innerList = ReadDA(ref _innerList);
				var innerListSpinLock = ReadDA(ref _innerListSpinLock);
				var locLink = new P_Link(parent, component, new P_LinkTag(new LinkedListNode<TComponent>(component)));
				try {
					innerListSpinLock
						.Invoke(
							() => {
								EnsureNotReadOnly();
								//
								innerList.AddLast(locLink.Tag.InnerListNode);
							});
					component.SetParent(locLink);
					return locLink;
				}
				catch (Exception firstException) {
					try {
						innerListSpinLock.Invoke(() => locLink.Tag.InnerListNode.TryRemove());
					}
					catch (Exception secondException) {
						throw firstException.ToAggregateException(secondException);
					}
					throw;
				}
			}
		}

		public bool Remove(TComponent component) {
			component.EnsureNotNull(nameof(component));
			//
			var link = component.ParentLink as ILink<TOwner, TComponent>;
			if (link == null)
				return false;
			else
				return Remove(link);
		}

		// TODO: Put strings into the resources.
		//
		public bool Remove(ILink<TOwner, TComponent> link) {
			link.EnsureNotNull(nameof(link));
			//
			if (P_Contains(link)) {
				var operationArgs = P_ValidateLinkAndGetLinkOperationArgs(link);
				EnsureNotReadOnly();
				//
				if (operationArgs.LinkChild.IsReadOnly)
					throw new EonException($"Указанный компонент недоступен для редактирования.{Environment.NewLine}\tКомпонент:{Environment.NewLine}{operationArgs.LinkChild.FmtStr().G().IndentLines2()}");
				operationArgs
					.InnerListSpinLock
					.Invoke(
						() => {
							EnsureNotReadOnly();
							//
							operationArgs.InnerList.Remove(operationArgs.LinkTag.InnerListNode);
						});
				operationArgs
					.LinkChild
					.RemoveParent();
				return true;
			}
			else
				return false;
		}

		public void Clear() {
			var innerList = ReadDA(ref _innerList);
			var innerListSpinLock = ReadDA(ref _innerListSpinLock);
			if (innerListSpinLock.Invoke(() => innerList.Count) > 0) {
				EnsureNotReadOnly();
				//
				var innerListNodesArray =
					innerListSpinLock
					.Invoke(
						() => {
							EnsureNotReadOnly();
							//
							var locInnerListNodesArray = new LinkedListNode<TComponent>[ innerList.Count ];
							var innerListNode = innerList.First;
							for (var i = 0; innerListNode != null; i++) {
								locInnerListNodesArray[ i ] = innerListNode;
								innerList.Remove(innerListNode);
								innerListNode = innerList.First;
							}
							return locInnerListNodesArray;
						});
				for (var y = 0; y < innerListNodesArray.Length; y++)
					innerListNodesArray[ y ].Value.RemoveParent();
			}
		}

		public void CopyTo(TComponent[ ] array, int arrayIndex)
			=> ReadDA(ref _innerList).CopyTo(array, arrayIndex);

		public IEnumerator<TComponent> GetEnumerator() {
			var innerList = ReadDA(ref _innerList);
			var innerListNode = innerList.First;
			for (; innerListNode != null;) {
				EnsureNotDisposeState();
				yield return innerListNode.Value;
				innerListNode = innerListNode.Next;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		// TODO: Put strings into the resources.
		//
		public TComponent GetBefore(TComponent component) {
			component.EnsureNotNull(nameof(component));
			//
			var linkTag = component.ParentLink?.Tag as P_LinkTag;
			var ownerComponent = OwnerComponent;
			if (linkTag == null || !ReferenceEquals(ReadDA(ref _innerList), linkTag.InnerListNode.List))
				throw new ArgumentException($"Указанный компонент не входит в структуру компонента-владельца данного контейнера.{Environment.NewLine}\tКомпонент:{Environment.NewLine}{component.FmtStr().G().IndentLines2()}{Environment.NewLine}\tКонтейнер:{Environment.NewLine}{this.FmtStr().G().IndentLines2()}", nameof(component));
			else
				return linkTag.InnerListNode.Previous?.Value;
		}

		// TODO: Put strings into the resources.
		//
		public TComponent GetAfter(TComponent component) {
			component.EnsureNotNull(nameof(component));
			//
			var linkTag = component.ParentLink?.Tag as P_LinkTag;
			var ownerComponent = OwnerComponent;
			if (linkTag == null || !ReferenceEquals(ReadDA(ref _innerList), linkTag.InnerListNode.List))
				throw new ArgumentException($"Указанный компонент не входит в структуру компонента-владельца данного контейнера.{Environment.NewLine}\tКомпонент:{Environment.NewLine}{component.FmtStr().G().IndentLines2()}{Environment.NewLine}\tКонтейнер:{Environment.NewLine}{this.FmtStr().G().IndentLines2()}", nameof(component));
			else
				return linkTag.InnerListNode.Next?.Value;
		}

		public override string ToString()
			=>
			$"Тип:{Environment.NewLine}{GetType().FmtStr().G().IndentLines()}"
			+ $"{Environment.NewLine}Компонент-владелец:{Environment.NewLine}{_ownerComponent.FmtStr().G().IndentLines()}";

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_innerListSpinLock?.EnterAndExitLock();
				//
				var innerList = _innerList;
				if (innerList != null) {
					var innerListNode = innerList.First;
					for (; innerListNode != null;) {
						innerListNode.Value?.Dispose();
						innerListNode = innerListNode.Next;
					}
				}
				_innerList?.Clear();
			}
			_innerList = null;
			_innerListSpinLock = null;
			_ownerComponent = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}