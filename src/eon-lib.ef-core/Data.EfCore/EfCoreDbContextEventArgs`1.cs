namespace Eon.Data.EfCore {

	public class EfCoreDbContextEventArgs<TContext>
		:EfCoreDbContextEventArgs
		where TContext : EfCoreDbContext {

		public EfCoreDbContextEventArgs(TContext ctx)
			: base(ctx: ctx) { }

		public new TContext Context 
			=> (TContext)base.Context;

	}

}