using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Eon.Threading.Tasks.Internal;

namespace Eon.Threading.Tasks {

	public sealed class TaskWrap<TResult>
		:ITaskWrap<TResult> {

		#region Static members

		static readonly Type __ResultType = typeof(TResult);

		public static TaskWrap<TResult> Wrap(Exception exception)
			=> new TaskWrap<TResult>(task: Task.FromException<TResult>(exception: exception));

		public static TaskWrap<TResult> Wrap(Task<TResult> task)
			=> new TaskWrap<TResult>(task: task);

		public static TaskWrap<TResult> Wrap(Func<TResult> func) {
			if (func is null)
				throw new ArgumentNullException(paramName: nameof(func));
			//
			try {
				return Wrap(task: Task.FromResult(result: func()));
			}
			catch (Exception exception) {
				return Wrap(exception: exception);
			}
		}

		public static TaskWrap<TResult> Wrap<TArg>(TArg arg, Func<TArg, TResult> func) {
			if (func is null)
				throw new ArgumentNullException(paramName: nameof(func));
			//
			try {
				return Wrap(task: Task.FromResult(result: func(arg: arg)));
			}
			catch (Exception exception) {
				return Wrap(exception: exception);
			}
		}

		public static TaskWrap<TResult> Wrap(TResult result)
			=> Wrap(task: Task.FromResult(result: result));

		// TODO: Put strings into the resources.
		//
		public static TaskWrap<TResult> Wrap(Func<Task<TResult>> factory) {
			factory.EnsureNotNull(nameof(factory));
			//
			try {
				var task = factory();
				if (task is null)
					throw new EonException(message: $"Specified factory has not created a task.{Environment.NewLine}\tFactory:{factory.FmtStr().GNLI2()}");
				else
					return new TaskWrap<TResult>(task: task);
			}
			catch (Exception exception) {
				return new TaskWrap<TResult>(task: Task.FromException<TResult>(exception: exception));
			}
		}

		#endregion

		readonly Task<TResult> _innerTask;

		readonly Dictionary<Type, Task> _convertedTasks;

		readonly PrimitiveSpinLock _convertedTasksSpinLock;

		public TaskWrap(Task<TResult> task) {
			task.EnsureNotNull(nameof(task));
			//
			_innerTask = task;
			_convertedTasks = new Dictionary<Type, Task>(capacity: 4);
			_convertedTasksSpinLock = new PrimitiveSpinLock(name: $"{nameof(TaskWrap<TResult>)}[{__ResultType.FullName}]");
		}

		public Task Task
			=> _innerTask;


		public TResult GetResult()
			=> _innerTask.Result;

		public TaskAwaiter<TResult> GetAwaiter()
			=> _innerTask.GetAwaiter();

		public Task<TToResult> ConvertTask<TToResult>() {
			var toResultType = typeof(TToResult);
			if (__ResultType == toResultType)
				return (Task<TToResult>)(Task)_innerTask;
			else {
				Task converted = default;
				if (!_convertedTasksSpinLock.Invoke(() => _convertedTasks.TryGetValue(key: toResultType, value: out converted))) {
					var taskProxy = new TaskCompletionSource<TToResult>(creationOptions: TaskCreationOptions.None);
					var registeredTask =
						_convertedTasksSpinLock
						.Invoke(
							func:
								() => {
									try {
										_convertedTasks.Add(key: toResultType, value: taskProxy.Task);
										return taskProxy.Task;
									}
									catch (ArgumentException) {
										return _convertedTasks[ key: toResultType ];
									}
								});
					if (ReferenceEquals(taskProxy.Task, registeredTask)) {
						_innerTask
							.ContinueWith(
								continuationAction:
									locInnerTask => {
										if (locInnerTask.IsCanceled)
											taskProxy.SetCanceled();
										else if (locInnerTask.IsFaulted)
											TaskUtilitiesInternalCoreL1.SetException(task: locInnerTask, setter: locException => taskProxy.SetException(locException));
										else {
											try {
												taskProxy.SetResult(result: locInnerTask.Result.Cast<TResult, TToResult>());
											}
											catch (Exception locException) {
												taskProxy.SetException(exception: locException);
											}
										}
									},
								continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
					}
					else
						taskProxy.TrySetCanceled();
					converted = registeredTask;
				}
				return (Task<TToResult>)converted;
			}
		}

	}

}