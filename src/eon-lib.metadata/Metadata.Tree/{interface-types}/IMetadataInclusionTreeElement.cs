using System;

namespace Eon.Metadata.Tree {

	/// <summary>
	/// Определяет элемент дерева метаданных (см. <seealso cref="IMetadataTreeElement"/>) как включение метаданных <see cref="IMetadata"/> из источника <see cref="LocationUri"/>.
	/// </summary>
	public interface IMetadataInclusionTreeElement
		:IMetadataTreeElement {

		/// <summary>
		/// Возвращает URI расположения метаданных.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// <para>Не может быть относительным.</para>
		/// </summary>
		Uri LocationUri { get; set; }

		/// <summary>
		/// Возвращает медиа-тип формата включения метаданных.
		/// </summary>
		string FormatMediaTypeName { get; set; }

	}

}