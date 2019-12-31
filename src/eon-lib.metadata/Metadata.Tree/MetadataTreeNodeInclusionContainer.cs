using System.Runtime.Serialization;

namespace Eon.Metadata.Tree {

	/// <summary>
	/// Представляет реализацию контейнера включений узлов дерева метаданных.
	/// </summary>
	[CollectionDataContract(ItemName = CollectionDataContractItemName)]
	public class MetadataTreeNodeInclusionContainer
		:MetadataTreeStructureContainerBase<IMetadataTreeNode, IMetadataTreeNodeInclusion> {

		public const string CollectionDataContractItemName = "Inclusion";

		/// <summary>
		/// Создает экземпляр <seealso cref="MetadataTreeNodeInclusionContainer"/>.
		/// <para>Данный конструктор не предназначен для прямого использования из кода. Конструктор предназначен для выполнения требований контрактов данных коллекций.</para>
		/// </summary>
		MetadataTreeNodeInclusionContainer() { }

		public MetadataTreeNodeInclusionContainer(IMetadataTreeNode ownerNode)
			: base(ownerComponent: ownerNode) { }

	}

}