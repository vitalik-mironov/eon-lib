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

		public Task<IRunControlAttemptSuccess> StartAsync(TaskCreationOptions options, IContext ctx = default) {
			options &= ~TaskCreationOptions.PreferFairness;
			if (options == TaskCreationOptions.None)
				return StartAsync(ctx: ctx);
			else
				return TaskUtilities.WaitResultAsync(task: StartAsync(ctx: ctx), options: options, ct: ctx.Ct());
		}

		// TODO: Put strings into the resources.
		//
		public Task<IRunControlAttemptSuccess> StartAsync(IContext ctx = default) {
			try {
				var component = Component;
				if ((Options & RunControlOptions.ForbidStart) == RunControlOptions.ForbidStart)
					throw new EonException(message: $"Опциями элемента управления запуском/остановкой компонента, запрещено выполнение операции запуска.{Environment.NewLine}\tКомпонент:{component.FmtStr().GNLI2()}");
				else
					return TaskUtilities.RunOnDefaultScheduler(factory: doStartAsync);
			}
			catch (Exception exception) {
				return Task.FromException<IRunControlAttemptSuccess>(exception: exception);
			}
			//
			async Task<IRunControlAttemptSuccess> doStartAsync() {
				ctx.ThrowIfCancellationRequested();
				var ctxCt = ctx.Ct();
				var existingStartOp = default(RunControlStartOperationState<TComponent>);
				var newStartOp = default(RunControlStartOperationState<TComponent>);
				var component = Component;
				var begimTimestamp = StopwatchUtilities.GetTimestampAsTimeSpan();
				try {
					UpdDABool(
						location: ref _existingStartOp,
						transform:
							(locCurrent, locPrevious) => {
								if (locCurrent is null)
									return locPrevious ?? (newStartOp = new RunControlStartOperationState<TComponent>(runControl: this, beginTimestamp: begimTimestamp, ct: ctxCt));
								else
									return locCurrent;
							},
						current: out existingStartOp);
					if (ReferenceEquals(newStartOp, existingStartOp)) {
						// Поток (задача), который непосредственно выполняет запуск.
						//
						if (!itrlck.IncrementBool(location: ref _startOpAttemptNumber, maxInclusive: int.MaxValue, result: out var attemptNumber))
							throw
								new EonException(message: $"Невозможно присвоить операции последовательный номер попытки. Счётчик попыток достиг предельного значения.{Environment.NewLine}\tКомпонент:{component.FmtStr().GNLI2()}");
						// Если выполняется операция остановки, то дождаться её завершения.
						//
						var existingStopOp = itrlck.Get(ref _existingStopOp);
						if (!(existingStopOp is null)) {
							try {
								await existingStopOp.Completion.WaitCompletionAsync(ct: ctxCt).ConfigureAwait(false);
							}
							catch (Exception exception) {
								if (exception.HasSingleBaseExceptionOf<OperationCanceledException>(out var operationCancellationException)) {
									itrlck.SetNullBool(ref _existingStartOp, comparand: newStartOp);
									newStartOp.TrySetCanceled(ct: operationCancellationException.CancellationToken);
								}
								throw;
							}
							try {
								await existingStopOp.Completion.ConfigureAwait(false);
							}
							catch (Exception exception) when (exception.HasSingleBaseExceptionOf<OperationCanceledException>()) {
								throw
									new EonException(
										message: $"Невозможно выполнить запуск компонента, так как ранее для компонента была инициирована операция остановки, которая была прервана или отменена.{Environment.NewLine}\tКомпонент:{component.FmtStr().GNLI2()}");
							}
							catch (Exception exception) {
								throw
									new EonException(
										message: $"Невозможно выполнить запуск компонента, так как ранее для компонента была инициирована операция остановки, которая завершилась сбоем. Детали сбоя см. во внутреннем стеке данного исключения.{Environment.NewLine}\tКомпонент:{component.FmtStr().GNLI2()}",
										innerException: exception);
							}
							if ((Options & RunControlOptions.MultipleStart) == RunControlOptions.MultipleStart) {
								if (existingStopOp.FiniteStop) {
									newStartOp.TrySetCanceled();
									throw
										new OperationCanceledException(
											message: $"Запуск компонента отменён, так как ранее поступил запрос остановки компонента без возможности перезапуска.{Environment.NewLine}\tКомпонент:{component.FmtStr().GNLI2()}");
								}
								else if (!itrlck.SetNullBool(ref _existingStopOp, comparand: existingStopOp))
									throw
										new EonException(
											message: $"Не удалось привести управляющий элемент запуска/остановки компонента к предстартовому состоянию.{Environment.NewLine}\tКомпонент:{component.FmtStr().GNLI2()}");
							}
							else {
								newStartOp.TrySetCanceled();
								throw
									new OperationCanceledException(
										message: $"Запуск компонента отменён, так как ранее поступил запрос остановки компонента и опциями элемента, управляющего запуском/остановкой компонента, не определена возможность многократного запуска компонента.{Environment.NewLine}\tКомпонент:{component.FmtStr().GNLI2()}");
							}
						}
						// Запуск.
						//
						IContext locCtx = default;
						try {
							try {
								locCtx = P_CreateStartCtx(startOp: newStartOp, outerCtx: ctx);
								newStartOp.AttemptState =
									ReadDA(ref _attemptStateFactory)
									(args:
										new RunControlAttemptStateFactoryArgs(
											runControl: this,
											completion: newStartOp.Completion,
											context: locCtx,
											isStart: true,
											attemptNumber: attemptNumber,
											succeededAttemptCountBefore: vlt.Read(ref _startOpSucceededCount)));
							}
							catch (Exception exception) {
								if (exception.HasSingleBaseExceptionOf<OperationCanceledException>(baseException: out var operationCancellationException)) {
									itrlck.SetNullBool(location: ref _existingStartOp, comparand: newStartOp);
									newStartOp.TrySetCanceled(ct: operationCancellationException.CancellationToken);
								}
								throw;
							}
							// Процедура предзапуска.
							//
							try {
								locCtx.ThrowIfCancellationRequested();
								await P_BeforeStartAsync(state: newStartOp.AttemptState).ConfigureAwait(false);
							}
							catch (Exception exception) {
								itrlck.SetNullBool(ref _existingStartOp, comparand: newStartOp);
								if (exception.HasSingleBaseExceptionOf<OperationCanceledException>(out var operationCancellationException)) {
									newStartOp.TrySetCanceled(ct: operationCancellationException.CancellationToken);
									throw;
								}
								else
									throw new EonException(message: $"Сбой во время процедуры предзапуска компонента.{Environment.NewLine}\tКомпонент:{component.FmtStr().GNLI2()}", innerException: exception);
							}
							// Процедура запуска.
							//
							try {
								locCtx.ThrowIfCancellationRequested();
								var startTask = ReadDA(ref _startTaskFactory)(arg: newStartOp.AttemptState);
								if (startTask is null)
									throw new EonException(message: $"Невозможно выполнить операцию запуска компонента, так как функция создания задачи выполнения запуска возвратила недопустимый результат '{startTask.FmtStr().G()}'.{Environment.NewLine}\tКомпонент:{component.FmtStr().GNLI2()}");
								await startTask.ConfigureAwait(false);
							}
							catch (Exception exception) {
								if (exception.HasSingleBaseExceptionOf<OperationCanceledException>(out var operationCancellationException)) {
									itrlck.SetNullBool(ref _existingStartOp, comparand: newStartOp);
									newStartOp.TrySetCanceled(ct: operationCancellationException.CancellationToken);
									throw;
								}
								else
									throw new EonException(message: $"Сбой во время запуска компонента.{Environment.NewLine}\tКомпонент:{component.FmtStr().GNLI2()}", innerException: exception);
							}
							itrlck.IncrementBool(location: ref _startOpSucceededCount, maxInclusive: int.MaxValue);
							var result = newStartOp.SetSucceeded(isMaster: true);
							P_TryFireAfterStartAsynchronously(result: result);
							return result;
						}
						finally {
							locCtx?.Dispose();
						}
					}
					else {
						newStartOp?.TrySetCanceled(synchronously: true);
						// TODO_HIGH: Возможно, нужно обрабатывать такую ситуацию, когда данный вызов получит результат в виде отменённой операции, потому что уже выполняющий другой вызов был отменён. Такая ситуация с точки зрения вызывающий стороны не совсем очевидна и предсказуема.
						// + нужно реализовать трансляцию хода выполнения из уже выполняющегося вызова в данный.
						//
						if (ctxCt == existingStartOp.Ct)
							// Один и тот же токен отмены из разных вызовов.
							//
							return new RunControlAttemptSuccess(other: await existingStartOp.Completion.ConfigureAwait(false), isMaster: false);
						else
							return new RunControlAttemptSuccess(other: await existingStartOp.Completion.WaitResultAsync(ct: ctxCt).ConfigureAwait(false), isMaster: false);
					}
				}
				catch (Exception exception) {
					if (ReferenceEquals(existingStartOp, newStartOp) && !(newStartOp is null)) {
						if (newStartOp.TrySetException(exception: exception, synchronously: true))
							// Здесь будет вызвано исключение.
							//
							await newStartOp.Completion;
						throw;
					}
					else
						throw;
				}
				finally {
					if (!ReferenceEquals(existingStartOp, newStartOp))
						newStartOp?.TrySetCanceled(synchronously: true);
				}
			}
		}

	}

}