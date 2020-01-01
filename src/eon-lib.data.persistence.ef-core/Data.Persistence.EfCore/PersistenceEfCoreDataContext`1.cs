using System;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel.Dependencies;
using Eon.Context;
using Eon.Data.EfCore;
using Eon.Data.Querying;
using Eon.Diagnostics.Logging;
using Eon.Linq;
using Eon.Reflection;
using Eon.Threading;
using Eon.Threading.Tasks;
using Eon.Transactions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

using itrlck = Eon.Threading.InterlockedUtilities;
using vlt = Eon.Threading.VolatileUtilities;

namespace Eon.Data.Persistence.EfCore {

	[DebuggerDisplay("{ToString(),nq}")]
	public partial class PersistenceEfCoreDataContext<TEfDbContext>
		:DependencySupport, IPersistenceDataContext<TEfDbContext>
		where TEfDbContext : EfCoreDbContext {

		#region Nested types

		[DebuggerDisplay("{ToString(),nq}")]
		sealed class P_TxScope
			:ITransactionScopeProxy {

			sealed class P_State {

				public readonly bool InitializationDone;

				public readonly IDbContextTransaction RealTx;

				public readonly Action<P_TxScope> DisposeCallback;

				public readonly Action<P_TxScope> CompleteCallback;

				public readonly bool ShouldRollback;

				public readonly bool CommitIntention;

				public readonly bool DisposeStart;

				internal P_State(bool initializationDone, IDbContextTransaction realTx, Action<P_TxScope> disposeCallback, Action<P_TxScope> completeCallback, bool shouldRollback = default, bool commitIntention = default, bool disposeStart = default) {
					InitializationDone = initializationDone;
					RealTx = realTx;
					DisposeCallback = disposeCallback;
					CompleteCallback = completeCallback;
					ShouldRollback = shouldRollback;
					CommitIntention = commitIntention;
					DisposeStart = disposeStart;
				}

				internal P_State(
					P_State other,
					ArgumentPlaceholder<bool> initializationDone = default,
					ArgumentPlaceholder<IDbContextTransaction> realTx = default,
					ArgumentPlaceholder<Action<P_TxScope>> disposeCallback = default,
					ArgumentPlaceholder<Action<P_TxScope>> completeCallback = default,
					ArgumentPlaceholder<bool> shouldRollback = default,
					ArgumentPlaceholder<bool> commitIntention = default,
					ArgumentPlaceholder<bool> disposeStart = default) {
					//
					InitializationDone = initializationDone.Substitute(other.InitializationDone);
					RealTx = realTx.Substitute(other.RealTx);
					DisposeCallback = disposeCallback.Substitute(other.DisposeCallback);
					CompleteCallback = completeCallback.Substitute(other.CompleteCallback);
					ShouldRollback = shouldRollback.Substitute(other.ShouldRollback);
					CommitIntention = commitIntention.Substitute(other.CommitIntention);
					DisposeStart = disposeStart.Substitute(other.DisposeStart);
				}

			}

			P_State _state;

			public P_TxScope(IsolationLevel isolationLevel) {
				Outer = null;
				P_OuterShouldRollback = false;
				NestingLevel = 0;
				IsolationLevel = isolationLevel;
			}

			public P_TxScope(P_TxScope outer) {
				Outer = outer;
				P_OuterShouldRollback = outer.ShouldRollback;
				NestingLevel = checked(outer.NestingLevel + 1);
				IsolationLevel = outer.IsolationLevel;
			}

			public P_TxScope Outer { get; }

			bool P_OuterShouldRollback { get; }

			public int NestingLevel { get; }

			public IsolationLevel IsolationLevel { get; }

			public bool InitializationDone
				=> itrlck.Get(location: ref _state)?.InitializationDone ?? false;

			public bool CommitIntention
				=> itrlck.Get(location: ref _state)?.CommitIntention ?? false;

			public bool ShouldRollback
				=> P_OuterShouldRollback || (itrlck.Get(location: ref _state)?.ShouldRollback ?? false);

			public IDisposable RealScope
				=> NopDisposable.Instance;

			public IDbContextTransaction RealTx
				=> itrlck.Get(location: ref _state)?.RealTx;

			public bool DisposeStart
				=> itrlck.Get(location: ref _state)?.DisposeStart ?? false;

			internal void Initialize(Action<P_TxScope> disposeCallback, Action<P_TxScope> completeCallback, IDbContextTransaction realTx = default) {
				if (!itrlck.UpdateBool(location: ref _state, value: new P_State(initializationDone: true, disposeCallback: disposeCallback, completeCallback: completeCallback, realTx: realTx), comparand: null))
					throw new EonException("Initialization must be done once.");
			}

			internal void SetCommitIntention() {
				for (; ; ) {
					var current = itrlck.Get(location: ref _state);
					if (current.DisposeStart)
						throw new EonException(message: "Dispose has been called already.");
					else if (current.CommitIntention || itrlck.UpdateBool(location: ref _state, comparand: current, value: new P_State(other: current, commitIntention: true)))
						break;
				}
			}

			internal void SetShouldRollback(bool value) {
				if (!P_OuterShouldRollback)
					for (; ; ) {
						var current = itrlck.Get(location: ref _state);
						if (current.DisposeStart)
							throw new EonException(message: "Dispose has been called already.");
						else if (current.ShouldRollback || current.ShouldRollback == value || itrlck.UpdateBool(location: ref _state, comparand: current, value: new P_State(other: current, shouldRollback: value)))
							break;
					}
			}

			public void Complete()
				=> itrlck.Get(location: ref _state).CompleteCallback(obj: this);

			public void Reset()
				=> itrlck.SetNull(location: ref _state);

			public void Dispose() {
				for (; ; ) {
					var currentState = itrlck.Get(location: ref _state);
					if (currentState is null) {
						if (itrlck.UpdateBool(location: ref _state, comparand: currentState, value: new P_State(initializationDone: false, realTx: null, completeCallback: null, disposeCallback: null, disposeStart: true)))
							break;
					}
					else if (currentState.DisposeStart)
						break;
					else if (itrlck.UpdateBool(location: ref _state, comparand: currentState, value: new P_State(other: currentState, disposeCallback: null, completeCallback: null, realTx: null, disposeStart: true))) {
						currentState.DisposeCallback?.Invoke(obj: this);
						if (!currentState.ShouldRollback && currentState.CommitIntention)
							currentState.RealTx?.Commit();
						else
							currentState.RealTx?.Rollback();
						currentState.RealTx?.Dispose();
						break;
					}
				}
			}

			public override string ToString()
				=> $"(nesting-level: {NestingLevel.ToString("d", CultureInfo.InvariantCulture)}, commit-intention: {(CommitIntention ? "y" : "n")}, should-rollback: {(ShouldRollback ? "y" : "n")}, dispose-start: {(DisposeStart ? "y" : "n")}, isolation: {IsolationLevel})";

		}

		#endregion

		#region Static members

		static readonly Type __TypeOfTEfDbContext = typeof(TEfDbContext);

		static Func<EfCoreDbContextCtorArgs, TEfDbContext> __EfDbContextCtor;

		// TODO: Put strings into the resources.
		//
		static Func<EfCoreDbContextCtorArgs, TEfDbContext> P_RequireEfDbContextCtor() {
			var ctor = itrlck.Get(ref __EfDbContextCtor);
			if (ctor is null) {
				try {
					ctor = ActivationUtilities.RequireConstructor<EfCoreDbContextCtorArgs, TEfDbContext>();
				}
				catch (Exception exception) {
					throw new EonException(message: $"An exception occurred while requiring the special constructor of the EF context class '{typeof(TEfDbContext)}'.", innerException: exception);
				}
				itrlck.UpdateIfNull(location: ref __EfDbContextCtor, value: ctor);
			}
			return ctor;
		}

		#endregion

		ILogger _logger;

		IPersistenceDataContextProvider<IPersistenceDataContext<TEfDbContext>> _provider;

		DbContextOptions<TEfDbContext> _efDbContextOptions;

		SemaphoreSlim _initializationLock;

		bool _isInitialized;

		DisposableLazy<TEfDbContext> _efCtxLazy;

		EventHandler<EfCoreDbContextEventArgs<TEfDbContext>> _eventHandler_AdjustEfDbContext;

		P_StorageOpCtx _storageOpCtx;

		ImmutableStack<P_TxScope> _txScopes;

		public PersistenceEfCoreDataContext(IPersistenceDataContextProvider<IPersistenceDataContext<TEfDbContext>> provider, DbContextOptions<TEfDbContext> efDbContextOptions, ILogger logger)
			: base(outerDependencies: provider.EnsureNotNull(nameof(provider)).Value) {
			efDbContextOptions.EnsureNotNull(nameof(efDbContextOptions));
			if (!efDbContextOptions.IsFrozen)
				throw
					new ArgumentException(
						message: $"EF context options must be frozen (see {nameof(DbContextOptions)}.{nameof(DbContextOptions.IsFrozen)}) before using by this component.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}",
						paramName: nameof(efDbContextOptions));
			//
			_provider = provider;
			_efDbContextOptions = efDbContextOptions;
			_initializationLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
			_isInitialized = false;
			_efCtxLazy = new DisposableLazy<TEfDbContext>(factory: () => P_CreateAndAdjustEfDbContext(isReadOnly: false), ownsValue: true);
			_logger = logger;
			_txScopes = ImmutableStack<P_TxScope>.Empty;
		}

		// TODO: Put strings into the resources.
		//
		public PersistenceEfCoreDataContext(IPersistenceDataContextProvider<IPersistenceDataContext<TEfDbContext>> provider, DbContextOptions<TEfDbContext> efDbContextOptions)
			: this(provider: provider, efDbContextOptions: efDbContextOptions, logger: null) { }

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

		bool P_IsInitialized {
			get => ReadDA(ref _isInitialized);
			set => vlt.Write(ref _isInitialized, value);
		}

		public bool PreferNoTracking {
			get {
				EnsureNotDisposeState();
				return false;
			}
		}

		public IPersistenceDataContextProvider<IPersistenceDataContext<TEfDbContext>> Provider
			=> ReadDA(ref _provider);

		IPersistenceDataContextProvider<IPersistenceDataContext> IPersistenceDataContext.Provider
			=> Provider;

		IDataContextProvider<IDataContext2> IDataContext2.Provider
			=> Provider;

		IPersistenceDataContextProvider<IPersistenceDataContext> IPersistenceDataContext<TEfDbContext>.Provider
			=> Provider;

		public DbContextOptions<TEfDbContext> EfDbContextOptions
		=> ReadDA(ref _efDbContextOptions);

		Type IPersistenceDataContext.ContextDiscriminatorType
			=> __TypeOfTEfDbContext;

		public event EventHandler<EfCoreDbContextEventArgs<TEfDbContext>> AdjustEfDbContext {
			add => AddEventHandler(ref _eventHandler_AdjustEfDbContext, value);
			remove => RemoveEventHandler(ref _eventHandler_AdjustEfDbContext, value);
		}

		// TODO: Put strings into the resources.
		//
		public ITransactionScopeProxy BeginTx(IsolationLevel level) {
			var efCtx = ReadDA(location: ref _efCtxLazy, considerDisposeRequest: true).Value;
			P_TxScope scope = default;
			IDbContextTransaction realTx = default;
			try {
				for (; ; ) {
					scope?.Dispose();
					var current = ReadDA(location: ref _txScopes, considerDisposeRequest: true);
					if (current.IsEmpty)
						scope = new P_TxScope(isolationLevel: level);
					else {
						var currentScope = current.Peek();
						if (currentScope.IsolationLevel != level)
							throw new EonException($"Specified transaction isolation level '{level}' is incompatible with this transaction scope isolation level '{currentScope.IsolationLevel}'.{Environment.NewLine}\tTx scope:{currentScope.FmtStr().GNLI2()}");
						else if (!currentScope.InitializationDone)
							throw new EonException(message: $"This transaction scope has not initialized yet.{Environment.NewLine}\tTx scope:{currentScope.FmtStr().GNLI2()}");
						else if (currentScope.DisposeStart)
							throw new EonException(message: $"This transaction scope has unappropriate state to begin the nested scope.{Environment.NewLine}\tTx scope:{currentScope.FmtStr().GNLI2()}");
						else if (currentScope.CommitIntention) {
							try { currentScope.SetShouldRollback(value: true); }
							catch (Exception exception) { throw new EonException(message: $"An exception occurred while handling this transaction scope.{Environment.NewLine}\tTx scope:{currentScope.FmtStr().GNLI2()}", innerException: exception); }
							throw new EonException(message: $"This transaction scope have the commit intention. The nested scope cann't be started.{Environment.NewLine}To avoid the possible data inconsistency this transaction scope has been marked as to be rolled back.{Environment.NewLine}\tTx scope:{currentScope.FmtStr().GNLI2()}");
						}
						else
							scope = new P_TxScope(outer: currentScope);
					}
					var changed = current.Push(scope);
					if (UpdDABool(location: ref _txScopes, comparand: current, value: changed)) {
						if (scope.NestingLevel == 0) {
							try {
								realTx = efCtx.Database.BeginTransaction(isolationLevel: level);
								scope.Initialize(realTx: realTx, disposeCallback: disposeCallback, completeCallback: completionCallback);
							}
							catch {
								realTx?.Dispose();
								realTx = null;
								throw;
							}
						}
						else
							scope.Initialize(realTx: null, disposeCallback: disposeCallback, completeCallback: completionCallback);
						return scope;
					}
				}
			}
			catch {
				if (!(scope is null)) {
					scope.Reset();
					for (; ; ) {
						var current = itrlck.Get(location: ref _txScopes);
						if (current is null || current.IsEmpty || !ReferenceEquals(scope, current.Peek()))
							break;
						else {
							var changed = current.Pop();
							if (itrlck.UpdateBool(location: ref _txScopes, value: changed, comparand: current))
								break;
						}
					}
					realTx?.Dispose();
					scope.Dispose();
				}
				throw;
			}
			//
			void completionCallback(P_TxScope locScope) {
				var current = ReadDA(location: ref _txScopes, considerDisposeRequest: true);
				if (current.IsEmpty || !ReferenceEquals(current.Peek(), locScope))
					throw new EonException(message: $"Consistency violation.{txScopeStackString(locScope: locScope, locScopes: current).FmtStr().GNLI()}");
				else
					locScope.SetCommitIntention();
			}
			void disposeCallback(P_TxScope locScope) {
				var logger = _logger;
				if (!(logger is null)) {
					logger
						.LogDebug(
							eventId: GenericEventIds.ExplicitDispose,
							message: $"Own tx scope explicit dispose call.{txScopeStackString(locScope).FmtStr().GNLI()}{Environment.NewLine}\tComponent:{Environment.NewLine}\t\t{{component}}",
							args: new object[ ] { ToString() });
				}
				for (; ; ) {
					var current = ReadDA(location: ref _txScopes, considerDisposeRequest: true);
					if (current.IsEmpty || !ReferenceEquals(current.Peek(), locScope))
						throw new EonException(message: $"Consistency violation.{txScopeStackString(locScope: locScope, locScopes: current).FmtStr().GNLI()}");
					else {
						var changed = current.Pop();
						if (!changed.IsEmpty)
							changed.Peek().SetShouldRollback(!locScope.CommitIntention || locScope.ShouldRollback);
						if (UpdDABool(location: ref _txScopes, comparand: current, value: changed))
							break;
					}
				}
			}
			string txScopeStackString(P_TxScope locScope, ImmutableStack<P_TxScope> locScopes = default)
				=> $"Tx scope:{locScope.FmtStr().GNLI()}{Environment.NewLine}Tx scope stack:{string.Join(separator: $",{Environment.NewLine}", values: ((locScopes ?? itrlck.Get(location: ref _txScopes))?.Select(k => k.ToString())).EmptyIfNull()).FmtStr().GNLI()}";
		}

		TEfDbContext P_CreateAndAdjustEfDbContext(bool isReadOnly) {
			var efContext = default(TEfDbContext);
			try {
				var ctor = P_RequireEfDbContextCtor();
				var ctorArgs = new EfCoreDbContextCtorArgs(isReadOnly: isReadOnly, options: EfDbContextOptions, logger: ReadDA(location: ref _logger));
				efContext = ctor(arg: ctorArgs);
				OnAdjustEfDbContext(eventArgs: new EfCoreDbContextEventArgs<TEfDbContext>(ctx: efContext));
				return efContext;
			}
			catch (Exception exception) {
				efContext?.Dispose(exception: exception);
				throw;
			}
		}

		protected virtual void OnAdjustEfDbContext(EfCoreDbContextEventArgs<TEfDbContext> eventArgs) {
			eventArgs.EnsureNotNull(nameof(eventArgs));
			//
			eventArgs.Context.ChangeTracker.AutoDetectChangesEnabled = false;
			// Point to inject some interceptions of EF.
			//
			// ...
			EventHandler<EfCoreDbContextEventArgs<TEfDbContext>> eventHandler;
			if ((eventHandler = ReadDA(ref _eventHandler_AdjustEfDbContext)) != null)
				eventHandler(this, eventArgs);
		}

		public async Task InitializeAsync(CancellationToken ct) {
			if (!P_IsInitialized) {
				if (ct.CanBeCanceled) {
					ct.ThrowExceptionIfCancellationRequested();
					using var ctx = ContextUtilities.Create(ct: ct);
					await InitializeAsync(ctx: ctx).ConfigureAwait(false);
				}
				else
					await InitializeAsync(ctx: null).ConfigureAwait(false);
			}
		}

		public async Task InitializeAsync(IContext ctx = default) {
			if (!P_IsInitialized) {
				var lck = ReadDA(ref _initializationLock, considerDisposeRequest: true);
				var lckAcquired = false;
				try {
					lckAcquired = await lck.WaitAsync(millisecondsTimeout: TaskUtilities.DefaultAsyncTimeoutMilliseconds, cancellationToken: ctx.Ct()).ConfigureAwait(false);
					if (!lckAcquired)
						throw new LockAcquisitionFailException(reason: LockAcquisitionFailReason.TimeoutElapsed);
					//
					if (!P_IsInitialized) {
						ReadDA(location: ref _efCtxLazy).Value?.GetType();
						await InitializeInnerAsync(ctx: ctx).ConfigureAwait(false);
						//
						P_IsInitialized = true;
					}
				}
				finally {
					if (lckAcquired)
						try { lck.Release(); }
						catch (ObjectDisposedException) { }
				}
			}
		}

		public Task InitializeAsync() {
			try {
				if (P_IsInitialized)
					return Task.CompletedTask;
				else
					return InitializeAsync(ct: CancellationToken.None);
			}
			catch (Exception exception) {
				return Task.FromException(exception: exception);
			}
		}

		protected virtual Task InitializeInnerAsync(IContext ctx = default)
			=> Task.CompletedTask;

		public virtual IPersistenceStorageOperationContext<IPersistenceEntity> GetStorageOperationContext() {
			var existingCtx = ReadDA(location: ref _storageOpCtx);
			if (existingCtx is null) {
				var createdCtx = default(P_StorageOpCtx);
				try {
					createdCtx = new P_StorageOpCtx(dataCtx: this, efCtx: ReadDA(location: ref _efCtxLazy, considerDisposeRequest: true).Value);
					existingCtx = UpdDAIfNull(location: ref _storageOpCtx, value: createdCtx);
				}
				catch (Exception exception) {
					createdCtx?.Dispose(exception: exception);
					throw;
				}
				finally {
					if (!ReferenceEquals(existingCtx, createdCtx))
						createdCtx?.Dispose();
				}
			}
			return existingCtx;
		}

		// TODO: Put strings into the resources.
		//
		public TQueryingContext GetQueryingContext<TQueryingContext>()
			where TQueryingContext : class, IDataQueryingContext {
			//
			var existing = ReadDA(ref _efCtxLazy).Value;
			if (!(existing is TQueryingContext required))
				throw
					new EonException(
						message: $"Existing querying context is incompatible with the required type.{Environment.NewLine}\tData context:{this.FmtStr().GNLI2()}{Environment.NewLine}\tType of existing querying context:{Environment.NewLine}{existing.GetType().FmtStr().G().IndentLines2()}{Environment.NewLine}\tType of required querying context:{Environment.NewLine}{typeof(TQueryingContext).FmtStr().G().IndentLines2()}");
			else
				return required;
		}

		public IDataQueryingContext GetQueryingContext()
			=> ReadDA(ref _efCtxLazy).Value;

		DependencyResult IDependencyHandler.ExecuteResolution(IDependencyResolutionContext resolutionContext)
			=> DependencyResult.None;

		public override string ToString() {
			var provider = _provider;
			return $"{GetType().Name}, hc0x{RuntimeHelpers.GetHashCode(o: this).ToString("x8")}, provider {(provider is null ? "'null'" : $"'{_provider.GetType().Name}, hc0x{RuntimeHelpers.GetHashCode(o: provider).ToString("x8")}'")}";
		}

		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose)
				itrlck.Get(location: ref _txScopes)?.Observe(z => z.RealTx?.Dispose());
			//
			base.FireBeforeDispose(explicitDispose);
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				var logger = _logger;
				if (!(logger is null)) {
					var componentString = ToString();
					logger
						.LogDebug(
							eventId: GenericEventIds.ExplicitDispose,
							message: $"Explicit dispose call.{Environment.NewLine}\tComponent:{Environment.NewLine}\t\t{{component}}",
							args: new object[ ] { componentString });
				}
				_initializationLock?.Dispose();
				_efCtxLazy?.Dispose();
				_storageOpCtx?.Dispose();
			}
			_provider = null;
			_efDbContextOptions = null;
			_efCtxLazy = null;
			_eventHandler_AdjustEfDbContext = null;
			_storageOpCtx = null;
			_initializationLock = null;
			_logger = null;
			_txScopes = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}