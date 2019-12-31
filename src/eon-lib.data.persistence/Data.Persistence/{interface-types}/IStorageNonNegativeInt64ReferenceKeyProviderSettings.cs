
namespace Eon.Data.Persistence {

	public interface IStorageNonNegativeInt64ReferenceKeyProviderSettings
		:IStorageReferenceKeyProviderSettings, IAsReadOnly<IStorageNonNegativeInt64ReferenceKeyProviderSettings> {

		long? ScopeLowerKey { get; set; }

		long? ScopeUpperKey { get; set; }

		long? ScopeKeysCount { get; }

	}

}