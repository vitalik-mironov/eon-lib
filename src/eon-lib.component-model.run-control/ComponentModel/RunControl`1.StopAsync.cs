using System;
using System.Threading.Tasks;

using Eon.ComponentModel.Internal;
using Eon.Context;
using Eon.Diagnostics;
using Eon.Threading.Tasks;

using itrlck = Eon.Threading.InterlockedUtilities;
using vlt = Eon.Threading.VolatileUtilities;

namespace Eon.ComponentModel {

	public sealed partial class RunControl<TComponent> {

		public Task<IRunControlAttemptSuccess> StopAsync(TaskCreationOptions options, IContext ctx = default, bool finiteStop = default) {
			options &= ~TaskCreationOptions.PreferFairness;
			if (options == TaskCreationOptions.None)
				return StopAsync(ctx: ctx, finiteStop: finiteStop);
			else
				return TaskUtilities.WaitResultAsync(task: StopAsync(ctx: ctx, finiteStop: finiteStop), options: options, ct: ctx?.Ct() ?? default);
		}

		public Task<IRunControlAttemptSuccess> StopAsync(IContext ctx = default, bool finiteStop = default)
			=> P_StopAsync(ctx: ctx, finiteStop: finiteStop);

		// TODO: Put strings into the resources.
		//
		Task<IRunControlAttemptSuccess> P_StopAsync(IContext ctx = default, bool finiteStop = default, AsyncOperationCallOption option = default) {
			try {
				if (!(option == AsyncOperationCallOption.ExistingOrNew || option == AsyncOperationCallOption.DefaultOrNew))
					throw new ArgumentOutOfRangeException(paramName: nameof(option));
				//
				return TaskUtilities.RunOnDefaultScheduler(factory: doStopAsync);
			}
			catch (Exception exception) {
				return TaskUtilities.FromError<IRunControlAttemptSuccess>(exception);
			}
			//
			async Task<IRunControlAttemptSuccess> doStopAsync() {
				ctx.ThrowIfCancellationRequested();
				finiteStop = finiteStop || (Options & RunControlOptions.SingleStart) == RunControlOptions.SingleStart;
				var correlationId = ctx?.FullCorrelationId;
				var ctxCt = ctx.Ct();
				var existingStopOp = default(RunControlStopOperationState<TComponent>);
				var newStopOp = default(RunControlStopOperationState<TComponent>);
				var component = TryReadDA(ref _component);
				var beginTimestamp = StopwatchUtilities.GetTimestampAsTimeSpan();
				var tryCatchFinallyAffectsNewStopOp = true;
				try {
					var setStopOpResult =
						itrlck
						.Update(
							location: ref _existingStopOp,
							transform:
								(locCurrent, locPrevious) => {
									if (locCurrent is null || (finiteStop && !locCurrent.FiniteStop))
										return locPrevious ?? (newStopOp = new RunControlStopOperationState<TComponent>(runControl: this, beginTimestamp: locCurrent?.BeginTimestamp ?? beginTimestamp, ct: ctxCt, finiteStop: finiteStop));
									else
										return locCurrent;
								});
					if (ReferenceEquals(objA: newStopOp, objB: existingStopOp = setStopOpResult.Current)) {
						if (setStopOpResult.Original is null) {
							// Новая операция остановки.
							//
							// Поток (задача), который выполняет остановку.
							//
							if (!itrlck.IncrementBool(location: ref _stopOpAttemptNumber, maxInclusive: int.MaxValue, result: out var attemptNumber))
								throw new EonException(message: $"Cann't assign a sequential number to this operation attempt. All numbers exhausted.{Environment.NewLine}\tComponent:{component.FmtStr().GNLI2()}");
							//
							var existingStartOp = itrlck.Get(ref _existingStartOp);
							if (existingStartOp is null) {
								newStopOp.AttemptState = P_DefaultAttemptStateFactory(args: new RunControlAttemptStateFactoryArgs(runControl: this, completion: newStopOp.Completion, context: ctx, isStart: false, attemptNumber: attemptNumber, succeededAttemptCountBefore: vlt.Read(ref _stopOpSucceededCount)));
								itrlck.IncrementBool(location: ref _stopOpSucceededCount, maxInclusive: int.MaxValue);
								return newStopOp.SetSucceeded(isMaster: false);
							}
							else {
								existingStartOp.SignalStopRequested();
								P_TryFireStopRequestedAsynchronously();
								//
								var attemptStateFactory = itrlck.Get(ref _attemptStateFactory);
								var stopTaskFactory = itrlck.Get(ref _stopTaskFactory);
								if (attemptStateFactory is null)
									throw new EonException(message: $"Attempt state factory is missing.{Environment.NewLine}\tComponent:{component.FmtStr().GNLI2()}");
								else if (stopTaskFactory is null)
									throw new EonException(message: $"Stop task factory is missing.{Environment.NewLine}\tComponent:{component.FmtStr().GNLI2()}");
								//
								IContext locCtx = default;
								try {
									locCtx = ctx ?? ContextUtilities.Create();
									newStopOp.AttemptState = attemptStateFactory(args: new RunControlAttemptStateFactoryArgs(runControl: this, completion: newStopOp.Completion, context: locCtx, isStart: false, attemptNumber: attemptNumber, succeededAttemptCountBefore: vlt.Read(ref _stopOpSucceededCount)));
									// Ожидание завершения операции запуска.
									//
									try {
										locCtx.ThrowIfCancellationRequested();
										await existingStartOp.Completion.WaitCompletionAsync(ct: newStopOp.Ct).ConfigureAwait(false);
									}
									catch (Exception exception) {
										if (exception.HasSingleBaseExceptionOf<OperationCanceledException>(out var operationCancellationException)) {
											itrlck.SetNull(location: ref _existingStopOp, comparand: newStopOp);
											newStopOp.TrySetCanceled(ct: operationCancellationException.CancellationToken);
										}
										throw;
									}
									//
									if (existingStartOp.Completion.IsSucceeded()) {
										// Логика остановки компонента выполняется только в том случае, если операция запуска была выполнена успешно.
										//
										try {
											locCtx.ThrowIfCancellationRequested();
											var stopTask = stopTaskFactory(arg: newStopOp.AttemptState);
											if (stopTask is null)
												throw new EonException(message: $"Stop task factory has not returned a task.{Environment.NewLine}\tComponent:{component.FmtStr().GNLI2()}");
											await stopTask.ConfigureAwait(false);
										}
										catch (Exception exception) {
											if (exception.HasSingleBaseExceptionOf<OperationCanceledException>(out var operationCancellationException)) {
												itrlck.SetNull(location: ref _existingStopOp, comparand: newStopOp);
												newStopOp.TrySetCanceled(ct: operationCancellationException.CancellationToken);
												throw;
											}
											else
												throw new EonException(message: $"Component stop fault.{Environment.NewLine}\tComponent:{component.FmtStr().GNLI2()}", innerException: exception);
										}
									}
									itrlck.IncrementBool(location: ref _stopOpSucceededCount, maxInclusive: int.MaxValue);
									itrlck.SetNull(location: ref _existingStartOp, comparand: existingStartOp);
									return newStopOp.SetSucceeded(isMaster: true);
								}
								finally {
									if (!ReferenceEquals(objA: ctx, objB: locCtx))
										locCtx?.Dispose();
								}
							}
						}
						else {
							// Существующая операция остановки была заменена новой, с флагом терминальное останоки — запуск компонента больше не может быть выполнен.
							//
							setStopOpResult
								.Current
								.Completion
								.ContinueWith(
									continuationAction:
										locTask => {
											if (locTask.IsCanceled)
												newStopOp.TrySetCanceled();
											else if (locTask.IsFaulted) {
												if (!newStopOp.TrySetException(exception: locTask.Exception))
													// TODO_HIGH: Escalate to special component IUnhandledExceptionObserver.
													//
													throw new AggregateException(locTask.Exception);
											}
											else
												newStopOp.SetSucceeded(isMaster: false);
										},
									continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
							tryCatchFinallyAffectsNewStopOp = false; // See exception catch/finally handler below.
							return await newStopOp.Completion.WaitResultAsync(ct: ctxCt).ConfigureAwait(false);
						}
					}
					else {
						// Отмена ненужной задачи.
						//
						newStopOp?.TrySetCanceled(synchronously: true);
						if (option == AsyncOperationCallOption.DefaultOrNew)
							return default;
						else {
							// TODO_HIGH: Возможно, нужно обрабатывать такую ситуацию, когда данный вызов получит результат в виде отменённой операции, потому что уже выполняющий другой вызов был отменён. Такая ситуация с точки зрения вызывающий стороны не совсем очевидна и предсказуема.
							// + нужно реализовать трансляцию хода выполнения из уже выполняющегося вызова в данный.
							//
							if (existingStopOp.Ct == ctxCt && existingStopOp.FiniteStop == finiteStop)
								return new RunControlAttemptSuccess(other: await existingStopOp.Completion.ConfigureAwait(false), isMaster: false);
							else if (finiteStop && !existingStopOp.FiniteStop) {
								await existingStopOp.Completion.WaitCompletionAsync(ct: ctxCt).ConfigureAwait(false);
								if (existingStopOp.Completion.IsFaulted)
									// Здесь в дейсвтительности будет вызвано исключение.
									//
									return await existingStopOp.Completion.ConfigureAwait(false);
								else
									return await StopAsync(ctx: ctx, finiteStop: true).ConfigureAwait(false);
							}
							else
								return new RunControlAttemptSuccess(other: await existingStopOp.Completion.WaitResultAsync(ct: ctxCt).ConfigureAwait(false), isMaster: false);
						}
					}
				}
				catch (Exception exception) {
					if (ReferenceEquals(objA: existingStopOp, objB: newStopOp) && tryCatchFinallyAffectsNewStopOp)
						newStopOp?.TrySetException(exception);
					throw;
				}
				finally {
					if (!ReferenceEquals(existingStopOp, newStopOp) && tryCatchFinallyAffectsNewStopOp)
						// Отмена ненужной задачи.
						//
						newStopOp?.TrySetCanceled(synchronously: true);
				}
			}
		}

	}

}