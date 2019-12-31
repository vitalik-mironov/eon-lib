using System;
using System.Threading.Tasks;

using Eon.Collections.Trees;
using Eon.Context;
using Eon.Threading.Tasks;

namespace Eon.Metadata.Tree {

	/// <summary>
	/// Определяет элемент дерева метаданных.
	/// <para>Элемент дерева метаданных — объект, представляющий связь метаданных (<see cref="IMetadata"/>, <see cref="Metadata"/>) и узла дерева (<see cref="IMetadataTreeNode"/>, <see cref="Node"/>), которому эти метаданные принадлежат.</para>
	/// </summary>
	public interface IMetadataTreeElement
		:IDisposable, IReadOnlyScope {

		ILink<IMetadataTreeElement, IVh<IMetadata>> MetadataLink { get; }

		IMetadata Metadata { get; }

		void RemoveMetadata();

		void SetMetadata(ILink<IMetadataTreeElement, IVh<IMetadata>> link);

		ILink<IMetadataTreeNode, IMetadataTreeElement> NodeLink { get; }

		IMetadataTreeNode Node { get; }

		void RemoveNode();

		void SetNode(ILink<IMetadataTreeNode, IMetadataTreeElement> link);

		/// <summary>
		/// Создает копию данного элемента, связывая копию с указанным узлом дерева метаданных <paramref name="node"/>.
		/// <para>Имеющаяся связь данного элемента с объектом метаданных (<seealso cref="MetadataLink"/>) не копируется.</para>
		/// </summary>
		/// <param name="node">Узел дерева метаданных, с которым в копии элемента будет установлена связь.</param>
		/// <param name="readOnlyState">Состояние доступности редактировании копии элемента.</param>
		/// <returns>Объект <see cref="IMetadataTreeElement"/>.</returns>
		IMetadataTreeElement CreateCopy(IMetadataTreeNode node, ReadOnlyStateTag readOnlyState = default);

		/// <summary>
		/// Создает копию данного элемента.
		/// <para>Имеющаяся связь данного элемента с объектом метаданных (<seealso cref="MetadataLink"/>) не копируется.</para>
		/// </summary>
		/// <param name="readOnlyState">Состояние доступности редактировании копии элемента.</param>
		/// <returns>Объект <see cref="IMetadataTreeElement"/>.</returns>
		IMetadataTreeElement CreateCopy(ReadOnlyStateTag readOnlyState = default);

		/// <summary>
		/// Выполняет асинхронную загрузку метаданных.
		/// </summary>
		/// <param name="loadCtx">
		/// Контекст загрузки метаданных.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <returns>Объект <see cref="ITaskWrap{TResult}"/>.</returns>
		Task<IVh<IMetadata>> LoadMetadataAsync(IMetadataLoadContext loadCtx);

	}

}