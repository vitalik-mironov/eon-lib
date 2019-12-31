using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Eon.Threading.Tasks;

namespace Eon.Linq {

	public sealed class EmptyQueryExecutionJob<TResult>
		:IQueryExecutionJob<TResult> {

		public static readonly EmptyQueryExecutionJob<TResult> Singleton = new EmptyQueryExecutionJob<TResult>();

		#region Nested types

		//[DebuggerStepThrough]
		//[DebuggerNonUserCode]
		sealed class P_EmptyDataQueryResult
			:IQueryExecutionJobResult<TResult> {

			readonly EmptyQueryExecutionJob<TResult> _query;

			internal P_EmptyDataQueryResult(EmptyQueryExecutionJob<TResult> query) {
				if (query == null)
					throw new ArgumentNullException("query");
				_query = query;
			}

			IQueryExecutionJob<TResult> IQueryExecutionJobResult<TResult>.ContinuationQuery { get { return null; } }

			IQueryExecutionJob<TResult> IQueryExecutionJobResult<TResult>.Query { get { return _query; } }

			IEnumerable<TResult> IQueryExecutionJobResult<TResult>.ResultData { get { return Enumerable.Empty<TResult>(); } }

			void IDisposable.Dispose() { }

		}

		#endregion

		readonly IQueryExecutionJobResult<TResult> _queryResult;

		EmptyQueryExecutionJob() {
			_queryResult = new P_EmptyDataQueryResult(this);
		}

		public ITaskWrap<IQueryExecutionJobResult<TResult>> ExecuteAsync(CancellationToken ct, TaskCreationOptions taskCreationOptions) {
			var taskCompletionSource = new TaskCompletionSource<IQueryExecutionJobResult<TResult>>(taskCreationOptions & ~TaskCreationOptions.PreferFairness);
			if (ct.IsCancellationRequested)
				taskCompletionSource.SetCanceled();
			else
				taskCompletionSource.SetResult(_queryResult);
			return new TaskWrap<IQueryExecutionJobResult<TResult>>(task: taskCompletionSource.Task);
		}

		public IQueryExecutionJobResult<TResult> Execute() => _queryResult;

		bool IQueryExecutionJob.IsContinuationQuery => false;

		Type IQueryExecutionJob.ElementType => typeof(TResult);

	}

}