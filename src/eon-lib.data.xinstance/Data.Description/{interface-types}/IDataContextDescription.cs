using Eon.ComponentModel.Dependencies.Description;

namespace Eon.Data.Description {

	/// <summary>
	/// Описание (конфигурация) контекста данных (см. <seealso cref="IDataContext2"/>).
	/// </summary>
	public interface IDataContextDescription
		:IDependencySupportDescription {

		/// <summary>
		/// Возвращает/устанавливает признак, определяющий предпочтение относительно того, отслеживать ли изменения данных сущностей, загруженных контекстом из источника данных.
		/// </summary>
		bool PreferNoTracking { get; set; }

	}

}