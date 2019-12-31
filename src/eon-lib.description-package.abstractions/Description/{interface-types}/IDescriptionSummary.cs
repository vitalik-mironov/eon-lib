using System;
using Eon.Metadata;

namespace Eon.Description {

	/// <summary>
	/// Представляет сводку сведений об описании (конфигурации) компонента (см. <seealso cref="IDescription"/>).
	/// </summary>
	public interface IDescriptionSummary {

		/// <summary>
		/// Возвращает уникальный идентификатор описания (см. <seealso cref="IMetadata.Guid"/>).
		/// <para>Не может <seealso cref="Guid.Empty"/>.</para>
		/// </summary>
		Guid Guid { get; }

		/// <summary>
		/// Возвращает полное имя описания (см. <seealso cref="IMetadata.FullName"/>).
		/// <para>Не может быть <see langword="null"/>, <seealso cref="string.Empty"/>.</para>
		/// </summary>
		string FullName { get; }

		/// <summary>
		/// Возвращает идентичность пакета описаний, которому принадлежит описание.
		/// <para>Может быть <see langword="null"/>.</para>
		/// </summary>
		DescriptionPackageIdentity PackageIdentity { get; }

		/// <summary>
		/// Возвращает URI происхождения (источника загрузки) пакета описаний, которому принадлежит описание.
		/// <para>Может быть <see langword="null"/>.</para>
		/// </summary>
		Uri PackageSiteOrigin { get; }

		/// <summary>
		/// Возвращает текстовое user-friendly представление описания, используемое, например, в UI.
		/// <para>Не может быть <see langword="null"/>, <seealso cref="string.Empty"/>.</para>
		/// </summary>
		string DisplayName { get; }

	}

}