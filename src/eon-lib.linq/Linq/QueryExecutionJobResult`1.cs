using System.Collections.Generic;

namespace Eon.Linq {

	public class QueryExecutionJobResult<TResult>
		:Disposable, IQueryExecutionJobResult<TResult> {

		IQueryExecutionJob<TResult> _query;

		IEnumerable<TResult> _resultData;

		IQueryExecutionJob<TResult> _continuationQuery;

		public QueryExecutionJobResult(IQueryExecutionJob<TResult> query, IEnumerable<TResult> result)
			: this(query: query, result: result, continuationQuery: null) { }

		public QueryExecutionJobResult(IQueryExecutionJob<TResult> query, IEnumerable<TResult> result, IQueryExecutionJob<TResult> continuationQuery) {
			query.EnsureNotNull(nameof(query));
			result.EnsureNotNull(nameof(result));
			//
			_query = query;
			_resultData = result;
			_continuationQuery = continuationQuery;
		}

		public IQueryExecutionJob<TResult> ContinuationQuery 
			=> ReadDA(ref _continuationQuery);

		public IQueryExecutionJob<TResult> Query 
			=> ReadDA(ref _query);

		public IEnumerable<TResult> ResultData 
			=> ReadDA(ref _resultData);

		protected override void Dispose(bool explicitDispose) {
			_continuationQuery = null;
			_query = null;
			_resultData = null;
			//
			base.Dispose(explicitDispose);
		}

	}


}