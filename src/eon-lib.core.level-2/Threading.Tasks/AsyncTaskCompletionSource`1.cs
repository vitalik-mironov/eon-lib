using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eon.Threading.Tasks {

	public class AsyncTaskCompletionSource<T> {

		#region Static & constant members

		const int __TrueFlag = 1;

		const int __FalseFlag = 0;

		#endregion

		readonly TaskCreationOptions _options;

		protected internal readonly TaskCompletionSource<T> InnerTaskCompletionSource;

		int _completionSetFlag;

		readonly object _tag;

		public AsyncTaskCompletionSource(object tag = default)
			: this(options: TaskCreationOptions.None, tag: tag) { }

		public AsyncTaskCompletionSource(TaskCreationOptions options, object tag = default) {
			_options = options;
			InnerTaskCompletionSource = new TaskCompletionSource<T>(creationOptions: _options & ~TaskCreationOptions.PreferFairness);
			_completionSetFlag = __FalseFlag;
			_tag = tag;
		}

		public AsyncTaskCompletionSource(T result, object tag = default) {
			_options = TaskCreationOptions.None;
			InnerTaskCompletionSource = new TaskCompletionSource<T>(creationOptions: _options);
			InnerTaskCompletionSource.SetResult(result: result);
			_completionSetFlag = __TrueFlag;
			_tag = tag;
		}

		public AsyncTaskCompletionSource(Task<T> fromTask, object tag = default) {
			fromTask.EnsureNotNull(nameof(fromTask));
			//
			_options = TaskCreationOptions.None;
			InnerTaskCompletionSource = new TaskCompletionSource<T>(creationOptions: _options);
			_completionSetFlag = __FalseFlag;
			_tag = tag;
			//
			fromTask
				.ContinueWith(
					continuationAction:
						locTask => {
							if (locTask.IsCanceled)
								TrySetCanceled(ct: CancellationToken.None);
							else if (locTask.IsFaulted) {
								if (!TrySetException(exception: locTask.Exception))
									throw locTask.Exception.Flatten();
							}
							else
								TrySetResult(result: locTask.Result);
						},
					continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
		}

		public TaskCreationOptions Options
			=> _options;

		public Task<T> Task
			=> InnerTaskCompletionSource.Task;

		public object Tag
			=> _tag;

		public bool TrySetResult(T result) {
			if (Interlocked.CompareExchange(ref _completionSetFlag, __TrueFlag, __FalseFlag) == __FalseFlag) {
				DoSetResult(result: result);
				return true;
			}
			else
				return false;
		}

		protected virtual void DoSetResult(T result)
			=> TaskUtilities.RunOnDefaultScheduler(action: () => InnerTaskCompletionSource.SetResult(result: result));

		public bool TrySetCanceled(CancellationToken ct, bool synchronously = default) {
			if (Interlocked.CompareExchange(ref _completionSetFlag, __TrueFlag, __FalseFlag) == __FalseFlag) {
				DoSetCanceled(ct: ct, synchronously: synchronously);
				return true;
			}
			else
				return false;
		}

		public bool TrySetCanceled(bool synchronously = default)
			=> TrySetCanceled(ct: CancellationToken.None, synchronously: synchronously);

		protected virtual void DoSetCanceled(CancellationToken ct, bool synchronously = default) {
			if (synchronously)
				InnerTaskCompletionSource.TrySetCanceled(cancellationToken: ct);
			else
				TaskUtilities.RunOnDefaultScheduler(action: () => InnerTaskCompletionSource.TrySetCanceled(cancellationToken: ct));
		}

		public bool TrySetException(Exception exception)
			=> TrySetException(exception, synchronously: default);

		public bool TrySetException(Exception exception, bool synchronously) {
			exception.EnsureNotNull(nameof(exception));
			//
			if (Interlocked.CompareExchange(ref _completionSetFlag, __TrueFlag, __FalseFlag) == __FalseFlag) {
				DoSetException(exception: exception, synchronously: synchronously);
				return true;
			}
			else
				return false;
		}

		protected virtual void DoSetException(Exception exception, bool synchronously = default) {
			if (synchronously)
				InnerTaskCompletionSource.SetException(exception: exception);
			else
				TaskUtilities.RunOnDefaultScheduler(action: () => InnerTaskCompletionSource.SetException(exception: exception));

		}

	}

}