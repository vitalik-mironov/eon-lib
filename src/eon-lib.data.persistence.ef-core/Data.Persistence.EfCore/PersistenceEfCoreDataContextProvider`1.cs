#pragma warning disable CS3003 // Type is not CLS-compliant
#pragma warning disable CS3001 // Argument type is not CLS-compliant

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Eon.ComponentModel.Dependencies;
using Eon.Context;
using Eon.Data.EfCore;
using Eon.Pooling;
using Eon.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using deputils = Eon.ComponentModel.Dependencies.DependencyUtilities;

namespace Eon.Data.Persistence.EfCore {

	public class PersistenceEfCoreDataContextProvider<TEfDbContext>
		:DependencySupport, IPersistenceDataContextProvider<IPersistenceDataContext<TEfDbContext>>
		where TEfDbContext : EfCoreDbContext {

		IServiceProvider _serviceProvider;

		DbContextOptions<TEfDbContext> _efDbContextOptions;

		IPoolingSettings _dataContextPoolingSettings;

		readonly bool _isDataContextPoolingEnabled;

		IPool<IPersistenceDataContext<TEfDbContext>> _dataContextPool;

		IReferenceKeyProviderSettings _referenceKeyProviderSettings;

		ValueHolderClass<IReferenceKeyProvider> _referenceKeyProvider;

		// TODO: Put strings into the resources.
		//
		public PersistenceEfCoreDataContextProvider(IServiceProvider outerServiceProvider, DbContextOptions<TEfDbContext> efDbContextOptions, IPoolingSettings pooling = default, IReferenceKeyProviderSettings referenceKeyProvider = default)
			: base(outerServiceProvider: outerServiceProvider.EnsureNotNull(nameof(outerServiceProvider)).Value) {
			efDbContextOptions.EnsureNotNull(nameof(efDbContextOptions));
			if (!efDbContextOptions.IsFrozen)
				throw
					new ArgumentException(
						message: $"EF context options must be frozen (see {nameof(DbContextOptions)}.{nameof(DbContextOptions.IsFrozen)}) before using by this component.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}",
						paramName: nameof(efDbContextOptions));
			pooling.Arg(nameof(pooling)).EnsureReadOnly().EnsureValid();
			referenceKeyProvider.Arg(nameof(referenceKeyProvider)).EnsureReadOnly().EnsureValid();
			//
			_serviceProvider = outerServiceProvider;
			_efDbContextOptions = efDbContextOptions;
			_dataContextPoolingSettings = pooling;
			_isDataContextPoolingEnabled = !(pooling?.IsDisabled ?? true);
			if (_isDataContextPoolingEnabled)
				_dataContextPool =
					new Pool<IPersistenceDataContext<TEfDbContext>>(
						itemFactory: CreateContextAsync,
						ownsItem: true,
						itemPreferredSlidingTtl: pooling.PreferredSlidingTtl,
						maxSize: pooling.PoolSize.Value,
						displayName: pooling.PoolDisplayName,
						logger: ((ILogger)outerServiceProvider.GetService(serviceType: typeof(ILogger<TEfDbContext>))).ArgPlaceholder());
			else
				_dataContextPool = null;
			//
			_referenceKeyProviderSettings = referenceKeyProvider;
		}

		bool IDependencyHandler2.CanShareDependency {
			get {
				EnsureNotDisposeState();
				return true;
			}
		}

		bool IDependencyHandler2.CanRedirect {
			get {
				EnsureNotDisposeState();
				return true;
			}
		}

		protected IServiceProvider ServiceProvider
			=> ReadDA(ref _serviceProvider);

		public DbContextOptions<TEfDbContext> EfDbContextOptions
			=> ReadDA(location: ref _efDbContextOptions);

		public IPoolingSettings DataContextPoolingSettings
			=> ReadDA(location: ref _dataContextPoolingSettings);

		public IReferenceKeyProviderSettings ReferenceKeyProviderSettings
			=> ReadDA(location: ref _referenceKeyProviderSettings);

		// TODO: Put strings into the resources.
		//
		public virtual async Task<IReferenceKeyProvider<TKey>> RequireReferenceKeyProviderAsync<TKey>(PersistenceEntityReferenceKeyTypeDescriptor keyTypeDescriptor, IContext ctx = default)
			where TKey : struct {
			await Task.CompletedTask;
			var provider = InitDA(location: ref _referenceKeyProvider, factory: create, cleanup: cleanup);
			if (provider is IReferenceKeyProvider<TKey> requiredProvider)
				return requiredProvider;
			else
				throw new EonException(message: $"Type of existing provider is not compatible with the required type.{Environment.NewLine}Type of existing provider:{provider.GetType().FmtStr().GNLI2()}{Environment.NewLine}Required type of provider:{typeof(IReferenceKeyProvider<TKey>).FmtStr().GNLI2()}");
			//
			IReferenceKeyProvider create() {
				var locSettings = ReferenceKeyProviderSettings;
				if (locSettings is null)
					throw new EonException(message: $"There is no the reference key provider settings.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
				else if (locSettings.IsDisabled)
					throw new EonException(message: $"Usage of the reference key provider not enabled by the settings.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
				else {
					var locFactory = ServiceProvider.GetRequiredService<IReferenceKeyProviderFactory>();
					if (locFactory.CanCreate(arg: locSettings))
						using (var locCtx = new GenericContext(outerCtx: ctx)) {
							locCtx.Set(prop: DependencyContextProps.DependenciesProp, value: this.ToValueHolder(ownsValue: false));
							return locFactory.CreateAsync(arg: ReferenceKeyProviderSettings, ctx: locCtx).Unwrap().WaitResultWithTimeout();
						}
					else
						throw new EonException(message: $"The factory cannot create an instance of the reference key provider using the specified settings.{Environment.NewLine}\tFactory:{locFactory.FmtStr().GNLI2()}{Environment.NewLine}\tSettings:{locSettings.FmtStr().GNLI2()}{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
				}
			}
			void cleanup(IReferenceKeyProvider locProvider)
				=> locProvider?.Dispose();
		}

		public async Task<IPersistenceDataContext<TEfDbContext>> CreateContextAsync(IContext ctx = default) {
			ctx.ThrowIfCancellationRequested();
			//
			PersistenceEfCoreDataContext<TEfDbContext> dataContext = default;
			try {
				dataContext = await DoCreateContextAsync(ctx: ctx).ConfigureAwait(false);
				await OnAdjustContextAsync(dataContext: dataContext, ctx: ctx).ConfigureAwait(false);
				//
				ctx.ThrowIfCancellationRequested();
				//
				return dataContext;
			}
			catch (Exception exception) {
				dataContext?.Dispose(exception);
				throw;
			}
		}

		ITaskWrap<IPersistenceDataContext<TEfDbContext>> IDataContextProvider<IPersistenceDataContext<TEfDbContext>>.CreateContextAsync(IContext ctx)
			=> CreateContextAsync(ctx: ctx).Wrap();

		protected virtual async Task<PersistenceEfCoreDataContext<TEfDbContext>> DoCreateContextAsync(IContext ctx = default) {
			await Task.CompletedTask;
			return new PersistenceEfCoreDataContext<TEfDbContext>(provider: this, efDbContextOptions: EfDbContextOptions, logger: (ILogger)ServiceProvider.GetService(serviceType: typeof(ILogger<PersistenceEfCoreDataContext<TEfDbContext>>)));
		}

		protected virtual async Task OnAdjustContextAsync(PersistenceEfCoreDataContext<TEfDbContext> dataContext, IContext ctx = default) {
			dataContext.EnsureNotNull(nameof(dataContext));
			//
			await Task.CompletedTask;
		}

		public async Task<IUsing<IPersistenceDataContext<TEfDbContext>>> LeaseContextAsync(IContext ctx = default) {
			ctx.ThrowIfCancellationRequested();
			//
			if (_isDataContextPoolingEnabled)
				return await ReadDA(ref _dataContextPool, considerDisposeRequest: true).TakeAsync(ctx: ctx).Unwrap().ConfigureAwait(false);
			else
				return
					new UsingClass<IPersistenceDataContext<TEfDbContext>>(
						value: await CreateContextAsync(ctx: ctx).ConfigureAwait(false),
						dispose:
							(locValue, locExplicitDispose) => {
								if (locExplicitDispose)
									locValue?.Dispose();
							});
		}

		ITaskWrap<IUsing<IPersistenceDataContext<TEfDbContext>>> IDataContextProvider<IPersistenceDataContext<TEfDbContext>>.LeaseContextAsync(IContext ctx) {
			try {
				return LeaseContextAsync(ctx: ctx).Wrap();
			}
			catch (Exception exception) {
				return TaskUtilities.Wrap<IUsing<IPersistenceDataContext<TEfDbContext>>>(exception: exception);
			}
		}

		public override IEnumerable<IVh<IDependencyHandler2>> LocalDependencies() {
			yield return deputils.CreateHandlerForNew(factory: () => CreateContextAsync(ctx: null).WaitResultWithTimeout());
			//
			foreach (var baseHandler in base.LocalDependencies())
				yield return baseHandler;
		}

		DependencyResult IDependencyHandler.ExecuteResolution(IDependencyResolutionContext resolutionCtx)
			=> DependencyResult.None;

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_dataContextPool?.Dispose();
				_referenceKeyProvider?.Dispose();
			}
			_dataContextPool = null;
			_dataContextPoolingSettings = null;
			_efDbContextOptions = null;
			_referenceKeyProviderSettings = null;
			_referenceKeyProvider = null;
			_serviceProvider = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}

#pragma warning restore CS3001 // Argument type is not CLS-compliant
#pragma warning restore CS3003 // Type is not CLS-compliant