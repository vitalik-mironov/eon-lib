using Eon.ComponentModel.Dependencies.Description;
using Eon.Threading;

namespace Eon.Data.Description {

	/// <summary>
	/// Описание (конфигурация) поставщика контекста данных.
	/// </summary>
	public interface IDataContextProviderDescription
		:IDependencySupportDescription {

		/// <summary>
		/// Возвращает описание (конфигурацию) контекста данных, на основе которого формируется контекст данных, поставляемый компонентом, созданным на основе данного описания.
		/// </summary>
		IDataContextDescription DataContext { get; set; }

		bool IsDataContextPoolEnabled { get; set; }

		int? DataContextPoolSize { get; set; }

		TimeoutDuration DataContextPoolPreferredSlidingTtl { get; set; }

	}

}