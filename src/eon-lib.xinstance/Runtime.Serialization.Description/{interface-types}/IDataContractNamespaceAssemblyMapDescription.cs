using Eon.Description;

namespace Eon.Runtime.Serialization.Description {

	/// <summary>
	/// Description (configuration) of the data contract namespace assembly mapping(s).
	/// </summary>
	public interface IDataContractNamespaceAssemblyMapDescription
		:IDescription {

		/// <summary>
		/// Gets/sets the collection of maps of data contract names to assembly names (see <seealso cref="IDataContractNamespaceAssemblyMap"/>).
		/// </summary>
		IDataContractNamespaceAssemblyMapCollection Maps { get; set; }

	}

}