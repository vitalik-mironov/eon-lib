using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Eon.Data.EfCore {

	/// <summary>
	/// Аргументы конструктора экземпляра контекста EF Core <see cref="EfCoreDbContext"/>.
	/// </summary>
	public class EfCoreDbContextCtorArgs {

		public readonly DbContextOptions Options;

		public readonly bool IsReadOnly;

		public readonly ILogger Logger;

		public EfCoreDbContextCtorArgs(DbContextOptions options, bool isReadOnly) {
			options.EnsureNotNull(nameof(options));
			//
			Options = options;
			IsReadOnly = isReadOnly;
			Logger = null;
		}

		public EfCoreDbContextCtorArgs(DbContextOptions options, bool isReadOnly, ILogger logger) {
			options.EnsureNotNull(nameof(options));
			//
			Options = options;
			IsReadOnly = isReadOnly;
			Logger = logger;
		}

	}

}