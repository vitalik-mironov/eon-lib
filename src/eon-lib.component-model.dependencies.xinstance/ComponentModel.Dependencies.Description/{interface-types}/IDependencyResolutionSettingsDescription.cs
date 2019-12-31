using Eon.Description;
using Eon.Metadata;

namespace Eon.ComponentModel.Dependencies.Description {

	/// <summary>
	/// Представляет описание (конфигурацию) дефолтных настроек разрешения функциональных зависимостей.
	/// </summary>
	public interface IDependencyResolutionSettingsDescription
		:IDescription {

		/// <summary>
		/// Возвращает настройки разрешения функциональных зависимостей, определяемый данным описанием.
		/// <para>Не может быть null (для валидного состояния (см. <seealso cref="IMetadata.IsValidated"/>)).</para>
		/// </summary>
		IDependencyResolutionSettings Settings { get; }

	}

}