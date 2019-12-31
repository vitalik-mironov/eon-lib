#pragma warning disable CS3001 // Argument type is not CLS-compliant
#pragma warning disable CS3002 // Return type is not CLS-compliant

using System;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Security.Cryptography;
using Eon.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Npgsql;

namespace Eon.Data.Storage.PgsqlClient {

	public class PgsqlDbConnectionStringBuilderFactory
		:IStorageDbConnectionStringBuilderFactory<NpgsqlConnectionStringBuilder> {

		/// <summary>
		/// Max. length of source connection string (see <see cref="CreateBuilder(string, out NpgsqlConnectionStringBuilder, IContext)"/>).
		/// <para>Value: '1024'.</para>
		/// </summary>
		public static readonly int MaxLengthOfSourceConnectionString = 1024;

		readonly IServiceProvider _serviceProvider;

		public PgsqlDbConnectionStringBuilderFactory(IServiceProvider serviceProvider) {
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

		public virtual NpgsqlConnectionStringBuilder Create(IStorageConnectionStringSettings settings, IContext ctx = default)
			=> CreateAsync(settings: settings, ctx: ctx).WaitResultWithTimeout();

		public virtual async Task<NpgsqlConnectionStringBuilder> CreateAsync(IStorageConnectionStringSettings settings, IContext ctx = default) {
			settings.EnsureNotNull(nameof(settings)).EnsureReadOnly().EnsureValid();
			//
			NpgsqlConnectionStringBuilder builder;
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

		protected virtual void CreateBuilder(string sourceConnectionString, out NpgsqlConnectionStringBuilder builder, IContext ctx = default) {
			sourceConnectionString.EnsureNotNull(nameof(sourceConnectionString)).EnsureHasMaxLength(maxLength: MaxLengthOfSourceConnectionString).EnsureNotEmptyOrWhiteSpace();
			//
			ctx.ThrowIfCancellationRequested();
			builder = new NpgsqlConnectionStringBuilder(connectionString: sourceConnectionString);
		}

		ITaskWrap<NpgsqlConnectionStringBuilder> IOptionalFactory<IStorageConnectionStringSettings, NpgsqlConnectionStringBuilder>.CreateAsync(IStorageConnectionStringSettings settings, IContext ctx)
			=> CreateAsync(settings: settings, ctx: ctx).Wrap();

	}

}

#pragma warning restore CS3002 // Return type is not CLS-compliant
#pragma warning restore CS3001 // Argument type is not CLS-compliant