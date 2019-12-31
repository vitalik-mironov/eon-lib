using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Eon.Threading.Tasks;

namespace Eon.Linq {

	public class QueryExecutionJob<TResult>
		:Disposable, IQueryExecutionJob<TResult> {

		static readonly Type __ElementType = typeof(TResult);

		readonly bool _isContinuationQuery;

		IQueryable<TResult> _query;

		public QueryExecutionJob()
			: this(continuationQuery: false) { }

		public QueryExecutionJob(bool continuationQuery)
			: this(query: QueryableUtilities.GetEmptyQueryable<TResult>(), continuationQuery: continuationQuery) { }

		public QueryExecutionJob(IQueryable<TResult> query)
			: this(query: query, continuationQuery: false) { }

		public QueryExecutionJob(IQueryable<TResult> query, bool continuationQuery) {
			query.EnsureNotNull(nameof(query));
			//
			_query = query;
			_isContinuationQuery = continuationQuery;
		}

		public Type ElementType 
			=> __ElementType;

		public bool IsContinuationQuery 
			=> _isContinuationQuery;

		public virtual IQueryExecutionJobResult<TResult> Execute() {
			var query = ReadDA(ref _query);
			if (query.IsEmptyQueryable())
				return new QueryExecutionJobResult<TResult>(this, new TResult[ ] { });
			else
				return new QueryExecutionJobResult<TResult>(this, query.ToArray());
		}

		public virtual ITaskWrap<IQueryExecutionJobResult<TResult>> ExecuteAsync(CancellationToken ct, TaskCreationOptions taskCreationOptions) {
			var query = ReadDA(ref _query);
			if (query.IsEmptyQueryable())
				return EmptyQueryExecutionJob<TResult>.Singleton.ExecuteAsync(ct, taskCreationOptions);
			else
				return
					new TaskWrap<IQueryExecutionJobResult<TResult>>(
						task:
							TaskUtilities
							.RunOnDefaultScheduler<IQueryExecutionJobResult<TResult>>(
								function: () => new QueryExecutionJobResult<TResult>(this, query.ToArray()),
								cancellationToken: ct,
								options: taskCreationOptions));
		}

		protected override void Dispose(bool explicitDispose) {
			_query = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}