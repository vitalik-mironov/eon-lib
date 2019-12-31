using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Eon.Collections;
using Eon.Threading.Tasks;

namespace Eon.Linq {

	public static class QueryExecutionJobUtilities {

		/// <summary>
		/// Выполняет задание исполнения запроса до тех пор, пока набор данных, соответствующий запросу, не будет получен полностью.
		/// <para>Данный метод выполняет все запросы продолжения, которые могут быть получены в ходе выполнения указанного задания исполнения запроса.</para>
		/// </summary>
		/// <typeparam name="TResult">Тип элемента результата запроса.</typeparam>
		/// <param name="job">Задание исполнения запроса.</param>
		/// <param name="antecedentTask">Задание-предшественник.</param>
		/// <param name="ct">Токен отмены.</param>
		/// <param name="options">Опции задания продолжения.</param>
		/// <returns>Задание исполнения запроса.</returns>
		public static Task<IReadOnlyList<TResult>> ExecuteFullAsTaskContinuation<TResult>(this IQueryExecutionJob<TResult> job, Task antecedentTask, CancellationToken ct, TaskContinuationOptions options) {
			antecedentTask.EnsureNotNull(nameof(antecedentTask));
			var resultTaskProxy = new TaskCompletionSource<IReadOnlyList<TResult>>((options & TaskContinuationOptions.AttachedToParent) == TaskContinuationOptions.AttachedToParent ? TaskCreationOptions.AttachedToParent : TaskCreationOptions.None);
			if (ct.CanBeCanceled)
				ct.Register(callback: () => resultTaskProxy.TrySetCanceled());
			var entryTask =
				antecedentTask
				.ContinueWith(
					continuationAction: a0 => P_ExecuteFullTaskWork(resultTaskProxy, job, ct),
					cancellationToken: ct,
					continuationOptions: options,
					scheduler: TaskScheduler.Current ?? TaskScheduler.Default);
			entryTask
				.ContinueWith(
					continuationAction:
						antecedentTask01 => {
							if (antecedentTask01.IsCanceled)
								resultTaskProxy.TrySetCanceled();
						},
					cancellationToken: CancellationToken.None,
					continuationOptions: TaskContinuationOptions.ExecuteSynchronously,
					scheduler: TaskScheduler.Current ?? TaskScheduler.Default);
			return resultTaskProxy.Task;
		}

		/// <summary>
		/// Выполняет задание исполнения запроса до тех пор, пока набор данных, соответствующий запросу, не будет получен полностью.
		/// <para>Данный метод выполняет все запросы продолжения, которые могут быть получены в ходе выполнения указанного задания исполнения запроса.</para>
		/// </summary>
		/// <typeparam name="TResult">Тип элемента результата запроса.</typeparam>
		/// <param name="job">Задание исполнения запроса.</param>
		/// <param name="ct">Токен отмены.</param>
		/// <param name="options">Опции создания задания.</param>
		/// <returns>Задание исполнения запроса.</returns>
		public static Task<IReadOnlyList<TResult>> ExecuteFullAsync<TResult>(this IQueryExecutionJob<TResult> job, CancellationToken ct, TaskCreationOptions options) {
			var resultTaskProxy = new TaskCompletionSource<IReadOnlyList<TResult>>(options & ~TaskCreationOptions.PreferFairness);
			if (ct.CanBeCanceled)
				ct.Register(() => resultTaskProxy.TrySetCanceled());
			var entryTask =
				TaskUtilities
				.RunOnDefaultScheduler(
					action: () => P_ExecuteFullTaskWork(resultTaskProxy, job, ct),
					ct: ct,
					options: options);
			entryTask
				.ContinueWith(
					continuationAction:
						antecedentTask01 => {
							if (antecedentTask01.IsCanceled)
								resultTaskProxy.TrySetCanceled();
						},
					cancellationToken: CancellationToken.None,
					continuationOptions: TaskContinuationOptions.ExecuteSynchronously,
					scheduler: TaskScheduler.Current ?? TaskScheduler.Default);
			return resultTaskProxy.Task;
		}

		public static Task<IReadOnlyList<TResult>> ExecuteFullAsync<TResult>(this IQueryExecutionJob<TResult> job, TaskCreationOptions options)
			=> ExecuteFullAsync(job: job, ct: CancellationToken.None, options: options);

		public static Task<IReadOnlyList<TResult>> ExecuteFullAsync<TResult>(this IQueryExecutionJob<TResult> job)
			=> ExecuteFullAsync(job: job, ct: CancellationToken.None, options: TaskCreationOptions.None);

		static void P_ExecuteFullTaskWork<TResult>(TaskCompletionSource<IReadOnlyList<TResult>> proxy, IQueryExecutionJob<TResult> job, CancellationToken ct) {
			proxy.EnsureNotNull(nameof(proxy));
			job.EnsureNotNull(nameof(job));
			//
			try {
				var resultBuffer = new List<TResult>();
				Task<IQueryExecutionJobResult<TResult>> execAsync(IQueryExecutionJob<TResult> query)
					=> (Task<IQueryExecutionJobResult<TResult>>)query.ExecuteAsync(cancellationToken: ct, taskCreationOptions: TaskCreationOptions.None).Task;
				void continuationWork(Task<IQueryExecutionJobResult<TResult>> locQueryExecTask, Action<Task<IQueryExecutionJobResult<TResult>>> locContinuationWorkRedirector) {
					try {
						if (locQueryExecTask.IsCanceled) {
							proxy.TrySetCanceled();
							return;
						}
						else if (locQueryExecTask.IsFaulted) {
							proxy.TrySetException(exception: locQueryExecTask.Exception);
							return;
						}
						IQueryExecutionJobResult<TResult> queryExecTaskResult;
						try {
							queryExecTaskResult = locQueryExecTask.Result;
						}
						catch (Exception exception) {
							if (exception is OperationCanceledException)
								proxy.TrySetCanceled();
							else
								proxy.TrySetException(exception);
							return;
						}
						foreach (var queryResultElement in queryExecTaskResult.ResultData)
							resultBuffer.Add(queryResultElement);
						var continuationQuery = queryExecTaskResult.ContinuationQuery;
						if (continuationQuery is null)
							proxy.TrySetResult(result: new ListReadOnlyWrap<TResult>(list: resultBuffer));
						else if (ct.IsCancellationRequested)
							proxy.TrySetCanceled();
						else
							execAsync(continuationQuery)
								.ContinueWith(
									continuationAction: locContinuationWorkRedirector,
									cancellationToken: ct,
									continuationOptions: TaskContinuationOptions.PreferFairness | TaskContinuationOptions.AttachedToParent,
									scheduler: TaskScheduler.Current ?? TaskScheduler.Default);
					}
					catch (Exception exception) {
						proxy.TrySetException(exception);
					}
				}
				void continuationWorkRedirector(Task<IQueryExecutionJobResult<TResult>> locTsk)
					=> continuationWork(locQueryExecTask: locTsk, locContinuationWorkRedirector: (global::System.Action<global::System.Threading.Tasks.Task<global::Eon.Linq.IQueryExecutionJobResult<TResult>>>)continuationWorkRedirector);
				execAsync(query: job)
					.ContinueWith(
						continuationAction: locTsk => continuationWork(locQueryExecTask: locTsk, locContinuationWorkRedirector: (global::System.Action<global::System.Threading.Tasks.Task<global::Eon.Linq.IQueryExecutionJobResult<TResult>>>)continuationWorkRedirector),
						cancellationToken: ct,
						continuationOptions: TaskContinuationOptions.PreferFairness | TaskContinuationOptions.AttachedToParent,
						scheduler: TaskScheduler.Current ?? TaskScheduler.Default);
			}
			catch (Exception exception) {
				proxy.TrySetException(exception);
			}
		}

		public static IReadOnlyList<TResult> ExecuteFull<TResult>(this IQueryExecutionJob<TResult> job) {
			job.EnsureNotNull(nameof(job));
			//
			var resultBuffer = new List<TResult>();
			var continuationJob = job;
			for (; !(continuationJob is null);) {
				var jobResult = continuationJob.Execute();
				foreach (var jobResultElement in jobResult.ResultData)
					resultBuffer.Add(jobResultElement);
				continuationJob = jobResult.ContinuationQuery;
			}
			return new ListReadOnlyWrap<TResult>(list: resultBuffer);
		}

	}

}