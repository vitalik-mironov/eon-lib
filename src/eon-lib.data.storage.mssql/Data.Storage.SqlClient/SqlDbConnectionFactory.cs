using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace Eon.Data.Storage.SqlClient {

	public class SqlDbConnectionFactory
		:IStorageDbConnectionFactory<SqlConnection> {

		readonly IServiceProvider _serviceProvider;

		public SqlDbConnectionFactory(IServiceProvider serviceProvider) {
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

		public virtual SqlConnection Create(IStorageSettings settings, IContext ctx = default)
			=> CreateAsync(settings: settings, ctx: ctx).WaitResultWithTimeout();

		public virtual async Task<SqlConnection> CreateAsync(IStorageSettings settings, IContext ctx = default) {
			settings.EnsureNotNull(nameof(settings)).EnsureReadOnly().EnsureValid();
			//
			ctx.ThrowIfCancellationRequested();
			var connectionStringBuilderFactory = ServiceProvider.GetRequiredService<IStorageDbConnectionStringBuilderFactory<SqlConnectionStringBuilder>>();
			var connectionStringBuilder = await connectionStringBuilderFactory.CreateAsync(arg: settings.ConnectionString, ctx: ctx).Unwrap().ConfigureAwait(false);
			CreateConnection(builder: connectionStringBuilder, connection: out var connection, ctx: ctx);
			return connection;
		}

		protected virtual void CreateConnection(SqlConnectionStringBuilder builder, out SqlConnection connection, IContext ctx = default) {
			builder.EnsureNotNull(nameof(builder));
			//
			ctx.ThrowIfCancellationRequested();
			connection = new SqlConnection(connectionString: builder.ConnectionString);
		}

		ITaskWrap<SqlConnection> IOptionalFactory<IStorageSettings, SqlConnection>.CreateAsync(IStorageSettings settings, IContext ctx)
			=> CreateAsync(settings: settings, ctx: ctx).Wrap();

	}

}