using Eon.Description;
using Eon.Threading;

namespace Eon.Data.Storage {

	public interface IStorageSettings
		:ISettings, IAsReadOnly<IStorageSettings> {

		IStorageConnectionStringSettings ConnectionString { get; set; }

		TimeoutDuration CommandExecutionTimeout { get; set; }

		TimeoutDuration LockTimeout { get; set; }

	}

}