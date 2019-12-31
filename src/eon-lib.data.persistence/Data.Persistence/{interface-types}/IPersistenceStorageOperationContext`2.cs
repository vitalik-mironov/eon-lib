namespace Eon.Data.Persistence {

	/// <summary>
	/// Контекст модификации данных.
	/// </summary>
	public interface IPersistenceStorageOperationContext<TEntity, TReferenceKey>
		:IPersistenceStorageOperationContext<TEntity>
		where TEntity : class, IPersistenceEntity<TReferenceKey>
		where TReferenceKey : struct { }

}