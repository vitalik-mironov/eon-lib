using System.Collections.Generic;
using System.Xml.Linq;
using Eon.Reflection;

namespace Eon.Runtime.Serialization {

	/// <summary>
	/// Определяет соответствие одного пространства имён контрактов данных одной или более сборкам, в которых объявлены соответствующие типы контрактов данных.
	/// </summary>
	public interface IDataContractNamespaceAssemblyMap
		:IAsReadOnly<IDataContractNamespaceAssemblyMap>, IValidatable {

		/// <summary>
		/// Возвращает/устанавливает пространство имен контрактов данных.
		/// </summary>
		XNamespace ContractNamespace { get; set; }

		/// <summary>
		/// Возвращает/устанавливает набор имен сборок (см. <seealso cref="DotNetAssemblyName"/>), в которых объявлены типы контрактов данных из указанного пространства имён <seealso cref="ContractNamespace"/>.
		/// </summary>
		IEnumerable<DotNetAssemblyName> Assemblies { get; set; }

	}

}