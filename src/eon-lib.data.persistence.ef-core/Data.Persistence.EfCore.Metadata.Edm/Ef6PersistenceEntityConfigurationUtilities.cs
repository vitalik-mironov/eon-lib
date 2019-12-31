using Eon.Data.EfCore.Metadata.Edm;
using Eon.Data.Persistence.Ef6.Metadata.Edm;

namespace Eon.Data.Persistence.EfCore.Metadata.Edm {

	public static class Ef6PersistenceEntityConfigurationUtilities {

		public static EfCorePersistenceInt64ReferenceKeyEntityTypeBuilder<TEntity> CreateBuilder<TEntity>(EfCoreEntityTypeBuilder<TEntity> builder)
			where TEntity : PersistenceEntityBase<long>
			=> new EfCorePersistenceInt64ReferenceKeyEntityTypeBuilder<TEntity>(builder: builder);

	}

}