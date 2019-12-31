using Eon.Metadata;

namespace Eon.Description {

	/// <summary>
	/// Описание (конфигурация) обслуживающего компонента <see cref="Component"/>.
	/// </summary>
	public interface IServantDescription
		:IDescription {

		/// <summary>
		/// Возвращает/устанавливает описание компонента.
		/// <para>В валидном состоянии (см. <see cref="IMetadata.IsValidated"/>):</para>
		/// <para>• Не может быть <see langword="null"/>.</para>
		/// </summary>
		IDescription Component { get; set; }

	}

}
