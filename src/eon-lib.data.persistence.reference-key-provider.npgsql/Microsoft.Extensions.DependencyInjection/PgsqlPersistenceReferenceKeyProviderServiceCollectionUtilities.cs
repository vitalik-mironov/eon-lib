#pragma warning disable CS3001 // Argument type is not CLS-compliant
#pragma warning disable CS3002 // Return type is not CLS-compliant

using System;
using System.Threading.Tasks;

using Eon;
using Eon.Context;
using Eon.Data.Persistence;
using Eon.Data.Persistence.PgsqlClient;
using Eon.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection {

	public static class PgsqlPersistenceReferenceKeyProviderServiceCollectionUtilities {

		#region Nested types

		sealed class P_NonNegativeInt64ReferenceKeyProviderFactory
			:IReferenceKeyProviderFactory {

			readonly IServiceProvider _serviceProvider;

			internal P_NonNegativeInt64ReferenceKeyProviderFactory(IServiceProvider serviceProvider) {
				serviceProvider.EnsureNotNull(nameof(serviceProvider));
				//
				_serviceProvider = serviceProvider;
			}

			public bool CanCreate(IReferenceKeyProviderSettings settings) {
				settings.EnsureNotNull(nameof(settings)).EnsureReadOnly().EnsureValid();
				//
				return settings is IStorageNonNegativeInt64ReferenceKeyProviderSettings;
			}

			public IReferenceKeyProvider Create(IReferenceKeyProviderSettings settings, IContext ctx = default) {
				var locSettings = settings.Arg(nameof(settings)).EnsureOfType<IReferenceKeyProviderSettings, IStorageNonNegativeInt64ReferenceKeyProviderSettings>().Value;
				//
				return new PgsqlNonNegativeInt64ReferenceKeyProvider(serviceProvider: _serviceProvider, settings: locSettings);
			}

			public async Task<IReferenceKeyProvider> CreateAsync(IReferenceKeyProviderSettings settings, IContext ctx = default) {
				await Task.CompletedTask;
				return Create(settings: settings, ctx: ctx);
			}

			ITaskWrap<IReferenceKeyProvider> IOptionalFactory<IReferenceKeyProviderSettings, IReferenceKeyProvider>.CreateAsync(IReferenceKeyProviderSettings settings, IContext ctx)
				=> CreateAsync(settings: settings, ctx: ctx).Wrap();

		}

		#endregion

		/// <summary>
		/// Registers singleton service of inner implementation of <see cref="IReferenceKeyProviderFactory"/>.
		/// <para>Registered implementation creates <see cref="PgsqlNonNegativeInt64ReferenceKeyProvider"/> using a settings (see <see cref="IStorageNonNegativeInt64ReferenceKeyProviderSettings"/>) passed to factory method (see <see cref="IOptionalFactory{TArg, T}.Create(TArg, IContext)"/>).</para>
		/// </summary>
		/// <param name="svcs">
		/// Service collection.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		public static IServiceCollection AddPgsqlNonNegativeInt64ReferenceKeyProvider(this IServiceCollection svcs) {
			svcs.EnsureNotNull(nameof(svcs));
			//
			svcs.AddSingleton<IReferenceKeyProviderFactory, P_NonNegativeInt64ReferenceKeyProviderFactory>(implementationFactory: factory);
			return svcs;
			//
			P_NonNegativeInt64ReferenceKeyProviderFactory factory(IServiceProvider locOuterSp)
				=> new P_NonNegativeInt64ReferenceKeyProviderFactory(serviceProvider: locOuterSp);
		}

	}

}

#pragma warning restore CS3002 // Return type is not CLS-compliant
#pragma warning restore CS3001 // Argument type is not CLS-compliant