using System.Data.Common;

namespace Eon.Data.Storage {

	public interface IStorageDbConnectionStringBuilderFactory<TBuilder>
		:IFactory<IStorageConnectionStringSettings, TBuilder>
		where TBuilder : DbConnectionStringBuilder { }

}