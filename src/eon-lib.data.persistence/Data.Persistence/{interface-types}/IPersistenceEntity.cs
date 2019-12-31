using System;
using Eon.ComponentModel.Properties.Annotations;

namespace Eon.Data.Persistence {

	/// <summary>
	/// Сущность данных.
	/// </summary>
	[PropertyValueCopyTypeOptions]
	public interface IPersistenceEntity {

		/// <summary>
		/// Возвращает тип ссылочного ключа сущности.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </summary>
		Type ReferenceKeyType { get; }

		/// <summary>
		/// Возвращает массив байтов, представляющий версию данных сущности.
		/// </summary>
		/// <returns>Объект <see cref="T:byte[]"/>.</returns>
		[PropertyValueCopyOptions(IsNotCopyableFrom = true, IsNotCopyableTo = true)]
		byte[ ] Etag { get; set; }

		/// <summary>
		/// Возвращает Int64-представление версии данных сущности.
		/// </summary>
		/// <returns>Значение <see cref="long"/>.</returns>
		[PropertyValueCopyOptions(IsNotCopyableFrom = true, IsNotCopyableTo = true)]
		long? EtagInt64 { get; set; }

		/// <summary>
		/// Возвращает признак, указывающий, является ли сущность новой (т.е. в хранилище данных отсутствует).
		/// </summary>
		/// <returns>Значение <see cref="bool"/>.</returns>
		bool IsNew { get; }

	}

}