
using Eon.Description;

namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Представляет настройки разрешения функциональных зависимостей.
	/// </summary>
	public interface IDependencyResolutionSettings
		:ISettings, IAsReadOnly<IDependencyResolutionSettings> {

		/// <summary>
		/// Возвращает/устанавливает значение, указывающее сколько раз может быть использован один и тот же обработчик разрешения функциональной зависимости в ходе разрешения зависимости.
		/// <para>Значение не может быть меньше 1.</para>
		/// </summary>
		int UsageCountLimitPerHandler { get; set; }

		/// <summary>
		/// Возвращает/устанавливает признак, указывающий, нужно ли вести расширенное логирование каждой операции разрешения функциональной зависимости.
		/// </summary>
		bool IsAdvancedLoggingEnabled { get; set; }

	}

}