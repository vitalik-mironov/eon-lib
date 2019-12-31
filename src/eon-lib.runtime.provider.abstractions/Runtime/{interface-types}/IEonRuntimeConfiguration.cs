using Eon.ComponentModel.Dependencies;
using Eon.Metadata;
using Eon.Runtime.Serialization;
using Eon.Threading;

namespace Eon.Runtime {

	/// <summary>
	/// Configuration of Oxy runtime.
	/// </summary>
	public interface IOxyRuntimeConfiguration
		:IAsReadOnly<IOxyRuntimeConfiguration>, IValidatable {

		string OxyDotDirectoryName { get; set; }

		MetadataPathName ProviderAppSatelliteDescriptionPath { get; set; }

		MetadataName ProviderAppSatelliteDescriptionPackageName { get; set; }

		int AsyncOperationMillisecondsTimeout { get; set; }

		TimeoutDuration AsyncOperationTimeout { get; set; }

		int LockMillisecondsTimeout { get; set; }

		int MaxLockMillisecondsTimeout { get; set; }

		/// <summary>
		///	Gets/sets the collection of data contract namespace assembly map (<seealso cref="IDataContractNamespaceAssemblyMap"/>).
		/// <para>Can't be <see langword="null"/>.</para>
		/// </summary>
		IDataContractNamespaceAssemblyMapCollection DataContractNamespaceAssemblyMap { get; set; }

		/// <summary>
		/// Gets/sets the dependency resolution settings (see <seealso cref="IDependencyResolutionContext"/>).
		/// <para>Can't be <see langword="null"/>.</para>
		/// </summary>
		IDependencyResolutionSettings DependencyResolutionSettings { get; set; }

	}

}