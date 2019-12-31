namespace Eon.Data.Persistence {

	/// <summary>
	/// Поставщик ссылочных ключей сущностей данных <seealso cref="IPersistenceEntity{TReferenceKey}"/>.
	/// </summary>
	/// <typeparam name="TKey">Тип значения ключа.</typeparam>
	/// <typeparam name="TSettings"></typeparam>
	public interface IReferenceKeyProvider<TKey, out TSettings>
		:IReferenceKeyProvider<TKey>
		where TKey : struct
		where TSettings : class, IReferenceKeyProviderSettings {

		TSettings Settings { get; }

	}

}