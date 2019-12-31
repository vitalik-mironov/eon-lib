using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Eon.Collections;
using Eon.ComponentModel.Dependencies;
using Eon.Context;
using Eon.Data.Storage;
using Eon.Linq;
using Eon.Reflection;
using Eon.Threading;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Eon.Data.Persistence.EfCore {

	public partial class PersistenceEfCoreDataContext<TEfDbContext> {

		#region Nested types

		struct P_EntityChangeTrackingStateWrap<TEntity>
			where TEntity : class {

			public TEntity Entity;

			public EntityState State;

			public EntityEntry<TEntity> Entry;

		}

		[DebuggerDisplay("{ToString(),nq}")]
		abstract class P_StorageOpCtxBase
			:DependencySupport {

			PersistenceEfCoreDataContext<TEfDbContext> _dataCtx;

			Func<TEfDbContext> _efCtxGetter;

			DisposableLazy<TEfDbContext> _efCtxLazy;

			protected P_StorageOpCtxBase(P_StorageOpCtx baseCtx)
				: base(outerDependencies: baseCtx.EnsureNotNull(nameof(baseCtx)).Value.DataContext) {
				_dataCtx = baseCtx.DataContext;
				_efCtxGetter = baseCtx.GetDataChangeEfDbContext;
				_efCtxLazy = new DisposableLazy<TEfDbContext>(factory: P_GetDataChangeEfDbContext, ownsValue: false);
			}

			protected P_StorageOpCtxBase(PersistenceEfCoreDataContext<TEfDbContext> dataCtx, TEfDbContext efCtx)
				: base(outerDependencies: dataCtx.EnsureNotNull(nameof(dataCtx)).Value) {
				efCtx.EnsureNotNull(nameof(efCtx));
				//
				_dataCtx = dataCtx;
				_efCtxGetter = null;
				_efCtxLazy = new DisposableLazy<TEfDbContext>(value: efCtx, ownsValue: false);
			}

			TEfDbContext P_GetDataChangeEfDbContext() {
				var result = ReadDA(ref _efCtxGetter, considerDisposeRequest: true)();
				EnsureNotDisposeState(considerDisposeRequest: true);
				return result;
			}

			public PersistenceEfCoreDataContext<TEfDbContext> DataContext
				=> ReadDA(ref _dataCtx);

			public TEfDbContext GetDataChangeEfDbContext()
				=> ReadDA(ref _efCtxLazy, considerDisposeRequest: true).Value;

			public abstract Task WriteBaseTypeEntityAsync(IPersistenceEntity entity, IContext ctx = default);

			public abstract Task InsertBaseTypeEntityAsync(IPersistenceEntity entity, bool newReferenceKeyAssignmentAllowed = default, IContext ctx = default);

			public abstract Task DeleteBaseTypeEntityAsync(IPersistenceEntity entity, IContext ctx = default);

			public abstract Task UpdateBaseTypeEntityAsync(IPersistenceEntity entity, IContext ctx = default);

			public override string ToString() {
				var dataCtx = _dataCtx;
				return $"{GetType().Name}, hc0x{RuntimeHelpers.GetHashCode(o: this).ToString("x8")}, data-context: {(dataCtx is null ? "'null'" : $"'{dataCtx.ToString()}'")}";
			}

			protected override void Dispose(bool explicitDispose) {
				if (explicitDispose) {
					_efCtxLazy?.Dispose();
				}
				_dataCtx = null;
				_efCtxLazy = null;
				_efCtxGetter = null;
				//
				base.Dispose(explicitDispose);
			}

		}

		[DebuggerDisplay("{ToString(),nq}")]
		sealed class P_StrictedStorageOpCtx<TEntity, TReferenceKey>
			:P_StorageOpCtxBase, IPersistenceStorageOperationContext<TEntity, TReferenceKey>
			where TEntity : class, IPersistenceEntity<TReferenceKey>
			where TReferenceKey : struct {

			#region Nested types

			enum SaveOpCode {

				Write = 0,

				Insert,

				Update,

				Delete

			}

			#endregion

			P_StorageOpCtx _baseCtx;

			readonly PersistenceEntityReferenceKeyTypeDescriptor _referenceKeyTypeDescriptor;

			IReferenceKeyProvider<TReferenceKey> _referenceKeyProvider;

			internal P_StrictedStorageOpCtx(P_StorageOpCtx baseCtx, PersistenceEntityReferenceKeyTypeDescriptor referenceKeyTypeDescriptor)
				: base(baseCtx: baseCtx) {
				//
				_baseCtx = baseCtx;
				_referenceKeyTypeDescriptor = referenceKeyTypeDescriptor;
			}

			IPersistenceDataContext IPersistenceStorageOperationContext<TEntity>.DataContext
				=> DataContext;

			async Task<IReferenceKeyProvider<TReferenceKey>> P_RequireReferenceKeyProviderAsync(IContext ctx = default) {
				var provider = ReadDA(ref _referenceKeyProvider);
				if (provider is null) {
					provider = await DataContext.Provider.RequireReferenceKeyProviderAsync<TReferenceKey>(keyTypeDescriptor: _referenceKeyTypeDescriptor, ctx: ctx);
					UpdDAIfNullBool(location: ref _referenceKeyProvider, value: provider, current: out provider);
				}
				return provider;
			}

			async Task P_SaveAsync(TEntity entity, SaveOpCode opCode, bool prohibitReferenceKeyAssignment, IContext ctx = default) {
				if (entity is null)
					throw new ArgumentNullException(paramName: nameof(entity));
				//
				await P_SaveAsync(entityCollection: new TEntity[ ] { entity }, opCode: opCode, prohibitReferenceKeyAssignment: prohibitReferenceKeyAssignment, ctx: ctx).ConfigureAwait(false);
			}

			// TODO: Put strings into the resources.
			//
			async Task P_SaveAsync(TEntity[ ] entityCollection, SaveOpCode opCode, bool prohibitReferenceKeyAssignment, IContext ctx = default) {
				if (entityCollection is null)
					throw new ArgumentNullException(paramName: nameof(entityCollection));
				//
				var entityCollectionLength = entityCollection.Length;
				if (entityCollectionLength > 0) {
					var entityCollectionLowerBound = entityCollection.GetLowerBound(dimension: 0);
					var locEntityCollection = new P_EntityChangeTrackingStateWrap<TEntity>[ entityCollectionLength ];
					var ct = ctx.Ct();
					var efContext = GetDataChangeEfDbContext();
					switch (opCode) {
						case SaveOpCode.Write:
							if (prohibitReferenceKeyAssignment) {
								for (var offset = 0; offset < entityCollectionLength; offset++) {
									var entity = entityCollection[ offset + entityCollectionLowerBound ];
									if (entity is null)
										throw new ArgumentNullException(paramName: $"{nameof(entityCollection)}[{offset + entityCollectionLowerBound:d}]");
									else {
										locEntityCollection[ offset ].Entity = entity;
										locEntityCollection[ offset ].State = entity.IsNew ? EntityState.Added : EntityState.Modified;
									}
								}
							}
							else {
								IReferenceKeyProvider<TReferenceKey> keyProvider = default;
								for (var offset = 0; offset < entityCollectionLength; offset++) {
									var entity = entityCollection[ offset + entityCollectionLowerBound ];
									if (entity is null)
										throw new ArgumentNullException(paramName: $"{nameof(entityCollection)}[{offset + entityCollectionLowerBound:d}]");
									else {
										locEntityCollection[ offset ].Entity = entity;
										if (entity.IsNew) {
											keyProvider = keyProvider ?? await P_RequireReferenceKeyProviderAsync(ctx: ctx).ConfigureAwait(false);
											entity.ReferenceKey = await keyProvider.NextKeyAsync(ctx: ctx).ConfigureAwait(false);
											locEntityCollection[ offset ].State = EntityState.Added;
										}
										else
											locEntityCollection[ offset ].State = EntityState.Modified;
									}
								}
							}
							break;
						case SaveOpCode.Insert:
							if (prohibitReferenceKeyAssignment) {
								for (var offset = 0; offset < entityCollectionLength; offset++) {
									var entity = entityCollection[ offset + entityCollectionLowerBound ];
									if (entity is null)
										throw new ArgumentNullException(paramName: $"{nameof(entityCollection)}[{offset + entityCollectionLowerBound:d}]");
									else {
										locEntityCollection[ offset ].Entity = entity;
										locEntityCollection[ offset ].State = EntityState.Added;
									}
								}
							}
							else {
								IReferenceKeyProvider<TReferenceKey> keyProvider = default;
								for (var offset = 0; offset < entityCollectionLength; offset++) {
									var entity = entityCollection[ offset + entityCollectionLowerBound ];
									if (entity is null)
										throw new ArgumentNullException(paramName: $"{nameof(entityCollection)}[{offset + entityCollectionLowerBound:d}]");
									else {
										if (entity.IsNew) {
											keyProvider = keyProvider ?? await P_RequireReferenceKeyProviderAsync(ctx: ctx).ConfigureAwait(false);
											entity.ReferenceKey = await keyProvider.NextKeyAsync(ctx: ctx).ConfigureAwait(false);
										}
										locEntityCollection[ offset ].Entity = entity;
										locEntityCollection[ offset ].State = EntityState.Added;
									}
								}
							}
							break;
						case SaveOpCode.Update:
							for (var offset = 0; offset < entityCollectionLength; offset++) {
								var entity = entityCollection[ offset + entityCollectionLowerBound ];
								if (entity is null)
									throw new ArgumentNullException(paramName: $"{nameof(entityCollection)}[{offset + entityCollectionLowerBound:d}]");
								else {
									locEntityCollection[ offset ].Entity = entity;
									locEntityCollection[ offset ].State = EntityState.Modified;
								}
							}
							break;
						case SaveOpCode.Delete:
							for (var offset = 0; offset < entityCollectionLength; offset++) {
								var entity = entityCollection[ offset + entityCollectionLowerBound ];
								if (entity is null)
									throw new ArgumentNullException(paramName: $"{nameof(entityCollection)}[{offset + entityCollectionLowerBound:d}]");
								else {
									locEntityCollection[ offset ].Entity = entity;
									locEntityCollection[ offset ].State = EntityState.Deleted;
								}
							}
							break;
						default:
							throw new NotSupportedException(message: $"Store operation code '{opCode}' is not supported.");
					}
					// Сохранение.
					//
					var saveFault = default(Exception);
					try {
						for (var y = 0; y < entityCollectionLength; y++) {
							try {
								(locEntityCollection[ y ].Entry = efContext.Entry(entity: locEntityCollection[ y ].Entity)).State = locEntityCollection[ y ].State;
							}
							catch (Exception exception) {
								throw
									new EonException(
										message: $"An exception occurred while placing the entity in EF context (change tracker).{Environment.NewLine}\tEntity ({y + 1:d} from {entityCollectionLength:d}):{locEntityCollection[ y ].Entity.FmtStr().GNLI2()}{Environment.NewLine}\tPlacing entity state:{locEntityCollection[ y ].State.FmtStr().GNLI2()}{Environment.NewLine}\tStore operation code:{opCode.FmtStr().GNLI2()}",
										innerException: exception);
							}
						}
						//
						try {
							await efContext.SaveChangesAsync(acceptAllChangesOnSuccess: true, ct: ct).ConfigureAwait(false);
						}
						catch (Exception exception) {
#if TRG_NETFRAMEWORK
							if (exception is OptimisticConcurrencyException || exception is DbUpdateConcurrencyException)
#else
							if (exception is DbUpdateConcurrencyException)
#endif
								throw new DataStorageOptimisticConcurrencyException(message: $"Объект:{(locEntityCollection.Length == 1 ? locEntityCollection[ 0 ].Entity?.ToString() : $"<коллекция из {locEntityCollection.Length:d} элементов>").FmtStr().GNLI2()}");
							throw;
						}
					}
					catch (Exception exception) {
						saveFault = exception;
						throw;
					}
					finally {
						try {
							for (var y = 0; y < entityCollectionLength; y++)
								locEntityCollection[ y ].Entry.State = EntityState.Detached;
						}
						catch (Exception exception) {
							if (saveFault is null)
								throw;
							else
								throw new AggregateException(saveFault, exception);
						}
					}
				}
			}

			public Task WriteAsync(TEntity entity, IContext ctx = default)
				=> P_SaveAsync(entity: entity, opCode: SaveOpCode.Write, prohibitReferenceKeyAssignment: false, ctx: ctx);

			public Task WriteAsync(TEntity[ ] entityCollection, IContext ctx = default)
				=> P_SaveAsync(entityCollection: entityCollection, opCode: SaveOpCode.Write, prohibitReferenceKeyAssignment: false, ctx: ctx);

			public Task InsertAsync(TEntity entity, bool newReferenceKeyAssignmentAllowed = default, IContext ctx = default)
				=> P_SaveAsync(entity: entity, opCode: SaveOpCode.Insert, prohibitReferenceKeyAssignment: !newReferenceKeyAssignmentAllowed, ctx: ctx);

			public Task InsertAsync(TEntity[ ] entityCollection, bool newReferenceKeyAssignmentAllowed = default, IContext ctx = default)
				=> P_SaveAsync(entityCollection: entityCollection, opCode: SaveOpCode.Insert, prohibitReferenceKeyAssignment: !newReferenceKeyAssignmentAllowed, ctx: ctx);

			public Task UpdateAsync(TEntity entity, IContext ctx = default)
				=> P_SaveAsync(entity: entity, opCode: SaveOpCode.Update, prohibitReferenceKeyAssignment: true, ctx: ctx);

			public Task UpdateAsync(TEntity[ ] entityCollection, IContext ctx = default)
				=> P_SaveAsync(entityCollection: entityCollection, opCode: SaveOpCode.Update, prohibitReferenceKeyAssignment: true, ctx: ctx);

			public Task DeleteAsync(TEntity entity, IContext ctx = default)
				=> P_SaveAsync(entity: entity, opCode: SaveOpCode.Delete, prohibitReferenceKeyAssignment: true, ctx: ctx);

			public Task DeleteAsync(TEntity[ ] entityCollection, IContext ctx = default)
				=> P_SaveAsync(entityCollection: entityCollection, opCode: SaveOpCode.Delete, prohibitReferenceKeyAssignment: true, ctx: ctx);

			public override async Task WriteBaseTypeEntityAsync(IPersistenceEntity entity, IContext ctx = default)
				=> await WriteAsync(entity: (TEntity)entity, ctx: ctx).ConfigureAwait(false);

			public override async Task InsertBaseTypeEntityAsync(IPersistenceEntity entity, bool newReferenceKeyAssignmentAllowed = default, IContext ctx = default)
				=> await InsertAsync(entity: (TEntity)entity, newReferenceKeyAssignmentAllowed: newReferenceKeyAssignmentAllowed, ctx: ctx).ConfigureAwait(false);

			public override async Task DeleteBaseTypeEntityAsync(IPersistenceEntity entity, IContext ctx = default)
				=> await DeleteAsync(entity: (TEntity)entity, ctx: ctx).ConfigureAwait(false);

			public override async Task UpdateBaseTypeEntityAsync(IPersistenceEntity entity, IContext ctx = default)
				=> await UpdateAsync(entity: (TEntity)entity, ctx: ctx).ConfigureAwait(false);

			IPersistenceStorageOperationContext<TStrictedEntity> IPersistenceStorageOperationContext<TEntity>.GetStricted<TStrictedEntity>()
				=> ReadDA(ref _baseCtx).GetStricted<TStrictedEntity>();

			protected override void Dispose(bool explicitDispose) {
				_referenceKeyProvider = null;
				_baseCtx = null;
				//
				base.Dispose(explicitDispose);
			}

		}

		[DebuggerDisplay("{ToString(),nq}")]
		sealed class P_StorageOpCtx
			:P_StorageOpCtxBase, IPersistenceStorageOperationContext<IPersistenceEntity> {

			Dictionary<PersistenceEntityReferenceKeyTypeDescriptor, P_StorageOpCtxBase> _strictedCtxs;

			PrimitiveSpinLock _strictedCtxsSpinLock;

			internal P_StorageOpCtx(PersistenceEfCoreDataContext<TEfDbContext> dataCtx, TEfDbContext efCtx)
				: base(dataCtx: dataCtx, efCtx: efCtx) {
				_strictedCtxs = new Dictionary<PersistenceEntityReferenceKeyTypeDescriptor, P_StorageOpCtxBase>();
				_strictedCtxsSpinLock = new PrimitiveSpinLock();
			}

			IPersistenceDataContext IPersistenceStorageOperationContext<IPersistenceEntity>.DataContext
				=> DataContext;

			P_StorageOpCtxBase P_CreateStrictedCtx(PersistenceEntityReferenceKeyTypeDescriptor key)
				=>
				ActivationUtilities
				.RequireConstructor<P_StorageOpCtx, PersistenceEntityReferenceKeyTypeDescriptor, P_StorageOpCtxBase>(
					concreteType: typeof(P_StrictedStorageOpCtx<,>).MakeGenericType(typeArguments: new Type[ ] { typeof(TEfDbContext), key.EntityType, key.ReferenceKeyType }))
				(arg1: this, arg2: key);

			P_StorageOpCtxBase P_GetStrictedCtx(PersistenceEntityReferenceKeyTypeDescriptor key)
				=>
				ReadDA(ref _strictedCtxs)
				.GetOrAdd(
					key: key,
					factory: P_CreateStrictedCtx,
					unclaimedValue: (locKey, locValue) => locValue?.Dispose(),
					dictionaryOwner: this,
					spinLock: ReadDA(ref _strictedCtxsSpinLock, considerDisposeRequest: true));

			P_StorageOpCtxBase P_GetStrictedCtx(IPersistenceEntity entity) {
				if (entity is null)
					throw new ArgumentNullException(paramName: nameof(entity));
				//
				return P_GetStrictedCtx(key: new PersistenceEntityReferenceKeyTypeDescriptor(referenceKeyType: entity.ReferenceKeyType, entityType: entity.GetType()));
			}

			public async Task WriteAsync(IPersistenceEntity entity, IContext ctx = default)
				=> await P_GetStrictedCtx(entity: entity).WriteBaseTypeEntityAsync(entity: entity, ctx: ctx);

			// TODO: Put strings into the resources.
			//
			public async Task WriteAsync(IPersistenceEntity[ ] entityCollection, IContext ctx = default) {
				await Task.CompletedTask;
				//
				throw new NotSupportedException(message: $"This operation is not supported by this instance of context. To operate with the entity collection it is required to use another instance returned by the method '{nameof(IPersistenceStorageOperationContext<IPersistenceEntity>)}.{nameof(IPersistenceStorageOperationContext<IPersistenceEntity>.GetStricted)}'.");
			}

			public async Task InsertAsync(IPersistenceEntity entity, bool newReferenceKeyAssignmentAllowed = default, IContext ctx = default)
				=> await P_GetStrictedCtx(entity: entity).InsertBaseTypeEntityAsync(entity: entity, newReferenceKeyAssignmentAllowed: newReferenceKeyAssignmentAllowed, ctx: ctx);

			// TODO: Put strings into the resources.
			//
			public async Task InsertAsync(IPersistenceEntity[ ] entityCollection, bool newReferenceKeyAssignmentAllowed = default, IContext ctx = default) {
				await Task.CompletedTask;
				//
				throw new NotSupportedException(message: $"This operation is not supported by this instance of context. To operate with the entity collection it is required to use another instance returned by the method '{nameof(IPersistenceStorageOperationContext<IPersistenceEntity>)}.{nameof(IPersistenceStorageOperationContext<IPersistenceEntity>.GetStricted)}'.");
			}


			public async Task DeleteAsync(IPersistenceEntity entity, IContext ctx = default)
				=> await P_GetStrictedCtx(entity: entity).DeleteBaseTypeEntityAsync(entity: entity, ctx: ctx);

			// TODO: Put strings into the resources.
			//
			public async Task DeleteAsync(IPersistenceEntity[ ] entityCollection, IContext ctx = default) {
				await Task.CompletedTask;
				//
				throw new NotSupportedException(message: $"This operation is not supported by this instance of context. To operate with the entity collection it is required to use another instance returned by the method '{nameof(IPersistenceStorageOperationContext<IPersistenceEntity>)}.{nameof(IPersistenceStorageOperationContext<IPersistenceEntity>.GetStricted)}'.");
			}

			public async Task UpdateAsync(IPersistenceEntity entity, IContext ctx = default)
				=> await P_GetStrictedCtx(entity: entity).UpdateBaseTypeEntityAsync(entity: entity, ctx: ctx);

			// TODO: Put strings into the resources.
			//
			public async Task UpdateAsync(IPersistenceEntity[ ] entityCollection, IContext ctx = default) {
				await Task.CompletedTask;
				//
				throw new NotSupportedException(message: $"This operation is not supported by this instance of context. To operate with the entity collection it is required to use another instance returned by the method '{nameof(IPersistenceStorageOperationContext<IPersistenceEntity>)}.{nameof(IPersistenceStorageOperationContext<IPersistenceEntity>.GetStricted)}'.");
			}

			public override async Task WriteBaseTypeEntityAsync(IPersistenceEntity entity, IContext ctx = default)
				=> await P_GetStrictedCtx(entity: entity).WriteBaseTypeEntityAsync(entity: entity, ctx: ctx);

			public override async Task InsertBaseTypeEntityAsync(IPersistenceEntity entity, bool newReferenceKeyAssignmentAllowed = default, IContext ctx = default)
				=> await P_GetStrictedCtx(entity: entity).InsertBaseTypeEntityAsync(entity: entity, newReferenceKeyAssignmentAllowed: newReferenceKeyAssignmentAllowed, ctx: ctx);

			public override async Task DeleteBaseTypeEntityAsync(IPersistenceEntity entity, IContext ctx = default)
				=> await P_GetStrictedCtx(entity: entity).DeleteBaseTypeEntityAsync(entity: entity, ctx: ctx);

			public override async Task UpdateBaseTypeEntityAsync(IPersistenceEntity entity, IContext ctx = default)
				=> await P_GetStrictedCtx(entity: entity).UpdateBaseTypeEntityAsync(entity: entity, ctx: ctx);

			public IPersistenceStorageOperationContext<TStrictedEntity> GetStricted<TStrictedEntity>()
				where TStrictedEntity : class, IPersistenceEntity {
				var key = PersistenceUtilities.GetReferenceKeyTypeDescriptor(entityType: typeof(TStrictedEntity).Arg(nameof(TStrictedEntity)));
				return (IPersistenceStorageOperationContext<TStrictedEntity>)P_GetStrictedCtx(key: key);
			}

			protected override void Dispose(bool explicitDispose) {
				if (explicitDispose) {
					_strictedCtxsSpinLock?.EnterAndExitLock();
					_strictedCtxs?.Values.Observe(action: locValue => locValue?.Dispose());
					_strictedCtxs?.Clear();
				}
				_strictedCtxs = null;
				_strictedCtxsSpinLock = null;
				//
				base.Dispose(explicitDispose);
			}

		}

		#endregion

	}

}