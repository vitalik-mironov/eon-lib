using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Eon.Collections.Trees;
using Eon.Context;
using Eon.Runtime.Serialization;

namespace Eon.Metadata.Tree {
#pragma warning disable IDE0034 // Simplify 'default' expression

	[DataContract]
	public abstract class MetadataTreeElementBase
		:CopyableReadOnlyScopeBase, IMetadataTreeElement {

		#region Nested types

		//[DebuggerStepThrough]
		//[DebuggerNonUserCode]
		protected new class CopyArgs
			:CopyableReadOnlyScopeBase.CopyArgs {

			public readonly IMetadataTreeNode Node;

			public CopyArgs(IMetadataTreeNode node, ReadOnlyStateTag readOnlyState)
				: base(readOnlyState) {
				Node = node;
			}

		}

		#endregion

		ILink<IMetadataTreeNode, IMetadataTreeElement> _nodeLink;

		ILink<IMetadataTreeElement, IVh<IMetadata>> _metadataLink;

		protected MetadataTreeElementBase(IMetadataTreeNode node, ReadOnlyStateTag readOnlyState = default(ReadOnlyStateTag))
			: base(readOnlyState: readOnlyState) {
			//
			if (node is null)
				_nodeLink = null;
			else {
				var nodeLink = _nodeLink = new Link<IMetadataTreeNode, IMetadataTreeElement>(source: node, target: this);
				node.SetMetadataElement(nodeLink);
			}
		}

		protected MetadataTreeElementBase(ReadOnlyStateTag readOnlyState = default(ReadOnlyStateTag))
			: this(node: null, readOnlyState: readOnlyState) { }

		protected MetadataTreeElementBase(SerializationContext ctx)
			: base(ctx: ctx) { }

		protected sealed override IReadOnlyScope OuterReadOnlyScope
			=> Node;

		public ILink<IMetadataTreeNode, IMetadataTreeElement> NodeLink
			=> ReadDA(ref _nodeLink);

		public IMetadataTreeNode Node
			=> ReadDA(ref _nodeLink)?.Source;

		// TODO: Put strings into the resources.
		//
		public void SetNode(ILink<IMetadataTreeNode, IMetadataTreeElement> link) {
			link.EnsureNotNull(nameof(link));
			//
			var existingLink = ReadDA(ref _nodeLink);
			if (!ReferenceEquals(link, existingLink)) {
				// Проверка аргумента.
				//
				var linkNode = link.Source;
				var linkElement = link.Target;
				if (linkNode == null)
					throw new ArgumentException(message: "У указанной связи отсутствует узел-владелец.", paramName: nameof(link));
				else if (!ReferenceEquals(linkElement, this))
					throw new ArgumentException(message: $"У указанной связи элемент не соответствует данному элементу.{Environment.NewLine}\tЭлемент связи ({nameof(link)}.{nameof(link.Target)}):{linkElement.FmtStr().GNLI2()}{Environment.NewLine}\tДанный элемент{this.FmtStr().GNLI2()}", paramName: nameof(link));
				//
				this.EnsureNotReadOnly();
				if ((existingLink = WriteDA(ref _nodeLink, link, null)) == null) {
					try {
						linkNode.SetMetadataElement(link);
					}
					catch {
						Interlocked.CompareExchange(ref _nodeLink, existingLink, link);
						throw;
					}
				}
				else if (!ReferenceEquals(existingLink, link))
					throw new EonException($"Изменение существующей связи с узлом дерева метаданных не допускается. Прежде существующая связь должна быть явно удалена методом '{nameof(RemoveNode)}'.");
			}
		}

		public void RemoveNode() {
			var existingLink = ReadDA(ref _nodeLink);
			if (existingLink != null) {
				this.EnsureNotReadOnly();
				//
				var compareExchangeResult = default(ILink<IMetadataTreeNode, IMetadataTreeElement>);
				try {
					compareExchangeResult = Interlocked.CompareExchange(ref _nodeLink, null, existingLink);
					if (ReferenceEquals(compareExchangeResult, existingLink))
						existingLink.Source.RemoveMetadataElement();
				}
				catch {
					if (ReferenceEquals(compareExchangeResult, existingLink))
						Interlocked.CompareExchange(ref _nodeLink, compareExchangeResult, null);
					throw;
				}
			}
		}

		public ILink<IMetadataTreeElement, IVh<IMetadata>> MetadataLink
			=> ReadDA(ref _metadataLink);

		public IMetadata Metadata
			=> ReadDA(ref _metadataLink)?.Target?.Value;

		// TODO: Put strings into the resources.
		//
		public void SetMetadata(ILink<IMetadataTreeElement, IVh<IMetadata>> metadataLink) {
			metadataLink.EnsureNotNull(nameof(metadataLink));
			//
			var existingLink = ReadDA(ref _metadataLink);
			if (!ReferenceEquals(metadataLink, existingLink)) {
				// Проверка аргумента.
				//
				var linkElement = metadataLink.Source;
				var linkMetadata = metadataLink.Target?.Value;
				if (linkMetadata == null)
					throw new ArgumentException("Для указанной связи не установлен объект метаданных.", nameof(metadataLink));
				else if (!ReferenceEquals(linkElement, this))
					throw new ArgumentException($"Для указанной связи элемент дерева метаданных '{linkElement}' не соответствует данному элементу '{this}'.", nameof(metadataLink));
				// Собственно установка.
				//
				this.EnsureNotReadOnly();
				if ((existingLink = WriteDA(ref _metadataLink, metadataLink, null)) == null) {
					try {
						linkMetadata.SetTreeElement(metadataLink);
					}
					catch {
						Interlocked.CompareExchange(ref _metadataLink, existingLink, metadataLink);
						throw;
					}
				}
				else if (!ReferenceEquals(existingLink, metadataLink))
					throw new EonException($"Изменение существующей связи с объектом метаданных не допускается. Прежде существующая связь должна быть явно удалена методом '{nameof(RemoveMetadata)}'.");
			}
		}

		public void RemoveMetadata() {
			var existingLink = ReadDA(ref _metadataLink);
			if (existingLink != null) {
				this.EnsureNotReadOnly();
				var compareExchangeResult = default(ILink<IMetadataTreeElement, IVh<IMetadata>>);
				try {
					compareExchangeResult = Interlocked.CompareExchange(ref _metadataLink, null, existingLink);
					if (ReferenceEquals(compareExchangeResult, existingLink))
						existingLink.Target?.Value?.RemoveTreeElement();
				}
				catch {
					if (ReferenceEquals(compareExchangeResult, existingLink))
						Interlocked.CompareExchange(ref _metadataLink, compareExchangeResult, null);
					throw;
				}
			}
		}

		/// <summary>
		/// Вызывается при копировании объекта.
		/// <para>Выполняет заполнение копии объекта <paramref name="copy"/>.</para>
		/// <para>Связь данного элемента с объектом метаданных (<seealso cref="MetadataLink"/>) в объект <paramref name="copy"/> не копируется.</para>
		/// </summary>
		/// <param name="args">Параметры копирования.</param>
		/// <param name="copy">Копия объекта.</param>
		protected sealed override void PopulateCopy(CopyableReadOnlyScopeBase.CopyArgs args, CopyableReadOnlyScopeBase copy) {
			var locArgs = args.Arg(nameof(args)).EnsureOfType<CopyableReadOnlyScopeBase.CopyArgs, CopyArgs>().Value;
			var locCopy = copy.EnsureNotNull(nameof(copy)).EnsureOfType<CopyableReadOnlyScopeBase, MetadataTreeElementBase>().Value;
			//
			locCopy._metadataLink = null;
			locCopy._nodeLink = null;
			PopulateCopy(locArgs, locCopy);
			base.PopulateCopy(args, copy);
		}

		/// <summary>
		/// Вызывается при копировании объекта.
		/// <para>Выполняет заполнение копии объекта <paramref name="copy"/>.</para>
		/// <para>Связь данного элемента с объектом метаданных (<seealso cref="MetadataLink"/>) в объект <paramref name="copy"/> не копируется.</para>
		/// </summary>
		/// <param name="args">Параметры копирования.</param>
		/// <param name="copy">Копия объекта.</param>
		protected virtual void PopulateCopy(CopyArgs args, MetadataTreeElementBase copy) {
			copy.EnsureNotNull(nameof(copy));
			//
			if (args.Node != null) {
				var link = new Link<IMetadataTreeNode, IMetadataTreeElement>(args.Node, copy);
				copy.WriteDA(ref copy._nodeLink, link);
				args.Node.SetMetadataElement(link);
			}
		}

		/// <summary>
		/// Создает копию данного элемента, связывая копию с указанным узлом дерева метаданных.
		/// <para>Связь данного элемента с объектом метаданных (<seealso cref="MetadataLink"/>) не копируется.</para>
		/// </summary>
		/// <param name="node">Узел дерева метаданных, с которым в копии будет установлена связь.</param>
		/// <param name="readOnlyState">Состояние доступности редактировании копии.</param>
		/// <returns>Объект <see cref="MetadataTreeElementBase"/>.</returns>
		public MetadataTreeElementBase CreateCopy(IMetadataTreeNode node, ReadOnlyStateTag readOnlyState = null)
			=> (MetadataTreeElementBase)CreateCopy(new CopyArgs(node, readOnlyState));

		IMetadataTreeElement IMetadataTreeElement.CreateCopy(IMetadataTreeNode node, ReadOnlyStateTag readOnlyState)
			=> CreateCopy(node, readOnlyState: readOnlyState);

		/// <summary>
		/// Создает копию данного элемента.
		/// <para>Связь данного элемента с объектом метаданных (<seealso cref="MetadataLink"/>) не копируется.</para>
		/// </summary>
		/// <param name="readOnlyState">Состояние доступности редактировании копии.</param>
		/// <returns>Объект <see cref="MetadataTreeElementBase"/>.</returns>
		public new MetadataTreeElementBase CreateCopy(ReadOnlyStateTag readOnlyState = default(ReadOnlyStateTag))
			=> CreateCopy(null, readOnlyState: readOnlyState);

		IMetadataTreeElement IMetadataTreeElement.CreateCopy(ReadOnlyStateTag readOnlyState)
			=> CreateCopy(null, readOnlyState: readOnlyState);

		protected abstract Task<IVh<IMetadata>> DoLoadMetadataAsync(IMetadataLoadContext loadCtx);

		public Task<IVh<IMetadata>> LoadMetadataAsync(IMetadataLoadContext loadCtx)
			=> DoLoadMetadataAsync(loadCtx: loadCtx);

		protected override void OnSetReadOnly(ReadOnlyStateTag previousState, ReadOnlyStateTag newState) {
			this.EnsureChangeToPermanentReadOnly(newState: newState);
			//
			base.OnSetReadOnly(previousState: previousState, newState: newState);
			//
			IReadOnlyScope metadataAsReadOnlyScope = ReadDA(ref _metadataLink, considerDisposeRequest: true)?.Target.Value;
			metadataAsReadOnlyScope?.SetReadOnly(isReadOnly: newState.IsReadOnly, isPermanent: newState.IsPermanent);
		}

		public override string ToString()
			=>
			$"Тип:{Environment.NewLine}{GetType().FmtStr().G().IndentLines()}"
			+ $"{Environment.NewLine}Метаданные:{Environment.NewLine}{(_metadataLink?.Target?.Value).FmtStr().G().IndentLines()}";

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose)
				_metadataLink?.Target?.Dispose();
			_nodeLink = null;
			_metadataLink = null;
			base.Dispose(explicitDispose);
		}

	}

#pragma warning restore IDE0034 // Simplify 'default' expression
}