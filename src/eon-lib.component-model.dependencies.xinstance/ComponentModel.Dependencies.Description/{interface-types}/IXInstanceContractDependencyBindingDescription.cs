using Eon.Description;
using Eon.Description.Annotations;

namespace Eon.ComponentModel.Dependencies.Description {

	/// <summary>
	/// Представляет описание (конфигурацию) байндинга поставляемой функциональной зависимости (см. <seealso cref="IDependencyBinding"/>). При этом цель байндинга определяется XInstance-контрактами, определенными для заданного описания (см. <seealso cref="DependencyTargetDescription"/>).
	/// <para>См. также <seealso cref="XInstanceContractAttribute"/></para>
	/// </summary>
	public interface IXInstanceContractDependencyBindingDescription
		:IDependencyBindingDescriptionBase {

		/// <summary>
		/// Возвращает объект описания (конфигурацию), который определяет цель банного байндинга.
		/// <para>Указанное описание должно иметь по меньшей мере один XInstance-контракт (см. также <seealso cref="XInstanceContractAttribute"/>).</para>
		/// </summary>
		IDescription DependencyTargetDescription { get; }

		bool IsDependencyTargetSharingDisabled { get; }

	}

}