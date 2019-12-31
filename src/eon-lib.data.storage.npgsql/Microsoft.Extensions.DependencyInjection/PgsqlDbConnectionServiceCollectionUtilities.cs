#pragma warning disable CS3002 // Return type is not CLS-compliant
#pragma warning disable CS3001 // Argument type is not CLS-compliant

using System;

using Eon;
using Eon.Context;
using Eon.Data.Storage;
using Eon.Data.Storage.PgsqlClient;

using Npgsql;

namespace Microsoft.Extensions.DependencyInjection {

	public static class PgsqlDbConnectionServiceCollectionUtilities {

		/// <summary>
		/// Registers singleton service of <see cref="PgsqlDbConnectionFactory"/>.
		/// <para>Registered factory creates <see cref="NpgsqlConnection"/> using a settings (see <see cref="IStorageSettings"/>) passed to factory method (see <see cref="IOptionalFactory{TArg, T}.Create(TArg, IContext)"/>).</para>
		/// </summary>
		/// <param name="svcs">
		/// Service collection.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		public static IServiceCollection AddPgsqlDbConnectionFactory(this IServiceCollection svcs) {
			svcs.EnsureNotNull(nameof(svcs));
			//
			svcs.AddSingleton<IStorageDbConnectionFactory<NpgsqlConnection>, PgsqlDbConnectionFactory>(implementationFactory: factory);
			return svcs;
			//
			PgsqlDbConnectionFactory factory(IServiceProvider locOuterSp)
				=> new PgsqlDbConnectionFactory(serviceProvider: locOuterSp);
		}

		/// <summary>
		/// Registers singleton service of <see cref="PgsqlDbConnectionStringBuilderFactory"/>.
		/// <para>Registered factory creates <see cref="NpgsqlConnectionStringBuilder"/> using a settings (see <see cref="IStorageConnectionStringSettings"/>) passed to factory method (see <see cref="IOptionalFactory{TArg, T}.Create(TArg, IContext)"/>).</para>
		/// </summary>
		/// <param name="svcs">
		/// Service collection.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		public static IServiceCollection AddPgsqlDbConnectionStringBuilderFactory(this IServiceCollection svcs) {
			svcs.EnsureNotNull(nameof(svcs));
			//
			svcs.AddSingleton<IStorageDbConnectionStringBuilderFactory<NpgsqlConnectionStringBuilder>, PgsqlDbConnectionStringBuilderFactory>(implementationFactory: factory);
			return svcs;
			//
			PgsqlDbConnectionStringBuilderFactory factory(IServiceProvider locOuterSp)
				=> new PgsqlDbConnectionStringBuilderFactory(serviceProvider: locOuterSp);
		}

	}

}

#pragma warning restore CS3001 // Argument type is not CLS-compliant
#pragma warning restore CS3002 // Return type is not CLS-compliant