using System.Data.Common;

namespace Eon.Data.Storage {

	public interface IStorageDbConnectionFactory<out TConnection>
		:IFactory<IStorageSettings, TConnection>
		where TConnection : DbConnection { }

}