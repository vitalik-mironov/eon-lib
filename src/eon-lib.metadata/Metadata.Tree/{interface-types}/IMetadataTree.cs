using System.Collections.Generic;

namespace Eon.Metadata.Tree {

	/// <summary>
	/// Определяет программный интерфейс дерева метаданных.
	/// </summary>
	public interface IMetadataTree
		:IMetadataTreeNode {

		/// <summary>
		/// Возвращает последовательность (набор) всех элементов дерева метаданных.
		/// <para>Не может быть null.</para>
		/// <para>Не может содержать null-элементов.</para>
		/// </summary>
		IEnumerable<IMetadataTreeElement> AllMetadataElements { get; }

	}

}