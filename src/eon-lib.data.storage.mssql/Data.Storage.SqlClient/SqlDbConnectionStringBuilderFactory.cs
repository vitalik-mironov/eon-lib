using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Security.Cryptography;
using Eon.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace Eon.Data.Storage.SqlClient {

	public class SqlDbConnectionStringBuilderFactory
		:IStorageDbConnectionStringBuilderFactory<SqlConnectionStringBuilder> {

		/// <summary>
		/// Max. length of source connection string (see <see cref="CreateBuilder(string, out SqlConnectionStringBuilder, IContext)"/>).
		/// <para>Value: '1024'.</para>
		/// </summary>
		public static readonly int MaxLengthOfSourceConnectionString = 1024;

		readonly IServiceProvider _serviceProvider;

		public SqlDbConnectionStringBuilderFactory(IServiceProvider serviceProvider) {
			serviceProvider.EnsureNotNull(nameof(serviceProvider));
			//
			_serviceProvider = serviceProvider;
		}

		protected IServiceProvider ServiceProvider
			=> _serviceProvider;

		public virtual bool CanCreate(IStorageConnectionStringSettings settings) {
			settings.EnsureNotNull(nameof(settings)).EnsureReadOnly().EnsureValid();
			//
			return true;
		}

		public virtual SqlConnectionStringBuilder Create(IStorageConnectionStringSettings settings, IContext ctx = default)
			=> CreateAsync(settings: settings, ctx: ctx).WaitResultWithTimeout();

		public virtual async Task<SqlConnectionStringBuilder> CreateAsync(IStorageConnectionStringSettings settings, IContext ctx = default) {
			settings.EnsureNotNull(nameof(settings)).EnsureReadOnly().EnsureValid();
			//
			SqlConnectionStringBuilder builder;
			if (settings.SkipSecretTextSubstitution)
				CreateBuilder(sourceConnectionString: await settings.GetConnectionStringRawAsync(ctx: ctx).ConfigureAwait(false), builder: out builder, ctx: ctx);
			else {
				ctx.ThrowIfCancellationRequested();
				var secretTextHandler = ServiceProvider.GetRequiredService<ISecretTextSubstitutionHandler>();
				var connectionString = await secretTextHandler.SubstituteAsync(template: await settings.GetConnectionStringRawAsync(ctx: ctx).ConfigureAwait(false), ctx: ctx).ConfigureAwait(false);
				CreateBuilder(sourceConnectionString: connectionString, builder: out builder, ctx: ctx);
			}
			return builder;
		}

		protected virtual void CreateBuilder(string sourceConnectionString, out SqlConnectionStringBuilder builder, IContext ctx = default) {
			sourceConnectionString.EnsureNotNull(nameof(sourceConnectionString)).EnsureHasMaxLength(maxLength: MaxLengthOfSourceConnectionString).EnsureNotEmptyOrWhiteSpace();
			//
			ctx.ThrowIfCancellationRequested();
			builder = new SqlConnectionStringBuilder(connectionString: sourceConnectionString);
		}

		ITaskWrap<SqlConnectionStringBuilder> IOptionalFactory<IStorageConnectionStringSettings, SqlConnectionStringBuilder>.CreateAsync(IStorageConnectionStringSettings settings, IContext ctx)
			=> CreateAsync(settings: settings, ctx: ctx).Wrap();

	}

}