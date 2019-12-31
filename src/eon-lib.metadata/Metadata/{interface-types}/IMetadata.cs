using System;
using System.Collections.Generic;

using Eon.Collections.Trees;
using Eon.ComponentModel.Dependencies;
using Eon.Metadata.Tree;

namespace Eon.Metadata {

	/// <summary>
	/// Определяет общее представление метаданных.
	/// </summary>
	public interface IMetadata
		:IDisposeNotifying, IDisposableDependencySupport, IReadOnlyScope {

		/// <summary>
		/// Возвращает имя метаданных.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </summary>
		MetadataName Name { get; set; }

		bool IsAutoName { get; }

		/// <summary>
		/// Возвращает уникальный идентификатор метаданных.
		/// <para>Не может быть <seealso cref="Guid.Empty"/>.</para>
		/// </summary>
		Guid Guid { get; }

		/// <summary>
		/// Возвращает полное имя метаданных.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </summary>
		MetadataPathName FullName { get; }

		IEnumerable<IMetadata> Children { get; }

		IEnumerable<IMetadata> Ancestors { get; }

		IEnumerable<IMetadata> Siblings { get; }

		IEnumerable<IMetadata> Descendants { get; }

		IEnumerable<IMetadata> SelfAndDescendants { get; }

		IMetadata Parent { get; }

		void Validate();

		bool IsValidated { get; }

		void EnsureValidated();

		ILink<IMetadataTreeElement, IVh<IMetadata>> TreeElementLink { get; }

		IMetadataTreeElement TreeElement { get; }

		void SetTreeElement(ILink<IMetadataTreeElement, IVh<IMetadata>> link);

		void RemoveTreeElement();

		IMetadata CreateCopy(IMetadataTreeElement element, ReadOnlyStateTag readOnlyState = default);

		IMetadata CreateCopy(ReadOnlyStateTag readOnlyState = default);

	}

}