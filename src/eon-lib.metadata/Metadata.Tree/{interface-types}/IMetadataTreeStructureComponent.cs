using System;
using System.Collections.Generic;

using Eon.Collections.Trees;

namespace Eon.Metadata.Tree {

	/// <summary>
	/// Определяет компонент структуры дерева метаданных <seealso cref="IMetadataTree"/>.
	/// </summary>
	public interface IMetadataTreeStructureComponent
		:IDisposable, IReadOnlyScope {

		/// <summary>
		/// Возвращает пользовательское наименование данного компонента.
		/// <para>Может быть null.</para>
		/// </summary>
		string Caption { get; set; }

		/// <summary>
		/// Вовзращает корневой компонент.
		/// </summary>
		IMetadataTreeStructureComponent Root { get; }

		/// <summary>
		/// Возвращает линк (связь) с родительским компонентом.
		/// </summary>
		ILink<IMetadataTreeStructureComponent, IMetadataTreeStructureComponent> ParentLink { get; }

		/// <summary>
		/// Возвращает родительский компонент.
		/// </summary>
		IMetadataTreeStructureComponent Parent { get; }

		/// <summary>
		/// Удаляет линк (связь) с родительским компонентом.
		/// </summary>
		void RemoveParent();

		/// <summary>
		/// Устанавливает линк (связь) с родительским компонентом.
		/// </summary>
		/// <param name="link">Объект связи (линк) с родительским компонентом.</param>
		void SetParent(ILink<IMetadataTreeStructureComponent, IMetadataTreeStructureComponent> link);

		/// <summary>
		/// Выполняет проверку, имеет ли данный компонент среди дочерних компонентов указанный компонент.
		/// </summary>
		/// <param name="component">
		/// Компонент.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <returns>Значение <see cref="bool"/>.</returns>
		bool HasChildComponent(IMetadataTreeStructureComponent component);

		/// <summary>
		/// Возвращает набор (последовательность) всех дочерних компонентов.
		/// <para>Не может быть null.</para>
		/// <para>Не может содержать null-элемент.</para>
		/// </summary>
		IEnumerable<IMetadataTreeStructureComponent> ChildComponents { get; }

	}

}