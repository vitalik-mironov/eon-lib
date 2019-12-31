using Eon.Description;
using Eon.Metadata;

namespace Eon.Triggers.Description {

	/// <summary>
	/// Описание (конфигурацию) триггера (см. <seealso cref="ITrigger{TDescription}"/>).
	/// </summary>
	public interface ITriggerDescription
		:IDescription, IAbilityOption {

		/// <summary>
		/// Возвращает/устанавливает признак, указывающий, что триггер должен перейти в сигнальное состояние после его активации (см. <see cref="ITrigger.ActivateControl"/>).
		/// </summary>
		bool SignalOnActivation { get; set; }

		/// <summary>
		/// Возвращает/устанавливает признак, указывающий, что триггер должен перейти в сигнальное состояние после его активации (см. <see cref="SignalOnActivation"/>), но только для его первой активации.
		/// <para>В валидном состоянии (см. <see cref="IMetadata.IsValidated"/>):</para>
		/// <para>• Не может быть <see langword="true"/>, если <see cref="SignalOnActivation"/> == <see langword="false"/>.</para>
		/// </summary>
		bool SignalOnFirstActivationOnly { get; set; }

	}

}