namespace Eon.Data.EfCore.Metadata.Edm {

	public delegate void EfCoreEntityTypeBuilder<TEntity>(EfCoreEntityTypeBuilderArgs<TEntity> args)
		where TEntity : class;

}