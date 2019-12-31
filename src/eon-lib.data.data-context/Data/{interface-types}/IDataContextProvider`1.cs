using System;

using Eon.ComponentModel.Dependencies;
using Eon.Context;
using Eon.Threading.Tasks;

namespace Eon.Data {

	/// <summary>
	/// Поставщик контекста данных <see cref="IDataContext2"/>.
	/// <para>Код должен использовать именно этот тип для создания любого контекста данных, производного от <seealso cref="IDataContext2"/>.</para>
	/// </summary>
	/// <typeparam name="TContext">Ограничение типа контекста данных <see cref="IDataContext2"/>.</typeparam>
	public interface IDataContextProvider<out TContext>
		:IDependencySupport, IDisposable, IDependencyHandler2
		where TContext : class, IDataContext2 {

		/// <summary>
		/// Создает новый экземпляр контекста данных.
		/// </summary>
		/// <param name="ctx">
		/// Контекст выполнения операции.
		/// <para>В общем случае указанный контекст будет использован для мониторинга отмены операции, трансляции прогресса выполнения операции и т.д.</para>
		/// </param>
		ITaskWrap<TContext> CreateContextAsync(IContext ctx = default);

		/// <summary>
		/// Захватывает свободный экземпляр контекста данных из пула.
		/// <para>Если пул не поодерживается или отключен, то создает новый экземпляр контекста (см. <see cref="CreateContextAsync(IContext)"/>).</para>
		/// <para>Если в пуле отсутствует свободный экземпляр контекста, то также создается новый экземпляр контекста.</para>
		/// </summary>
		/// <param name="context">
		/// Контекст выполнения операции.
		/// <para>В общем случае указанный контекст будет использован для мониторинга отмены операции, трансляции прогресса выполнения операции и т.д.</para>
		/// </param>
		ITaskWrap<IUsing<TContext>> LeaseContextAsync(IContext context = default);

	}

}