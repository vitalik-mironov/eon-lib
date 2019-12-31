using System;

using Eon.ComponentModel.Dependencies;
using Eon.Data.Querying;

namespace Eon.Data {

	/// <summary>
	/// Контекст данных.
	/// <para>Является поставщиком функциональных зависимостей (<seealso cref="IDependencySupport"/>).</para>
	/// <para>Является обработчиком функциональных зависимостей (см. <seealso cref="IDependencyHandler"/>).</para>
	/// </summary>
	public interface IDataContext
		:IDisposable, IDependencySupport, IDependencyHandler2 {

		/// <summary>
		/// Возвращает контекст запросов к данным, ограниченный типом <typeparamref name="TQueryingContext"/>.
		/// <para>Если требуемый контекст не может быть возвращен, генерируется исключение.</para>
		/// </summary>
		/// <typeparam name="TQueryingContext">Тип-ограничение контекста.</typeparam>
		/// <returns>Объект <typeparamref name="TQueryingContext"/>.</returns>
		TQueryingContext GetQueryingContext<TQueryingContext>()
			where TQueryingContext : class, IDataQueryingContext;

		/// <summary>
		/// Возвращает контекст запросов к данным.
		/// </summary>
		/// <returns>Объект <see cref="IDataQueryingContext"/>.</returns>
		IDataQueryingContext GetQueryingContext();

	}

}