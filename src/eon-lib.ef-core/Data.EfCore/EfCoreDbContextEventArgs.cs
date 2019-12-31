using System;

namespace Eon.Data.EfCore {

	public class EfCoreDbContextEventArgs
		:EventArgs {

		public EfCoreDbContextEventArgs(EfCoreDbContext ctx) {
			ctx.EnsureNotNull(nameof(ctx));
			//
			Context = ctx;
		}

		public EfCoreDbContext Context { get; }

	}

}