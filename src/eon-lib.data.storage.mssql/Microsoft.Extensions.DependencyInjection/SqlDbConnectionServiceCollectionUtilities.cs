using System;
using System.Data.SqlClient;

using Eon;
using Eon.Context;
using Eon.Data.Storage;
using Eon.Data.Storage.SqlClient;

namespace Microsoft.Extensions.DependencyInjection {

	public static class SqlDbConnectionServiceCollectionUtilities {

		/// <summary>
		/// Registers singleton service of <see cref="SqlDbConnectionFactory"/>.
		/// <para>Registered factory creates <see cref="SqlConnection"/> using a settings (see <see cref="IStorageSettings"/>) passed to the factory method (see <see cref="IOptionalFactory{TArg, T}.Create(TArg, IContext)"/>).</para>
		/// </summary>
		/// <param name="svcs">
		/// Service collection.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		public static IServiceCollection AddSqlDbConnectionFactory(this IServiceCollection svcs) {
			svcs.EnsureNotNull(nameof(svcs));
			//
			svcs.AddSingleton<IStorageDbConnectionFactory<SqlConnection>, SqlDbConnectionFactory>(implementationFactory: factory);
			return svcs;
			//
			SqlDbConnectionFactory factory(IServiceProvider locOuterSp)
				=> new SqlDbConnectionFactory(serviceProvider: locOuterSp);
		}

		/// <summary>
		/// Registers singleton service of <see cref="SqlDbConnectionStringBuilderFactory"/>.
		/// <para>Registered factory creates <see cref="SqlConnectionStringBuilder"/> using a settings (see <see cref="IStorageConnectionStringSettings"/>) passed to factory method (see <see cref="IOptionalFactory{TArg, T}.Create(TArg, IContext)"/>).</para>
		/// </summary>
		/// <param name="svcs">
		/// Service collection.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		public static IServiceCollection AddSqlDbConnectionStringBuilderFactory(this IServiceCollection svcs) {
			svcs.EnsureNotNull(nameof(svcs));
			//
			svcs.AddSingleton<IStorageDbConnectionStringBuilderFactory<SqlConnectionStringBuilder>, SqlDbConnectionStringBuilderFactory>(implementationFactory: factory);
			return svcs;
			//
			SqlDbConnectionStringBuilderFactory factory(IServiceProvider locOuterSp)
				=> new SqlDbConnectionStringBuilderFactory(serviceProvider: locOuterSp);
		}

	}

}