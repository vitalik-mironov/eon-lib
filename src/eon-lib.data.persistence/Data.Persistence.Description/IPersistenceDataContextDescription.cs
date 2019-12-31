using System.Collections.Generic;

using Eon.Data.Description;
using Eon.Data.TypeSystem;

namespace Eon.Data.Persistence.Description {

	/// <summary>
	/// Описание (конфигурация) контекста данных постоянного хранения (хранилища данных).
	/// </summary>
	public interface IPersistenceDataContextDescription
		:IDataContextDescription {

		/// <summary>
		/// Возвращает набор типов данных (их построителей), входящих в доменную модель (модель данных) контекста данных, определяемого данным описанием.
		/// </summary>
		/// <returns>Объект <seealso cref="IEnumerable{T}"/>.</returns>
		IEnumerable<IDataTypeBuilder> GetEntityTypes();

	}

}