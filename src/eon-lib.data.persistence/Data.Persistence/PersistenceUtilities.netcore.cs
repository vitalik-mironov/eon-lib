#if TRG_NETCORE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DigitalFlare.ComponentModel.Properties;
using DigitalFlare.Context;
using DigitalFlare.Threading;
using Microsoft.EntityFrameworkCore;
using static DigitalFlare.Transactions.TransactionUtilities;

namespace DigitalFlare.Data.Persistence {

	public static partial class PersistenceUtilities {

		/// <summary>
		/// Выполняет копирование исходных сущностей данных <paramref name="source"/> в хранилище (БД).
		/// <para>Если сущность в БД отсутствует, то она создается в БД; если присутствует — обновляется.</para>
		/// </summary>
		/// <typeparam name="TEntity">Тип сущности.</typeparam>
		/// <param name="source">
		/// Исходный набор сущностей.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="factory">
		/// Фабрика нового экземпляра сущности.
		/// <para>Когда сущность в БД отсутствует, то создается новый экземпляр сущности, используя указанную фабрику.</para>
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="storeOpCtx">
		/// Контекст модификации данных.
		/// <para>Используется для записи новой/существующей сущности в хранилище данных (БД).</para>
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="copier">
		/// Компонент, поставлющий необходимую функциональность копирования свойств из одного экземпляра в другой.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="ctx">
		/// Контекст выполнения операции.
		/// </param>
		/// <returns>Объект <see cref="Task{TResult}"/>.</returns>
		public static async Task<DataChangeStats> CopyEntitiesAsync<TEntity>(IEnumerable<TEntity> source, Func<TEntity, TEntity> factory, IPersistenceStoreOperationContext<IPersistenceEntity> storeOpCtx, IPropertyValueCopier copier, IContext ctx = default)
			where TEntity : PersistenceEntityBase<long> {
			//
			source.EnsureNotNull(nameof(source));
			factory.EnsureNotNull(nameof(factory));
			storeOpCtx.EnsureNotNull(nameof(storeOpCtx));
			copier.EnsureNotNull(nameof(copier));
			//
			var queryingCtx = storeOpCtx.DataContext.GetQueryingContext();
			var searchExistingBaseQuery = queryingCtx.CreateQuery<TEntity>();
			var ct = ctx?.Ct() ?? default;
			var insertedCounter = 0;
			var updatedCounter = 0;
			DataChangeStats result;
			using (var storeTx = RequireTxSnapshot()) {
				foreach (var sourceEntity in source) {
					var entity = await searchExistingBaseQuery.Where(i => i.ReferenceKey == sourceEntity.ReferenceKey).Take(2).SingleOrDefaultAsync(ct).ConfigureAwait(false);
					if (entity is null) {
						entity = factory(sourceEntity);
						copier.CopyValuesOf(from: sourceEntity, to: ref entity, ctx: ctx);
						await storeOpCtx.InsertAsync(entity: entity, ctx: ctx).ConfigureAwait(false);
						insertedCounter++;
					}
					else if (copier.CopyValuesOf(from: sourceEntity, to: ref entity, ctx: ctx) > 0) {
						await storeOpCtx.WriteAsync(entity: entity, ctx: ctx).ConfigureAwait(false);
						updatedCounter++;
					}
				}
				//
				result = new DataChangeStats(inserted: insertedCounter, updated: updatedCounter);
				//
				ct.ThrowExceptionIfCancellationRequested();
				storeTx.Complete();
			}
			//
			return result;
		}

	}

}
#endif