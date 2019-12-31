using Eon.Metadata;

namespace Eon.Description {

	/// <summary>
	/// Описание (конфигурация) активации компонента <see cref="IServantDescription.Component"/>).
	/// </summary>
	public interface IActivationServantDescription
		:IServantDescription, IAbilityOption, IAutoActivationOption {

		/// <summary>
		/// Возвращает/устанавливает опции повторных попыток активации компонента <seealso cref="IServantDescription.Component"/>.
		/// <para>В валидном состоянии (см. <seealso cref="IMetadata.IsValidated"/>):</para>
		/// <para>• Не может быть <see langword="null"/>.</para>
		/// </summary>
		RetrySettings RetryOptions { get; set; }

	}

}