using System;

namespace Eon.Linq {

	/// <summary>
	/// Определяет функциональность задания исполнения запроса.
	/// </summary>
	public interface IQueryExecutionJob {

		/// <summary>
		/// Признак, указывающий на то, что запрос, представленный данным заданием, является запросом продолжения.
		/// </summary>
		/// <returns>True — Запрос, представленный данным заданием, является запросом продолжения.
		/// <para>False — Запрос, представленный данным заданием, не является запросом продолжения.</para></returns>
		bool IsContinuationQuery { get; }

		/// <summary>
		/// Тип элемента результата запроса.
		/// </summary>
		/// <returns>Тип элемента результата запроса.</returns>
		Type ElementType { get; }

	}

}