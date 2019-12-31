using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Threading.Tasks;

namespace Eon.Metadata.Tree {

	/// <summary>
	/// Определяет включение узла(-ов) дерева метаданных (см. <seealso cref="IMetadataTreeNode"/>) из источника <see cref="LocationUri"/>.
	/// </summary>
	public interface IMetadataTreeNodeInclusion
		:IMetadataTreeStructureComponent {

		/// <summary>
		/// Возвращает URI загрузки узлов дерева метаданных.
		/// <para>Не может быть null.</para>
		/// <para>Не может быть относительным.</para>
		/// </summary>
		Uri LocationUri { get; set; }

		/// <summary>
		/// Выполняет асинхронную загрузку узлов дерева из указанного источника <seealso cref="LocationUri"/>.
		/// </summary>
		/// <param name="loadCtx">
		/// Контекст загрузки метаданных.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <returns>Объект <see cref="ITaskWrap{TResult}"/>.</returns>
		Task<IEnumerable<IMetadataTreeNode>> LoadNodesAsync(IMetadataLoadContext loadCtx);

		/// <summary>
		/// Создает копию данного включения, устанавливая у копии указанное состояние доступности редактирования.
		/// </summary>
		/// <param name="readOnlyState">Состояние достпуности редактирования копии.</param>
		/// <returns>Объект <see cref="IMetadataTreeNodeInclusion"/>.</returns>
		IMetadataTreeNodeInclusion CreateCopy(ReadOnlyStateTag readOnlyState = default);

	}

}