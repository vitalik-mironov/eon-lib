using System;
using System.Threading.Tasks;

using Eon.ComponentModel.Dependencies;
using Eon.Context;

namespace Eon.Data.Persistence {

	/// <summary>
	/// Defines the persistent data storage change context.
	/// </summary>
	public interface IPersistenceStorageOperationContext<TEntity>
		:IDisposable, IDependencySupport
		where TEntity : class, IPersistenceEntity {

		/// <summary>
		/// Возвращает контекст данных, которому принадлежит даннаый контекст.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </summary>
		IPersistenceDataContext DataContext { get; }

		/// <summary>
		/// Выполняет операцию записи (вставки или обновления) сущности <paramref name="entity"/>.
		/// </summary>
		/// <param name="entity">
		/// Сущность <typeparamref name="TEntity"/>.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="ctx">
		/// Контекст выполнения операции.
		/// </param>
		Task WriteAsync(TEntity entity, IContext ctx = default);

		Task WriteAsync(TEntity[ ] entityCollection, IContext ctx = default);

		/// <summary>
		/// Выполняет операцию вставки сущности <paramref name="entity"/>.
		/// </summary>
		/// <param name="entity">
		/// Сущность <typeparamref name="TEntity"/>.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="newReferenceKeyAssignmentAllowed">
		/// Признак, указывающий допустимость присвоения ссылочного ключа сущности перед выполнением операции вставки.
		/// <para>Ссылочный ключ будет присвоен, если указанная сущность <paramref name="entity"/> является новой (см. <see cref="IPersistenceEntity.IsNew"/>).</para>
		/// </param>
		/// <param name="ctx">
		/// Контекст выполнения операции.
		/// </param>
		Task InsertAsync(TEntity entity, bool newReferenceKeyAssignmentAllowed = default, IContext ctx = default);

		Task InsertAsync(TEntity[ ] entityCollection, bool newReferenceKeyAssignmentAllowed = default, IContext ctx = default);

		/// <summary>
		/// Выполняет операцию удаления сущности <paramref name="entity"/>.
		/// </summary>
		/// <param name="entity">
		/// Сущность <typeparamref name="TEntity"/>.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="ctx">
		/// Контекст выполнения операции.
		/// </param>
		Task DeleteAsync(TEntity entity, IContext ctx = default);

		Task DeleteAsync(TEntity[ ] entityCollection, IContext ctx = default);

		/// <summary>
		/// Выполняет операцию обновления сущности <paramref name="entity"/>.
		/// </summary>
		/// <param name="entity">
		/// Сущность <typeparamref name="TEntity"/>.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="ctx">
		/// Контекст выполнения операции.
		/// </param>
		Task UpdateAsync(TEntity entity, IContext ctx = default);

		Task UpdateAsync(TEntity[ ] entityCollection, IContext ctx = default);

		IPersistenceStorageOperationContext<TStrictedEntity> GetStricted<TStrictedEntity>()
			where TStrictedEntity : class, IPersistenceEntity;

	}

}