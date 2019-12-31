using System;
using System.Threading;
using System.Threading.Tasks;
using Eon.Context;
using Eon.Diagnostics;
using Eon.Runtime.Options;
using Eon.Threading.Tasks.Internal;

namespace Eon.Threading.Tasks {

	public static class TaskUtilities {

		#region Nested types

		static class P_SucceededTaskProxy<TResult> {

			static readonly TaskCompletionSource<TResult> __Proxy;

			static P_SucceededTaskProxy() {
				__Proxy = new TaskCompletionSource<TResult>(creationOptions: TaskCreationOptions.None);
				__Proxy.SetResult(default);
			}

			public static Task<TResult> Awaitable()
				=> __Proxy.Task;

		}

		static class P_CanceledTaskProxy<TResult> {

			static readonly TaskCompletionSource<TResult> __Proxy;

			static P_CanceledTaskProxy() {
				__Proxy = new TaskCompletionSource<TResult>(TaskCreationOptions.None);
				__Proxy.SetCanceled();
			}

			public static Task<TResult> Awaitable()
				=> __Proxy.Task;

		}

		#endregion

		static readonly TaskCompletionSource<bool> __FalseResultTaskProxy;

		static readonly TaskCompletionSource<bool> __TrueResultTaskProxy;

		static TaskUtilities() {
			__FalseResultTaskProxy = new TaskCompletionSource<bool>(creationOptions: TaskCreationOptions.None);
			__FalseResultTaskProxy.SetResult(false);
			//
			__TrueResultTaskProxy = new TaskCompletionSource<bool>(creationOptions: TaskCreationOptions.None);
			__TrueResultTaskProxy.SetResult(true);
		}

		/// <summary>
		/// Gets the current async operaion wait timeout set by <see cref="AsyncOperationTimeoutOption"/> (see <see cref="RuntimeOptions"/>).
		/// </summary>
		public static int DefaultAsyncTimeoutMilliseconds
			=> DefaultAsyncTimeout.Milliseconds;

		/// <summary>
		/// Gets the current async operaion wait timeout set by <see cref="AsyncOperationTimeoutOption"/> (see <see cref="RuntimeOptions"/>).
		/// </summary>
		public static TimeoutDuration DefaultAsyncTimeout
			=> AsyncOperationTimeoutOption.Require();

		public static Task StartNewAttachedToParent(Action action)
			=> Task.Factory.StartNew(action: action.EnsureNotNull(nameof(action)), cancellationToken: CancellationToken.None, creationOptions: TaskCreationOptions.AttachedToParent | TaskCreationOptions.PreferFairness, scheduler: TaskScheduler.Current ?? TaskScheduler.Default);

		public static Task Delay(TimeoutDuration duration, CancellationToken cancellationToken, Task delayBreak = null)
			=> Delay(duration: duration, taskCreationOptions: TaskCreationOptions.None, ct: cancellationToken, cancellationBreak: false, delayBreak: delayBreak);

		public static Task Delay(TimeoutDuration duration, CancellationToken cancellationToken, bool cancellationBreak, Task delayBreak = null)
			=> Delay(duration: duration, taskCreationOptions: TaskCreationOptions.None, ct: cancellationToken, cancellationBreak: cancellationBreak, delayBreak: delayBreak);

		public static Task Delay(TimeoutDuration duration, TaskCreationOptions taskCreationOptions, Task delayBreak = null)
			=> Delay(duration: duration, taskCreationOptions: taskCreationOptions, ct: CancellationToken.None, cancellationBreak: false, delayBreak: delayBreak);

		public static Task Delay(TimeoutDuration duration, TaskCreationOptions taskCreationOptions, CancellationToken ct, bool cancellationBreak, Task delayBreak = null) {
			duration.EnsureNotNull(nameof(duration)).EnsureNotInfinite();
			//
			var taskProxy = default(TaskCompletionSource<int>);
			try {
				if (ct.IsCancellationRequested) {
					if (cancellationBreak)
						return FromVoidResult();
					else
						FromCanceled(ct: ct);
				}
				else if (duration.Milliseconds == 0 || delayBreak?.IsCompleted == true)
					return FromVoidResult();
				else {
					taskProxy = new TaskCompletionSource<int>((taskCreationOptions | TaskCreationOptions.RunContinuationsAsynchronously) & ~TaskCreationOptions.PreferFairness);
					if (!(delayBreak is null))
						delayBreak.ContinueWith(continuationAction: locTask => taskProxy.TrySetResult(0), continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
					Task
						.Delay(millisecondsDelay: duration.Milliseconds, cancellationToken: ct)
						.ContinueWith(
							continuationAction:
								locDelayTask => {
									if (locDelayTask.IsCanceled) {
										if (cancellationBreak)
											taskProxy.TrySetResult(0);
										else
											taskProxy.TrySetCanceled(cancellationToken: ct);
									}
									else if (locDelayTask.IsFaulted) {
										if (!taskProxy.TrySetException(locDelayTask.Exception))
											throw locDelayTask.Exception.Flatten();
									}
									else
										taskProxy.TrySetResult(0);
								},
							continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
				}
				return taskProxy.Task;
			}
			catch {
				taskProxy?.TrySetCanceled();
				throw;
			}
		}

		public static Task Delay(TimeoutDuration duration, Task delayBreak = null)
			=> Delay(duration: duration, taskCreationOptions: TaskCreationOptions.None, ct: CancellationToken.None, cancellationBreak: false, delayBreak: delayBreak);

		public static Task Delay(TimeSpan duration, Task delayBreak = null)
			=> Delay(duration: TimeoutDuration.FromTimeSpan(ts: duration.Arg(nameof(duration))), taskCreationOptions: TaskCreationOptions.None, ct: CancellationToken.None, cancellationBreak: false, delayBreak: delayBreak);

		public static Task Delay(TimeSpan duration, CancellationToken cancellationToken, Task delayBreak = null)
			=> Delay(duration: TimeoutDuration.FromTimeSpan(ts: duration.Arg(nameof(duration))), taskCreationOptions: TaskCreationOptions.None, ct: cancellationToken, cancellationBreak: false, delayBreak: delayBreak);

		public static Task Delay(TimeSpan duration, CancellationToken cancellationToken, bool cancellationBreak, Task delayBreak = null)
			=> Delay(duration: TimeoutDuration.FromTimeSpan(ts: duration.Arg(nameof(duration))), taskCreationOptions: TaskCreationOptions.None, ct: cancellationToken, cancellationBreak: cancellationBreak, delayBreak: delayBreak);

		public static Task<TResult> FromResult<TResult>(Func<TResult> getter) {
			getter.EnsureNotNull(nameof(getter));
			//
			TResult result;
			try {
				result = getter();
			}
			catch (OperationCanceledException operationCanceledException) {
				return FromCanceled<TResult>(ct: operationCanceledException.CancellationToken);
			}
			catch (Exception exception) {
				return FromError<TResult>(error: exception);
			}
			return FromResult(result: result);
		}

		public static Task<bool> FromTrue()
			=> __TrueResultTaskProxy.Task;

		public static Task<bool> FromFalse()
			=> __FalseResultTaskProxy.Task;

		public static Task<TResult> FromResult<TResult>(TResult result) {
			var taskCompletionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.None);
			taskCompletionSource.SetResult(result);
			return taskCompletionSource.Task;
		}

		public static Task<TResult> FromResult<TResult>(TResult result, CancellationToken cancellationToken) {
			var taskCompletionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.None);
			if (cancellationToken.IsCancellationRequested)
				taskCompletionSource.SetCanceled();
			else
				taskCompletionSource.SetResult(result);
			return taskCompletionSource.Task;
		}

		public static Task FromVoidResult(CancellationToken cancellationToken) {
			if (cancellationToken.IsCancellationRequested)
				return FromCanceled<int>(cancellationToken);
			else
				return P_SucceededTaskProxy<int>.Awaitable();
		}

		public static Task FromVoidResult()
			=> P_SucceededTaskProxy<int>.Awaitable();

		public static Task FromCanceled()
			=> P_CanceledTaskProxy<int>.Awaitable();

		public static Task FromCanceled(CancellationToken ct)
			=> FromCanceled<int>(ct);

		public static Task<TResult> FromCanceled<TResult>(CancellationToken ct) {
			var taskProxy = new TaskCompletionSource<TResult>(creationOptions: TaskCreationOptions.None);
			taskProxy.TrySetCanceled(cancellationToken: ct);
			return taskProxy.Task;
		}

		public static Task<TResult> FromCanceled<TResult>()
			=> P_CanceledTaskProxy<TResult>.Awaitable();

		public static Task FromAction(Action action) {
			action.EnsureNotNull(nameof(action));
			//
			try {
				action();
			}
			catch (OperationCanceledException operationCanceledException) {
				return FromCanceled(ct: operationCanceledException.CancellationToken);
			}
			catch (Exception exception) {
				return FromError(error: exception);
			}
			return FromVoidResult();
		}

		public static TaskWrap<TResult> Wrap<TResult>(Exception exception)
			=> TaskWrap<TResult>.Wrap(exception: exception);

		public static TaskWrap<TResult> Wrap<TResult>(this Task<TResult> task)
			=> TaskWrap<TResult>.Wrap(task: task);

		public static TaskWrap<TResult> Wrap<TResult>(Func<TResult> func)
			=> TaskWrap<TResult>.Wrap(func: func);

		public static TaskWrap<TResult> Wrap<TArg, TResult>(TArg arg, Func<TArg, TResult> func)
			=> TaskWrap<TResult>.Wrap(arg: arg, func: func);

		public static TaskWrap<TResult> Wrap<TResult>(TResult result)
			=> TaskWrap<TResult>.Wrap(result: result);

		public static TaskWrap<TResult> Wrap<TResult>(Func<Task<TResult>> factory)
			=> TaskWrap<TResult>.Wrap(factory: factory);

		public static Task<TResult> Unwrap<TResult>(this ITaskWrap<TResult> wrap) {
			wrap.EnsureNotNull(nameof(wrap));
			//
			return wrap.ConvertTask<TResult>();
		}

		public static Task<TResult> Unwrap<TResult>(this TaskWrap<TResult> wrap) {
			wrap.EnsureNotNull(nameof(wrap));
			//
			return wrap.ConvertTask<TResult>();
		}

		public static Task<TResult> FromError<TResult>(Exception error) {
			error.EnsureNotNull(nameof(error));
			//
			var taskCompletionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.None);
			taskCompletionSource.SetException(error);
			return taskCompletionSource.Task;
		}

		public static Task FromError(Exception error) {
			error.EnsureNotNull(nameof(error));
			//
			var taskCompletionSource = new TaskCompletionSource<int>(TaskCreationOptions.None);
			taskCompletionSource.SetException(error);
			return taskCompletionSource.Task;
		}

		public static bool IsSucceeded(this Task task)
			=> task.IsCompleted && !(task.IsCanceled || task.IsFaulted);

		public static void ContinueWithApplyResultTo(this Task task, TaskCompletionSource<Nil> taskProxy) {
			task.EnsureNotNull(nameof(task));
			taskProxy.EnsureNotNull(nameof(taskProxy));
			//
			task
				.ContinueWith(
					continuationAction:
						locTask => {
							if (locTask.IsCanceled)
								taskProxy.SetCanceled();
							else if (locTask.IsFaulted)
								SetException(task: locTask, setter: taskProxy.SetException);
							else
								taskProxy.SetResult(result: Nil.Value);
						},
					continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
		}

		public static void ContinueWithApplyResultTo<T>(this Task<T> task, TaskCompletionSource<T> taskProxy) {
			task.EnsureNotNull(nameof(task));
			taskProxy.EnsureNotNull(nameof(taskProxy));
			//
			task
				.ContinueWith(
					continuationAction:
						locTask => {
							if (locTask.IsCanceled)
								taskProxy.SetCanceled();
							else if (locTask.IsFaulted)
								SetException(task: locTask, setter: taskProxy.SetException);
							else
								taskProxy.SetResult(locTask.Result);
						},
					continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
		}

		public static void ContinueWithTryApplyResultTo(this Task task, TaskCompletionSource<Nil> taskProxy, TaskContinuationOptions? options = default, TaskScheduler scheduler = default) {
			task.EnsureNotNull(nameof(task));
			taskProxy.EnsureNotNull(nameof(taskProxy));
			//
			task
				.ContinueWith(
					continuationAction:
						locTask => {
							if (locTask.IsCanceled)
								taskProxy.TrySetCanceled();
							else if (locTask.IsFaulted)
								P_TrySetException(task: locTask, setter: taskProxy.TrySetException);
							else
								taskProxy.TrySetResult(result: Nil.Value);
						},
					cancellationToken: CancellationToken.None,
					continuationOptions: options ?? (scheduler is null ? TaskContinuationOptions.ExecuteSynchronously : TaskContinuationOptions.PreferFairness),
					scheduler: scheduler is null ? (TaskScheduler.Current ?? TaskScheduler.Default) : scheduler);
		}

		public static void ContinueWithTryApplyResultTo<T>(this Task<T> task, TaskCompletionSource<T> taskProxy, TaskContinuationOptions? options = default, TaskScheduler scheduler = default) {
			task.EnsureNotNull(nameof(task));
			taskProxy.EnsureNotNull(nameof(taskProxy));
			//
			task
				.ContinueWith(
					continuationAction:
						locTask => {
							if (locTask.IsCanceled)
								taskProxy.TrySetCanceled();
							else if (locTask.IsFaulted)
								P_TrySetException(task: locTask, setter: taskProxy.TrySetException);
							else
								taskProxy.TrySetResult(locTask.Result);
						},
					cancellationToken: CancellationToken.None,
					continuationOptions: options ?? (scheduler is null ? TaskContinuationOptions.ExecuteSynchronously : TaskContinuationOptions.PreferFairness),
					scheduler: scheduler is null ? (TaskScheduler.Current ?? TaskScheduler.Default) : scheduler);
		}

		public static void ContinueWithTryApplyResultTo<T>(this Task<T> task, AsyncTaskCompletionSource<T> taskProxy) {
			task.EnsureNotNull(nameof(task));
			taskProxy.EnsureNotNull(nameof(taskProxy));
			//
			task
				.ContinueWith(
					continuationAction:
						locTask => {
							if (locTask.IsCanceled)
								taskProxy.TrySetCanceled();
							else if (locTask.IsFaulted)
								P_TrySetException(task: locTask, setter: taskProxy.TrySetException);
							else
								taskProxy.TrySetResult(locTask.Result);
						},
					continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
		}

		/// <summary>
		/// Ожидает завершения выполнения указанной задачи в течение времени <seealso cref="DefaultAsyncTimeoutMilliseconds"/>.
		/// </summary>
		/// <param name="task">
		/// Задача.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="notThrowOnTimeoutExpiry">
		/// Указывает, должно ли генерироваться исключение в случае, если таймаут истекает раньше, чем задача переходит в одно из финальных состояний (завершается).
		/// <para>Если == true, то в случае, если таймаут истекает раньше, чем завершается задача, метод возвращает false.</para>
		/// </param>
		/// <returns>Значение <seealso cref="bool"/>.</returns>
		public static bool WaitWithTimeout(this Task task, bool notThrowOnTimeoutExpiry = false)
			=> P_WaitWithTimeout(task: task, millisecondsTimeout: DefaultAsyncTimeoutMilliseconds, isVoidTaskResult: true, taskResultType: null, notThrowOnTimeoutExpiry: notThrowOnTimeoutExpiry);

		/// <summary>
		/// Ожидает завершения выполнения указанной задачи в течение времени <paramref name="millisecondsTimeout"/>.
		/// </summary>
		/// <param name="task">
		/// Задача.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="millisecondsTimeout">
		/// Таймаут ожидания завершения задачи <paramref name="task"/>.
		/// </param>
		/// <param name="notThrowOnTimeoutExpiry">
		/// Указывает, должно ли генерироваться исключение в случае, если таймаут истекает раньше, чем задача переходит в одно из финальных состояний (завершается).
		/// <para>Если == true, то в случае, если таймаут истекает раньше, чем завершается задача, метод возвращает false.</para>
		/// </param>
		/// <returns>Значение <seealso cref="bool"/>.</returns>
		public static bool WaitWithTimeout(this Task task, int millisecondsTimeout, bool notThrowOnTimeoutExpiry = false)
			=>
			P_WaitWithTimeout(
				task: task,
				millisecondsTimeout: millisecondsTimeout,
				isVoidTaskResult: true,
				taskResultType: null,
				notThrowOnTimeoutExpiry: notThrowOnTimeoutExpiry);

		/// <summary>
		/// Ожидает завершения выполнения указанной задачи в течение времени <seealso cref="DefaultAsyncTimeoutMilliseconds"/>.
		/// </summary>
		/// <typeparam name="TResult">Тип возвращаемого результата задачи.</typeparam>
		/// <param name="task">
		/// Задача.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <returns>Объект <typeparamref name="TResult"/>.</returns>
		public static TResult WaitResultWithTimeout<TResult>(this Task<TResult> task) {
			P_WaitWithTimeout(
				task: task,
				millisecondsTimeout: DefaultAsyncTimeoutMilliseconds,
				isVoidTaskResult: false,
				taskResultType: typeof(TResult),
				notThrowOnTimeoutExpiry: false);
			//
			return task.Result;
		}

		public static TResult WaitResultWithTimeout<TResult>(this Task<TResult> task, int millisecondsTimeout) {
			P_WaitWithTimeout(task: task, millisecondsTimeout: millisecondsTimeout, isVoidTaskResult: false, taskResultType: typeof(TResult), notThrowOnTimeoutExpiry: false);
			return task.Result;
		}

		// TODO: Put strings into the resources.
		//
		static bool P_WaitWithTimeout(Task task, int millisecondsTimeout, bool isVoidTaskResult, Type taskResultType, bool notThrowOnTimeoutExpiry) {
			task.EnsureNotNull(nameof(task));
			//
			if (task.Wait(millisecondsTimeout))
				return true;
			else if (notThrowOnTimeoutExpiry)
				return false;
			else
				throw
					new TimeoutException(
						message: $"Операция (задача{(!isVoidTaskResult ? ", возвращающая результат" : string.Empty)}) не была завершена в отведённое время, таймаут ожидания истёк.{Environment.NewLine}\tЗадача:{task.FmtStr().GNLI2()}{(!isVoidTaskResult ? $"{Environment.NewLine}\tТип значения, возвращаемого задачей:{taskResultType.FmtStr().GNLI2()}" : string.Empty)}{Environment.NewLine}\tТаймаут ожидания:{Environment.NewLine}{TimeSpan.FromMilliseconds(millisecondsTimeout).FmtStr().Constant().IndentLines2()}");
		}

		public static Task<TResult> RunOnDefaultScheduler<TResult>(
			Func<TResult> function,
			CancellationToken cancellationToken = default,
			ArgumentPlaceholder<TaskCreationOptions> options = default) {
			//
			function.EnsureNotNull(nameof(function));
			//
			return
				Task
				.Factory
				.StartNew(
					function: function,
					cancellationToken: cancellationToken,
					creationOptions: options.HasExplicitValue ? options.ExplicitValue : TaskCreationOptions.PreferFairness,
					scheduler: TaskScheduler.Default);
		}

		public static Task<TResult> RunOnDefaultScheduler<TResult>(
			Func<Task<TResult>> factory,
			CancellationToken cancellationToken = default,
			ArgumentPlaceholder<TaskCreationOptions> options = default) {
			//
			factory.EnsureNotNull(nameof(factory));
			//
			return
				Task
				.Factory
				.StartNew(
					function: factory,
					cancellationToken: cancellationToken,
					creationOptions: options.HasExplicitValue ? options.ExplicitValue : TaskCreationOptions.PreferFairness,
					scheduler: TaskScheduler.Default)
				.Unwrap();
		}

		public static Task RunOnDefaultScheduler(Func<Task> factory, CancellationToken ct = default, ArgumentPlaceholder<TaskCreationOptions> options = default) {
			factory.EnsureNotNull(nameof(factory));
			//
			return
				Task
				.Factory
				.StartNew(
					function: factory,
					cancellationToken: ct,
					creationOptions: options.HasExplicitValue ? options.ExplicitValue : TaskCreationOptions.PreferFairness,
					scheduler: TaskScheduler.Default)
				.Unwrap();
		}

		public static Task RunOnDefaultScheduler<TState>(Func<TState, Task> factory, TState state, CancellationToken ct = default, ArgumentPlaceholder<TaskCreationOptions> options = default) {
			factory.EnsureNotNull(nameof(factory));
			//
			return
				Task
				.Factory
				.StartNew(
					function: locStateObj => factory(locStateObj is null ? default : (TState)locStateObj),
					state: state,
					cancellationToken: ct,
					creationOptions: options.HasExplicitValue ? options.ExplicitValue : TaskCreationOptions.PreferFairness,
					scheduler: TaskScheduler.Default)
				.Unwrap();
		}

		public static Task RunOnDefaultScheduler(Action action, CancellationToken ct = default, ArgumentPlaceholder<TaskCreationOptions> options = default) {
			action.EnsureNotNull(nameof(action));
			//
			return
				Task
				.Factory
				.StartNew(
					action: action,
					cancellationToken: ct,
					creationOptions: options.HasExplicitValue ? options.ExplicitValue : TaskCreationOptions.PreferFairness,
					scheduler: TaskScheduler.Default);
		}

		public static void RunOnDefaultSchedulerOneWay(Action action, CancellationToken ct = default, ArgumentPlaceholder<TaskCreationOptions> options = default, IUnhandledExceptionObserver exceptionObserver = default) {
			action.EnsureNotNull(nameof(action));
			//
			var task =
				Task
				.Factory
				.StartNew(
					action: action,
					cancellationToken: ct,
					creationOptions: options.HasExplicitValue ? options.ExplicitValue : TaskCreationOptions.PreferFairness,
					scheduler: TaskScheduler.Default);
			if (!(exceptionObserver is null))
				task
					.ContinueWith(
						continuationAction:
							(locTask, locState) => {
								var locTaskException = locTask.Exception.Flatten();
								if (!((IUnhandledExceptionObserver)locState).ObserveException(exception: locTaskException))
									throw new AggregateException(locTaskException);
							},
						continuationOptions: TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
						state: exceptionObserver);
		}

		public static Task RunOnDefaultScheduler<TState>(
			Action<TState> action,
			TState state,
			CancellationToken cancellationToken = default,
			ArgumentPlaceholder<TaskCreationOptions> options = default) {
			//
			action.EnsureNotNull(nameof(action));
			//
			return
				Task
				.Factory
				.StartNew(
					action: locStateObj => action(locStateObj is null ? default(TState) : (TState)locStateObj),
					state: state,
					cancellationToken: cancellationToken,
					creationOptions: options.Substitute(value: TaskCreationOptions.PreferFairness),
					scheduler: TaskScheduler.Default);
		}

		public static TResult GetResultIfRanToCompletion<TResult>(this TaskCompletionSource<TResult> taskProxy)
			=>
			GetResultIfRanToCompletion(
				task: taskProxy.EnsureNotNull(nameof(taskProxy)).Value.Task);

		public static TResult GetResultIfRanToCompletion<TResult>(this Task<TResult> task) {
			task.EnsureNotNull(nameof(task));
			//
			if (task.Status == TaskStatus.RanToCompletion)
				return task.Result;
			else
				return default(TResult);
		}

		public static bool GetException<TResult>(this Task<IVh<TResult>> task, out Exception exception) {
			task.EnsureNotNull(nameof(task));
			//
			if (task.IsFaulted) {
				exception = task.Exception;
				return true;
			}
			else if (task.IsCanceled) {
				exception = null;
				return false;
			}
			else if (task.Result?.HasException == true) {
				exception = task.Result.Exception.SourceException;
				return true;
			}
			else {
				exception = null;
				return false;
			}
		}

		public static bool GetException<TResult>(this Task<Vh<TResult>> task, out Exception exception) {
			task.EnsureNotNull(nameof(task));
			//
			if (task.IsFaulted) {
				exception = task.Exception;
				return true;
			}
			else if (task.IsCanceled) {
				exception = null;
				return false;
			}
			else if (task.Result?.HasException == true) {
				exception = task.Result.Exception.SourceException;
				return true;
			}
			else {
				exception = null;
				return false;
			}
		}

		// TODO: Put strings into the resources.
		//
		public static bool TryApplyResultTo<TResult>(this Task<TResult> fromTask, TaskCompletionSource<TResult> toTaskProxy) {
			fromTask.EnsureNotNull(nameof(fromTask));
			toTaskProxy.EnsureNotNull(nameof(toTaskProxy));
			//
			if (fromTask.IsCanceled)
				return toTaskProxy.TrySetCanceled();
			else if (fromTask.IsFaulted)
				return P_TrySetException(task: fromTask, setter: toTaskProxy.TrySetException);
			else if (fromTask.IsCompleted)
				return toTaskProxy.TrySetResult(result: fromTask.Result);
			else
				throw
					new EonException(message: "Недопустимая операция. Указанная задача еще не достигла одного из своих финальных состояний. Задача либо еще выполняется, либо её выполнение еще не начиналось.");
		}

		public static bool TryApplyResultFrom<T>(this AsyncTaskCompletionSource<T> toTaskProxy, Task<T> fromTask) {
			toTaskProxy.EnsureNotNull(nameof(toTaskProxy));
			fromTask.EnsureNotNull(nameof(fromTask));
			//
			if (fromTask.IsCanceled)
				return toTaskProxy.TrySetCanceled(ct: CancellationToken.None);
			else if (fromTask.IsFaulted)
				return P_TrySetException(task: fromTask, setter: toTaskProxy.TrySetException);
			else if (fromTask.IsCompleted)
				return toTaskProxy.TrySetResult(result: fromTask.Result);
			else
				throw
					new EonException(message: "Недопустимая операция. Указанная задача еще не достигла одного из своих финальных состояний. Задача либо еще выполняется, либо её выполнение еще не начиналось.");
		}

		// TODO: Put strings into the resources.
		//
		static bool P_TrySetException(Task task, Func<Exception, bool> setter) {
			task.EnsureNotNull(nameof(task));
			setter.EnsureNotNull(nameof(setter));
			//
			if (task.IsFaulted) {
				AggregateException taskException = null;
				try {
					taskException = task.Exception;
					var rethrowFlattenException = taskException.Flatten();
					var setResult = setter(rethrowFlattenException.InnerExceptions.Count == 1 ? rethrowFlattenException.InnerExceptions[ 0 ] : rethrowFlattenException);
					if (setResult)
						return true;
					else {
						taskException = null;
						throw rethrowFlattenException;
					}
				}
				catch (Exception exception) {
					if (taskException is null)
						throw;
					else
						throw new AggregateException(taskException, exception);
				}
			}
			else
				throw
					new EonException(message: $"Недопустимая операция. Либо указанная задача еще не достигла одного из своих финальных состояний, либо финальное состояние задачи не соответствует состоянию сбоя.");
		}

		// TODO: Put strings into the resources.
		//
		static internal void SetException(Task task, Action<Exception> setter)
			=> TaskUtilitiesInternalCoreL1.SetException(task: task, setter: setter);

		public static Task FromTrySetResult<TResult>(this TaskCompletionSource<TResult> taskProxy, TResult result) {
			try {
				taskProxy.EnsureNotNull(nameof(taskProxy));
				//
				return
					Task
					.Factory
					.StartNew(
						action: () => taskProxy.TrySetResult(result),
						cancellationToken: CancellationToken.None,
						creationOptions: TaskCreationOptions.PreferFairness,
						scheduler: TaskScheduler.Current ?? TaskScheduler.Default);
			}
			catch (Exception exception) {
				return FromError(error: exception);
			}
		}

		public static Task FromTrySetException<TResult>(this TaskCompletionSource<TResult> taskProxy, Exception exception) {
			try {
				taskProxy.EnsureNotNull(nameof(taskProxy));
				exception.EnsureNotNull(nameof(exception));
				//
				return
					Task
					.Factory
					.StartNew(
						function: () => taskProxy.TrySetException(exception),
						cancellationToken: CancellationToken.None,
						creationOptions: TaskCreationOptions.PreferFairness,
						scheduler: TaskScheduler.Current ?? TaskScheduler.Default);
			}
			catch (Exception caughtException) {
				return FromError(exception == null ? caughtException : new AggregateException(exception, caughtException));
			}
		}

		public static Task FromTrySetCanceled<TResult>(this TaskCompletionSource<TResult> taskProxy, CancellationToken ct = default) {
			try {
				taskProxy.EnsureNotNull(nameof(taskProxy));
				//
				return
					Task
					.Factory
					.StartNew(
						function: () => taskProxy.TrySetCanceled(ct),
						cancellationToken: CancellationToken.None,
						creationOptions: TaskCreationOptions.PreferFairness,
						scheduler: TaskScheduler.Current ?? TaskScheduler.Default);
			}
			catch (Exception exception) {
				return FromError(exception);
			}
		}

		/// <summary>
		/// Создает задачу ожидания завершения другой задачи.
		/// <para>Следует учитывать, что созданная задача не транслирует конкретное финальное состояние ожидаемой задачи (отмена, результат или сбой).</para>
		/// </summary>
		/// <param name="task">
		/// Ожидаемая задача.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="options">
		/// Опции создаваемой задачи ожидания.
		/// </param>
		/// <param name="ct">
		/// Токен отмены ожидания завершения задачи <paramref name="task"/>.
		/// </param>
		/// <param name="cancellationBreak">
		/// Указывает поведение при поступлении сигнала отмены от <paramref name="ct"/>:
		/// <para><see langword="true"/> — задача ожидания будет завершена;</para>
		/// <para><see langword="false"/> — задача ожидания будет отменена.</para>
		/// </param>
		/// <returns>Объект <see cref="Task"/>.</returns>
		public static Task WaitCompletionAsync(this Task task, TaskCreationOptions options = default, CancellationToken ct = default, bool cancellationBreak = default) {
			try {
				task.EnsureNotNull(nameof(task));
				//
				if (ct.IsCancellationRequested) {
					return cancellationBreak ? Task.CompletedTask : FromCanceled(ct: ct);
				}
				else {
					TaskCompletionSource<Nil> proxy = default;
					CancellationTokenRegistration ctRegistration = default;
					try {
						proxy = new TaskCompletionSource<Nil>(creationOptions: options & ~TaskCreationOptions.PreferFairness);
						if (ct.CanBeCanceled) {
							ctRegistration = ct.Register(callback: () => { if (cancellationBreak) proxy.TrySetResult(result: Nil.Value); else proxy.TrySetCanceled(cancellationToken: ct); });
							proxy.Task.ContinueWith(locTask => ctRegistration.Dispose(), TaskContinuationOptions.ExecuteSynchronously);
						}
						task
							.ContinueWith(
								continuationAction: locTask => proxy.TrySetResult(result: Nil.Value),
								cancellationToken: ct,
								continuationOptions: TaskContinuationOptions.ExecuteSynchronously,
								scheduler: TaskScheduler.Current ?? TaskScheduler.Default);
						if (ct.IsCancellationRequested) {
							if (cancellationBreak)
								proxy.TrySetResult(result: Nil.Value);
							else
								proxy.TrySetCanceled(cancellationToken: ct);
						}
						return proxy.Task;
					}
					catch {
						ctRegistration.Dispose();
						proxy?.TrySetCanceled();
						throw;
					}
				}
			}
			catch (Exception exception) {
				return FromError(exception);
			}
		}

		/// <summary>
		/// Создает задачу ожидания завершения другой задачи.
		/// <para>Следует учитывать, что созданная задача не транслирует конкретное финальное состояние ожидаемой задачи (отмена, результат или сбой).</para>
		/// </summary>
		/// <param name="taskProxy">
		/// Ожидаемая задача.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="options">
		/// Опции создаваемой задачи ожидания.
		/// </param>
		/// <param name="ct">
		/// Токен отмены.
		/// </param>
		/// <param name="cancellationBreak">
		/// Указывает поведение при поступлении сигнала отмены от <paramref name="ct"/>:
		/// <para><see langword="true"/> — задача ожидания будет завершена;</para>
		/// <para><see langword="false"/> — задача ожидания будет отменена.</para>
		/// </param>
		/// <returns>Объект <see cref="Task"/>.</returns>
		public static Task WaitCompletionAsync<T>(this AsyncTaskCompletionSource<T> taskProxy, TaskCreationOptions options = default, CancellationToken ct = default, bool cancellationBreak = default) {
			try {
				taskProxy.EnsureNotNull(nameof(taskProxy));
				//
				return WaitCompletionAsync(task: taskProxy.Task, options: options, ct: ct, cancellationBreak: cancellationBreak);
			}
			catch (Exception exception) {
				return FromError(exception);
			}
		}

		/// <summary>
		/// Создает задачу ожидания завершения другой задачи.
		/// <para>Следует учитывать, что созданная задача не транслирует конкретное финальное состояние ожидаемой задачи (отмена, результат или сбой).</para>
		/// </summary>
		/// <param name="taskProxy">
		/// Ожидаемая задача.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="options">
		/// Опции создаваемой задачи ожидания.
		/// </param>
		/// <param name="ct">
		/// Токен отмены.
		/// </param>
		/// <param name="cancellationBreak">
		/// Указывает поведение при поступлении сигнала отмены от <paramref name="ct"/>:
		/// <para><see langword="true"/> — задача ожидания будет завершена;</para>
		/// <para><see langword="false"/> — задача ожидания будет отменена.</para>
		/// </param>
		/// <returns>Объект <see cref="Task"/>.</returns>
		public static Task WaitCompletionAsync<T>(this TaskCompletionSource<T> taskProxy, TaskCreationOptions options = default, CancellationToken ct = default, bool cancellationBreak = default) {
			try {
				taskProxy.EnsureNotNull(nameof(taskProxy));
				//
				return WaitCompletionAsync(task: taskProxy.Task, options: options, ct: ct, cancellationBreak: cancellationBreak);
			}
			catch (Exception exception) {
				return FromError(exception);
			}
		}

		public static async Task<T> WaitResultAsync<T>(this Task<T> task, TaskCreationOptions options = default, IContext ctx = default)
			=> await WaitResultAsync(task: task, options: options, ct: ctx.Ct()).ConfigureAwait(false);

		public static Task<T> WaitResultAsync<T>(this Task<T> task, TaskCreationOptions options = default, CancellationToken ct = default) {
			try {
				task.EnsureNotNull(nameof(task));
				//
				if (ct.IsCancellationRequested)
					return FromCanceled<T>(ct: ct);
				else {
					TaskCompletionSource<T> proxy = default;
					CancellationTokenRegistration ctRegistration = default;
					try {
						proxy = new TaskCompletionSource<T>(creationOptions: options & ~TaskCreationOptions.PreferFairness);
						if (ct.CanBeCanceled) {
							ctRegistration = ct.Register(callback: () => proxy.TrySetCanceled(ct));
							proxy.Task.ContinueWith(continuationAction: locTask => ctRegistration.Dispose(), continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
						}
						task
							.ContinueWith(
								continuationAction:
									locTask => {
										if (locTask.IsCanceled)
											proxy.TrySetCanceled();
										else if (locTask.IsFaulted)
											P_TrySetException(task: locTask, setter: locTaskException => proxy.TrySetException(locTaskException));
										else
											proxy.TrySetResult(result: locTask.Result);
									},
								cancellationToken: ct,
								continuationOptions: TaskContinuationOptions.PreferFairness,
								scheduler: TaskScheduler.Current ?? TaskScheduler.Default);
						if (ct.IsCancellationRequested)
							proxy.TrySetCanceled(ct);
						return proxy.Task;
					}
					catch {
						ctRegistration.Dispose();
						proxy?.TrySetCanceled();
						throw;
					}
				}
			}
			catch (Exception exception) {
				return Task.FromException<T>(exception: exception);
			}
		}

		public static async Task WaitAsync(this Task task, TaskCreationOptions options = default, IContext ctx = default, bool cancellationBreak = default)
			=> await WaitAsync(task: task, options: options, ct: ctx.Ct(), cancellationBreak: cancellationBreak).ConfigureAwait(false);

		public static Task WaitAsync(this Task task, TaskCreationOptions options = default, CancellationToken ct = default, bool cancellationBreak = default) {
			try {
				task.EnsureNotNull(nameof(task));
				//
				if (ct.IsCancellationRequested)
					return cancellationBreak ? Task.CompletedTask : Task.FromCanceled(cancellationToken: ct);
				else {
					TaskCompletionSource<Nil> proxy = default;
					CancellationTokenRegistration ctRegistration = default;
					try {
						proxy = new TaskCompletionSource<Nil>(creationOptions: options & ~TaskCreationOptions.PreferFairness);
						if (ct.CanBeCanceled) {
							ctRegistration = ct.Register(callback: () => { if (cancellationBreak) proxy.TrySetResult(result: Nil.Value); else proxy.TrySetCanceled(cancellationToken: ct); });
							proxy.Task.ContinueWith(continuationAction: locTask => ctRegistration.Dispose(), continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
						}
						task
							.ContinueWith(
								continuationAction:
									locTask => {
										if (locTask.IsCanceled)
											proxy.TrySetCanceled();
										else if (locTask.IsFaulted)
											P_TrySetException(task: locTask, setter: locTaskException => proxy.TrySetException(exception: locTaskException));
										else
											proxy.TrySetResult(result: Nil.Value);
									},
								cancellationToken: ct,
								continuationOptions: TaskContinuationOptions.ExecuteSynchronously,
								scheduler: TaskScheduler.Current ?? TaskScheduler.Default);
						if (ct.IsCancellationRequested) {
							if (cancellationBreak)
								proxy.TrySetResult(result: Nil.Value);
							else
								proxy.TrySetCanceled(cancellationToken: ct);
						}
						return proxy.Task;
					}
					catch {
						ctRegistration.Dispose();
						proxy?.TrySetCanceled();
						throw;
					}
				}
			}
			catch (Exception exception) {
				return Task.FromException(exception: exception);
			}
		}

	}

}