using System;
using System.Threading.Tasks;

using Eon.Context;

namespace Eon.Data.TypeSystem {

	/// <summary>
	/// Построитель типа данных, входящего в какую-либо доменную модель приложения данных.
	/// </summary>
	public interface IDataTypeBuilder {

		/// <summary>
		/// Строит и возвращает тип данных.
		/// </summary>
		/// <param name="ctx">
		/// Контекст выполнения операции.
		/// </param>
		/// <returns>Объект задачи <seealso cref="Task{TResult}"/>.</returns>
		Task<Type> BuildTypeAsync(IContext ctx = default);

	}

}