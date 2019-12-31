using Eon.ComponentModel.Dependencies.Description;
using Eon.Description;
using Eon.Metadata;
using Eon.Runtime.Serialization.Description;

namespace Eon.Runtime.Description {

	/// <summary>
	/// Description (configuration) of the Oxy runtime app.
	/// </summary>
	public interface IRuntimeXAppDescription
		:IXAppDescription, IOxyRuntimeConfiguration {

		/// <summary>
		/// Возвращает описание соответствий пространств имён контрактов данных сборкам, в которых объявлены соответствующие типы контрактов данных (см. <seealso cref="IDataContractNamespaceAssemblyMapDescription"/>.
		/// <para>Не может быть null (для валидного состояния (см. <seealso cref="IMetadata.IsValidated"/>)).</para>
		/// </summary>
		new IDataContractNamespaceAssemblyMapDescription DataContractNamespaceAssemblyMap { get; set; }

		/// <summary>
		/// Возвращает описание настроек разрешения функциональных зависимостей, используемых по умолчанию.
		/// <para>Не может быть null (для валидного состояния (см. <seealso cref="IMetadata.IsValidated"/>)).</para>
		/// </summary>
		new IDependencyResolutionSettingsDescription DependencyResolutionSettings { get; set; }

	}

}