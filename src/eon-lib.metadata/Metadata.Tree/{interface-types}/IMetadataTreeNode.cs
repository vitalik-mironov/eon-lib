using Eon.Collections.Trees;

namespace Eon.Metadata.Tree {

	/// <summary>
	/// Определяет программный интерфейс узла дерева метаданных.
	/// </summary>
	public interface IMetadataTreeNode
		:IMetadataTreeStructureComponent {

		/// <summary>
		/// Вовзращает корневой узел.
		/// </summary>
		new IMetadataTreeNode Root { get; }

		/// <summary>
		/// Возвращает родительский узел.
		/// </summary>
		new IMetadataTreeNode Parent { get; }

		/// <summary>
		/// Возвращает предшествующий данному узлу узел из числа дочерних узлов родительского узла.
		/// </summary>
		IMetadataTreeNode PreviousSibling { get; }

		/// <summary>
		/// Возвращает следующий после данного узла узел из числа дочерних узлов родительского узла.
		/// </summary>
		IMetadataTreeNode NextSibling { get; }

		/// <summary>
		/// Создает копию данного узла, устанавливая у копии указанное состояние доступности редактирования.
		/// </summary>
		/// <param name="readOnlyState">Состояние достпуности редактирования копии узла.</param>
		/// <returns>Объект <see cref="IMetadataTreeNode"/>.</returns>
		IMetadataTreeNode CreateCopy(ReadOnlyStateTag readOnlyState = default);

		/// <summary>
		/// Возвращает контейнер дочерних узлов.
		/// <para>Не может быть null.</para>
		/// <para>Не может содержать null-элемент.</para>
		/// </summary>
		IMetadataTreeStructureContainer<IMetadataTreeNode, IMetadataTreeNode> Children { get; }

		/// <summary>
		/// Вовзращает контейнер включений узлов дерева метаданных <seealso cref="IMetadataTreeNodeInclusion"/>.
		/// <para>Не может быть null.</para>
		/// <para>Не может содержать null-элемент.</para>
		/// </summary>
		IMetadataTreeStructureContainer<IMetadataTreeNode, IMetadataTreeNodeInclusion> Inclusions { get; }

		/// <summary>
		/// Возвращает объект, представляющий связь данного узла с элементом метаданных.
		/// </summary>
		ILink<IMetadataTreeNode, IMetadataTreeElement> MetadataElementLink { get; }

		/// <summary>
		/// Возвращает элемент метаданных данного узла.
		/// </summary>
		IMetadataTreeElement MetadataElement { get; }

		/// <summary>
		/// Устанавливает связь данного узла с элементом метаданных.
		/// </summary>
		/// <param name="link"></param>
		void SetMetadataElement(ILink<IMetadataTreeNode, IMetadataTreeElement> link);

		/// <summary>
		/// Удаляет связь данного узла с элементом метаданных.
		/// </summary>
		void RemoveMetadataElement();

	}

}