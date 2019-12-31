using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel.Dependencies;
using Eon.Data.EfCore.Metadata.Edm;
using Eon.Data.Querying;
using Eon.Data.TypeSystem;
using Eon.Diagnostics.Logging;
using Eon.Linq;
using Eon.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

using deputils = Eon.ComponentModel.Dependencies.DependencyUtilities;

namespace Eon.Data.EfCore {

	[DebuggerDisplay("{ToString(),nq}")]
	public partial class EfCoreDbContext
		:DbContext, IDataQueryingContext, IDbContextPoolable {

		#region Nested types

		abstract class P_CreateQueryInvoke {

			#region Nested types

			sealed class P_Invoke<TElementType>
				:P_CreateQueryInvoke
				where TElementType : class {

				public P_Invoke() { }

				protected override IQueryable CreateQuery(EfCoreDbContext context) {
					context.EnsureNotNull(nameof(context));
					//
					return context.CreateQuery<TElementType>();
				}

			}

			#endregion

			#region Static members

			static ImmutableDictionary<Type, P_CreateQueryInvoke> __InvokerByElementType = ImmutableDictionary<Type, P_CreateQueryInvoke>.Empty;

			#endregion

			P_CreateQueryInvoke() { }

			public static IQueryable Invoke(Type elementType, EfCoreDbContext context) {
				elementType
					.Arg(nameof(elementType))
					.EnsureNotNull()
					.EnsureClass();
				var invokerImpl =
					ImmutableInterlocked
					.GetOrAdd(
						location: ref __InvokerByElementType,
						key: elementType,
						valueFactory: locKey => ActivationUtilities.RequireConstructor<P_CreateQueryInvoke>(concreteType: typeof(P_Invoke<>).MakeGenericType(elementType))());
				return invokerImpl.CreateQuery(context);
			}

			protected abstract IQueryable CreateQuery(EfCoreDbContext context);

		}

		sealed class P_Disposable
			:DependencySupport {

			EfCoreDbContext _efContext;

			string _efContextString;

			IDataContext2 _dataContext;

			ILogger _logger;

			internal P_Disposable(EfCoreDbContext efContext, IDataContext2 dataContext, ILogger logger, IDependencySupport outerDependencies)
				: base(outerDependencies: outerDependencies) {
				//
				efContext.EnsureNotNull(nameof(efContext));
				//
				_efContext = efContext;
				_efContextString = efContext.ToString();
				_dataContext = dataContext;
				_logger = logger;
			}

			public IDataContext2 DataContext
				=> ReadDA(ref _dataContext);

			protected override void BuildDependencyScope(IOuterDependencyScopeGetter outerScopeGetter, IDependencyExporter exporter, bool ownsExporter, out IDependencyScope scope)
				=> scope = new DependencyScope(outerScopeGetter: outerScopeGetter, exporter: exporter, ownsExporter: ownsExporter, prohibitNewInstanceRequest: false, owner: ReadDA(location: ref _efContext));

			public override IEnumerable<IVh<IDependencyHandler2>> LocalDependencies()
				=> EnumerateDA(value: Enumerable.Concat(first: ReadDA(ref _efContext).LocalDependencies(), second: base.LocalDependencies()));

			protected override void Dispose(bool explicitDispose) {
				if (explicitDispose) {
					var logger = _logger;
					if (!(logger is null))
						logger
							.LogDebug(
								eventId: GenericEventIds.ExplicitDispose,
								message: $"Explicit dispose call.{Environment.NewLine}\tComponent:{Environment.NewLine}\t\t{{component}}",
								args: new object[ ] { _efContextString });
				}
				_efContext = null;
				_dataContext = null;
				_logger = null;
				_efContextString = null;
				//
				base.Dispose(explicitDispose);
			}

		}

		#endregion

		readonly bool _isReadOnly;

		readonly P_Disposable _disposable;

		#region Suspended. Do not remove code below.

		//public EfCoreDbContext(EfCoreDbContextCtorArgs args)
		//	: this(
		//			storeConnection: args.EnsureNotNull(nameof(args)).Value.StoreConnection,
		//			edm: args.Edm,
		//			isReadOnly: args.IsReadOnly,
		//			ownsStoreConnection: args.OwnsStoreConnection,
		//			dataCtx: args.DataContext,
		//			dataCtxDescription: args.DataContextDescription,
		//			dependencies: args.Dependencies) { }

		//protected EfCoreDbContext(
		//	DbConnection storeConnection,
		//	DbCompiledModel edm,
		//	bool isReadOnly,
		//	bool ownsStoreConnection,
		//	IDataContext2 dataCtx = default,
		//	IDataContextDescription dataCtxDescription = default,
		//	IDependencySupport dependencies = default)
		//	: base(existingConnection: storeConnection, model: edm, contextOwnsConnection: ownsStoreConnection) {
		//	//
		//	if (!(dataCtxDescription is null))
		//		dataCtx.EnsureNotNull(nameof(dataCtx));
		//	//
		//	_isReadOnly = isReadOnly;
		//	_preferNoTracking = dataCtx?.PreferNoTracking ?? false;
		//	_disposable = new P_Disposable(efContext: this, dataContext: dataCtx, outerDependencies: dependencies);
		//}

		//public EfCoreDbContext(
		//	DbConnection storeConnection,
		//	bool isReadOnly,
		//	bool ownsStoreConnection,
		//	IDependencySupport dependencies = null)
		//	: base(existingConnection: storeConnection, contextOwnsConnection: ownsStoreConnection) {
		//	//
		//	_isReadOnly = isReadOnly;
		//	_disposable = new P_Disposable(efContext: this, dataContext: null, outerDependencies: dependencies);
		//}

		#endregion

		protected EfCoreDbContext(EfCoreDbContextCtorArgs args)
			: base(options: args.EnsureNotNull(nameof(args)).Value.Options) {
			_isReadOnly = args.IsReadOnly;
			_disposable = new P_Disposable(efContext: this, dataContext: null, logger: args.Logger, outerDependencies: null);
		}

		public EfCoreDbContext(DbContextOptions options)
			: base(options: options) {
			_isReadOnly = true;
			_disposable = new P_Disposable(efContext: this, dataContext: null, logger: null, outerDependencies: null);
		}

		public bool IsReadOnly {
			get {
				_disposable.EnsureNotDisposeState(considerDisposeRequest: false);
				return _isReadOnly;
			}
		}

		/// <summary>
		/// Gets the data context associated with this EF context.
		/// <para>Can be <see langword="null" />.</para>
		/// </summary>
		public IDataContext2 DataContext
			=> _disposable.DataContext;

		protected virtual bool HasNecessityToSaveChanges() {
			if (ChangeTracker.HasChanges())
				return true;
			else {
				ChangeTracker.DetectChanges();
				return ChangeTracker.HasChanges();
			}
		}

		protected sealed override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
			=> base.OnConfiguring(optionsBuilder);

		protected sealed override void OnModelCreating(ModelBuilder builder)
			=> DoCreateModel(builder: builder);

		// TODO: Put strings into the resources.
		//
		protected virtual void DoCreateModel(ModelBuilder builder) {
			// TODO_HIGH. Need for implement another correct way to determine db provider (storeProviderInfo).
			//
			var modelBuildArgs = new EfCoreModelBuilderArgs(builder: builder, contextType: GetType(), storeProviderInfo: EfCoreUtilities.PgsqlDbProvider);
			var errorMessagePrologue = $"Unable to build EF model.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}{Environment.NewLine}";
			foreach (var typeBuilder in GetEntityTypes()) {
				if (typeBuilder as IEfCoreEntityTypeBuilder is null)
					throw new EonException(message: $"{errorMessagePrologue}The type builder is not compatible with '{typeof(IEfCoreEntityTypeBuilder)}'.{Environment.NewLine}\tBuilder:{typeBuilder.FmtStr().GNLI2()}");
				else
					try {
						(typeBuilder as IEfCoreEntityTypeBuilder).BuildEntity(model: modelBuildArgs);
					}
					catch (Exception exception) {
						throw new EonException(message: $"{errorMessagePrologue}The type build error.{Environment.NewLine}\tType:{typeBuilder.FmtStr().GNLI2()}", innerException: exception);
					}
			}
		}

		protected virtual IEnumerable<IDataTypeBuilder> GetEntityTypes() {
			yield break;
		}

		// TODO: Put exception messages into the resources.
		//
		protected void EnsureNotReadOnly() {
			if (_isReadOnly)
				throw new EonException(message: $"Невозможно выполнить требуемую операцию, так как конфигурация данного контекста определяет его как контекста только для чтения.{Environment.NewLine}\tКонтекст:{this.FmtStr().GNLI2()}");
		}

		public sealed override int SaveChanges() {
			if (HasNecessityToSaveChanges()) {
				EnsureNotReadOnly();
				return DoSaveChanges(acceptAllChangesOnSuccess: true);
			}
			else
				return 0;
		}

		public sealed override int SaveChanges(bool acceptAllChangesOnSuccess) {
			if (HasNecessityToSaveChanges()) {
				EnsureNotReadOnly();
				return DoSaveChanges(acceptAllChangesOnSuccess: acceptAllChangesOnSuccess);
			}
			else
				return 0;
		}

		public sealed override async Task<int> SaveChangesAsync(CancellationToken ct) {
			if (HasNecessityToSaveChanges()) {
				EnsureNotReadOnly();
				return await DoSaveChangesAsync(acceptAllChangesOnSuccess: true, ct: ct).ConfigureAwait(false);
			}
			else
				return 0;
		}

		public sealed override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default) {
			if (HasNecessityToSaveChanges()) {
				EnsureNotReadOnly();
				return await DoSaveChangesAsync(acceptAllChangesOnSuccess: acceptAllChangesOnSuccess, ct: ct).ConfigureAwait(false);
			}
			else
				return 0;
		}

		protected virtual Task<int> DoSaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct)
			=> base.SaveChangesAsync(acceptAllChangesOnSuccess: acceptAllChangesOnSuccess, cancellationToken: ct);

		protected virtual int DoSaveChanges(bool acceptAllChangesOnSuccess)
			=> base.SaveChanges(acceptAllChangesOnSuccess: acceptAllChangesOnSuccess);

		IQueryable IDataQueryingContext.CreateQuery(Type elementType)
			=> CreateQuery(elementType);

		protected virtual IQueryable CreateQuery(Type elementType)
			=> P_CreateQueryInvoke.Invoke(elementType: elementType, context: this);

		IQueryable<T> IDataQueryingContext.CreateQuery<T>()
			=> CreateQuery<T>();

		protected virtual IQueryable<T> CreateQuery<T>()
			where T : class
			=> Set<T>();

		IQueryExecutionJob<T> IDataQueryingContext.CreateQueryExecutionJob<T>(IQueryable<T> query)
			=> CreateQueryExecutionJob(query);

		protected virtual IQueryExecutionJob<T> CreateQueryExecutionJob<T>(IQueryable<T> query)
			where T : class {
			query.EnsureNotNull(nameof(query));
			//
			if (query.IsEmptyQueryable())
				return EmptyQueryExecutionJob<T>.Singleton;
			else
				return new QueryExecutionJob<T>(query);
		}

		public IDependencyScope GetDependencyScope()
			=> _disposable.GetDependencyScope();

		public virtual IEnumerable<IVh<IDependencyHandler2>> LocalDependencies() {
			yield return deputils.CreateHandlerForShared(factory: () => this.ToValueHolder(ownsValue: false));
		}

		void IDbContextPoolable.SetPool(IDbContextPool contextPool)
			=> throw new NotSupportedException(message: $"Pooling for this EF context must be avoided.{Environment.NewLine}\tContext:{this.FmtStr().GNLI2()}");

		public override string ToString()
			=> $"{GetType().Name}, hc0x{RuntimeHelpers.GetHashCode(o: this).ToString("x8")}";

		public override void Dispose() {
			_disposable.Dispose();
			//
			base.Dispose();
		}

	}

}