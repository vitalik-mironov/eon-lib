using System.Runtime.Serialization;

namespace Eon.Metadata.Tree {

	/// <summary>
	/// Представляет реализацию контейнера узлов дерева метаданных.
	/// </summary>
	[CollectionDataContract(ItemName = CollectionDataContractItemName)]
	public class MetadataTreeNodeContainer
		:MetadataTreeStructureContainerBase<IMetadataTreeNode, IMetadataTreeNode> {

		public const string CollectionDataContractItemName = "Node";

		/// <summary>
		/// Создает экземпляр <seealso cref="MetadataTreeNodeContainer"/>.
		/// <para>Данный конструктор не предназначен для прямого использования из кода. Конструктор предназначен для выполнения требований контрактов данных коллекций.</para>
		/// </summary>
		MetadataTreeNodeContainer() { }

		public MetadataTreeNodeContainer(IMetadataTreeNode ownerNode)
			: base(ownerComponent: ownerNode) { }

	}

}