namespace Eon.Data.Storage {

	public interface IPlainTextStorageConnectionStringSettings
		:IStorageConnectionStringSettings, IAsReadOnly<IPlainTextStorageConnectionStringSettings> {

		string ConnectionStringText { get; set; }

	}

}