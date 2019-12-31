using System;
using System.Collections.Generic;

namespace Eon.Linq {

	public interface IQueryExecutionJobResult<out TResult>
		:IDisposable {

		IQueryExecutionJob<TResult> ContinuationQuery { get; }

		IQueryExecutionJob<TResult> Query { get; }

		IEnumerable<TResult> ResultData { get; }

	}

}