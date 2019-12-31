#pragma warning disable CS3001 // Argument type is not CLS-compliant
#pragma warning disable CS3002 // Return type is not CLS-compliant

using System;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Npgsql;

namespace Eon.Data.Storage.PgsqlClient {

	public class PgsqlDbConnectionFactory
		:IStorageDbConnectionFactory<NpgsqlConnection> {

		readonly IServiceProvider _serviceProvider;

		public PgsqlDbConnectionFactory(IServiceProvider serviceProvider) {
			serviceProvider.EnsureNotNull(nameof(serviceProvider));
			//
			_serviceProvider = serviceProvider;
		}

		protected IServiceProvider ServiceProvider
			=> _serviceProvider;

		public virtual bool CanCreate(IStorageSettings settings) {
			settings.EnsureNotNull(nameof(settings)).EnsureReadOnly().EnsureValid();
			//
			return true;
		}

		public virtual NpgsqlConnection Create(IStorageSettings settings, IContext ctx = default)
			=> CreateAsync(settings: settings, ctx: ctx).WaitResultWithTimeout();

		public virtual async Task<NpgsqlConnection> CreateAsync(IStorageSettings settings, IContext ctx = default) {
			settings.EnsureNotNull(nameof(settings)).EnsureReadOnly().EnsureValid();
			//
			ctx.ThrowIfCancellationRequested();
			var connectionStringBuilderFactory = ServiceProvider.GetRequiredService<IStorageDbConnectionStringBuilderFactory<NpgsqlConnectionStringBuilder>>();
			var connectionStringBuilder = await connectionStringBuilderFactory.CreateAsync(arg: settings.ConnectionString, ctx: ctx).Unwrap().ConfigureAwait(false);
			CreateConnection(builder: connectionStringBuilder, connection: out var connection, ctx: ctx);
			return connection;
		}

		protected virtual void CreateConnection(NpgsqlConnectionStringBuilder builder, out NpgsqlConnection connection, IContext ctx = default) {
			builder.EnsureNotNull(nameof(builder));
			//
			ctx.ThrowIfCancellationRequested();
			connection = new NpgsqlConnection(connectionString: builder.ConnectionString);
		}

		ITaskWrap<NpgsqlConnection> IOptionalFactory<IStorageSettings, NpgsqlConnection>.CreateAsync(IStorageSettings settings, IContext ctx)
			=> CreateAsync(settings: settings, ctx: ctx).Wrap();

	}

}

#pragma warning restore CS3002 // Return type is not CLS-compliant
#pragma warning restore CS3001 // Argument type is not CLS-compliant