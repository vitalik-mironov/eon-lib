using System;

using Eon;
using Eon.Data.EfCore;
using Eon.Data.Persistence;
using Eon.Data.Persistence.EfCore;
using Eon.Pooling;

using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection {

	public static class PersistenceEfCoreServiceCollectionUtilities {

		/// <summary>
		/// Registers the singleton service of <see cref="IPersistenceDataContextProvider{TContext}"/> for Entity Framework Core.
		/// <para>As an option, registers EF read-only context <typeparamref name="TEfDbContext"/> using <see cref="EntityFrameworkServiceCollectionExtensions.AddDbContext{TContext}(IServiceCollection, Action{IServiceProvider, DbContextOptionsBuilder}, ServiceLifetime, ServiceLifetime)"/>. In this case the same options setup <paramref name="efDbContextOptionsSetup"/> used to configure EF contex options.</para>
		/// </summary>
		/// <param name="svcs">
		/// Service collection.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="efDbContextOptionsSetup">
		/// Entity Framework context options setup.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="poolingSetup">
		/// Data context pooling setup.
		/// </param>
		/// <param name="referenceKeyProviderSetup">
		/// Reference key provider setup.
		/// </param>
		/// <param name="registerEfDbContextAsService">
		/// Indicates the need to register EF Core context (see <see cref="EntityFrameworkServiceCollectionExtensions.AddDbContext{TContext}(IServiceCollection, Action{IServiceProvider, DbContextOptionsBuilder}, ServiceLifetime, ServiceLifetime)"/>).
		/// </param>
		// TODO: Put strings into the resources.
		//
		public static IServiceCollection AddEfCoreDataContextProvider<TEfDbContext>(
			this IServiceCollection svcs,
			Func<IServiceProvider, DbContextOptionsBuilder<TEfDbContext>, DbContextOptions<TEfDbContext>> efDbContextOptionsSetup,
			Func<IServiceProvider, IPoolingSettings> poolingSetup = default,
			Func<IServiceProvider, IReferenceKeyProviderSettings> referenceKeyProviderSetup = default,
			bool registerEfDbContextAsService = default)
			where TEfDbContext : EfCoreDbContext {
			//
			svcs.EnsureNotNull(nameof(svcs));
			efDbContextOptionsSetup.EnsureNotNull(nameof(efDbContextOptionsSetup));
			//
			svcs.AddSingleton<IPersistenceDataContextProvider<IPersistenceDataContext<TEfDbContext>>, PersistenceEfCoreDataContextProvider<TEfDbContext>>(implementationFactory: factory);
			if (registerEfDbContextAsService)
				svcs
					.AddDbContext<TEfDbContext>(
						optionsAction:
							(locSp, locOptionsBuilder) => {
								var locOptionsBuilderStricted = locOptionsBuilder.EnsureNotNull(nameof(locOptionsBuilder)).EnsureOfType<DbContextOptionsBuilder, DbContextOptionsBuilder<TEfDbContext>>().Value;
								var locConfiguredOptions = efDbContextOptionsSetup(arg1: locSp, arg2: locOptionsBuilderStricted);
								if (!ReferenceEquals(locOptionsBuilderStricted.Options, locConfiguredOptions))
									throw new EonException(message: $"The options setup delegate has returned the different options instance (see '{nameof(DbContextOptionsBuilder)}.{nameof(DbContextOptionsBuilder.Options)}') than one expected. The returned instance is not the same than instance that the options builder passed to that delegate has.");
							});
			return svcs;
			//
			PersistenceEfCoreDataContextProvider<TEfDbContext> factory(IServiceProvider locSp) {
				var locEfDbContextOptionsBuilder = new DbContextOptionsBuilder<TEfDbContext>();
				locEfDbContextOptionsBuilder.UseQueryTrackingBehavior(queryTrackingBehavior: QueryTrackingBehavior.NoTracking);
				var locEfDbContextOptions = efDbContextOptionsSetup(arg1: locSp, arg2: locEfDbContextOptionsBuilder);
				locEfDbContextOptions?.Freeze();
				//
				var locPoolingSettings = poolingSetup?.Invoke(arg: locSp)?.AsReadOnly();
				locPoolingSettings?.Validate();
				//
				var locReferenceKeyProviderSettings = referenceKeyProviderSetup?.Invoke(arg: locSp)?.AsReadOnly();
				locReferenceKeyProviderSettings?.Validate();
				//
				return new PersistenceEfCoreDataContextProvider<TEfDbContext>(outerServiceProvider: locSp, pooling: locPoolingSettings, referenceKeyProvider: locReferenceKeyProviderSettings, efDbContextOptions: locEfDbContextOptions);
			}
		}

	}

}