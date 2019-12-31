using Eon.Metadata;
using Eon.Triggers.Description;

namespace Eon.Description {

	/// <summary>
	/// Описание (конфигурация) компонента, выполняющего при определенных условиях сброс компонента <see cref="IServantDescription.Component"/>.
	/// </summary>
	public interface IResetServantDescription
		:IServantDescription, IAbilityOption, IAutoActivationOption {

		/// <summary>
		/// Возвращает/устанавливает описание триггера, инициирующего сброс.
		/// <para>В валидном состоянии (см. <see cref="IMetadata.IsValidated"/>):</para>
		/// <para>• Не может быть <see langword="null"/>.</para>
		/// </summary>
		IAggregateTriggerDescription ResetTrigger { get; set; }

		ResetServantResetFailureResponseCode ResetFailureResponseCode { get; set; }

		string ResetFailureProgram { get; set; }

		string ResetFailureProgramArgs { get; set; }

	}

}