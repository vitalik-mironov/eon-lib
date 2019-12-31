
namespace Eon.Metadata.Tree {

	/// <summary>
	/// Определяет элемент дерева метаданных как встраивание метаданных в этот элемент.
	/// <para>Элемент дерева является носителем метаданных.</para>
	/// </summary>
	public interface IEmbeddedMetadataTreeElement
		:IMetadataTreeElement {

		/// <summary>
		/// Возвращает метаданные, встроенные в данный элемент.
		/// </summary>
		IMetadata EmbeddedMetadata { get; }

	}

}