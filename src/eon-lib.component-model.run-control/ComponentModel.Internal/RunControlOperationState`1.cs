using System;
using System.Threading;
using System.Threading.Tasks;

using Eon.Diagnostics;
using Eon.Threading.Tasks;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.ComponentModel.Internal {

	internal abstract class RunControlOperationState<TComponent>
		where TComponent : class {

		readonly AsyncTaskCompletionSource<IRunControlAttemptSuccess> _completionProxy;

		IRunControlAttemptState _attemptState;

		protected RunControlOperationState(RunControl<TComponent> runControl, TimeSpan beginTimestamp, CancellationToken ct) {
			runControl.EnsureNotNull(nameof(runControl));
			beginTimestamp.Arg(nameof(beginTimestamp)).EnsureNotLessThan(operand: TimeSpan.Zero);
			//
			RunControl = runControl;
			BeginTimestamp = beginTimestamp;
			Ct = ct;
			_completionProxy = new AsyncTaskCompletionSource<IRunControlAttemptSuccess>(options: TaskCreationOptions.None);
			_completionProxy.Task.ContinueWith(continuationAction: P_LogAttemptContinuation, continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
		}

		public RunControl<TComponent> RunControl { get; }

		public TimeSpan BeginTimestamp { get; }

		public CancellationToken Ct { get; }

		public Task<IRunControlAttemptSuccess> Completion
			=> _completionProxy.Task;

		public bool TrySetCanceled(CancellationToken ct = default, bool synchronously = default)
			=> _completionProxy.TrySetCanceled(ct: ct, synchronously: synchronously);

		public bool TrySetException(Exception exception, bool synchronously = default)
			=> _completionProxy.TrySetException(exception: exception, synchronously: synchronously);

		// TODO: Put strings into the resources.
		//
		public RunControlAttemptSuccess SetSucceeded(bool isMaster) {
			var success = new RunControlAttemptSuccess(duration: StopwatchUtilities.GetTimestampAsTimeSpan().Subtract(ts: BeginTimestamp), isMaster: isMaster, props: AttemptState);
			if (_completionProxy.TrySetResult(result: success))
				return success;
			else
				throw new EonException(message: $"Final state has already been set earlier.{Environment.NewLine}\tComponent:{RunControl.FmtStr().GNLI2()}");
		}

		// TODO: Put strings into the resources.
		//
		public IRunControlAttemptState AttemptState {
			get {
				var state = itrlck.Get(location: ref _attemptState);
				if (state is null)
					throw new EonException(message: $"Property '{nameof(AttemptState)}' is not initialized with a value.");
				else
					return state;
			}
			set {
				value.EnsureNotNull(nameof(value));
				if (!ReferenceEquals(RunControl, value.RunControl))
					throw new ArgumentOutOfRangeException(paramName: nameof(value));
				//
				if (!itrlck.UpdateIfNullBool(location: ref _attemptState, value: value))
					throw new EonException(message: $"Value of property '{nameof(AttemptState)}' set once and can't be changed.");
			}
		}

		void P_LogAttemptContinuation(Task<IRunControlAttemptSuccess> completion) {
			completion.EnsureNotNull(nameof(completion));
			//
			RunControlAttemptLoggingData loggingData;
			if (completion.IsCanceled || completion.IsFaulted) {
				var endTimestamp = StopwatchUtilities.GetTimestampAsTimeSpan();
				var attemptState = itrlck.Get(location: ref _attemptState);
				if (!(attemptState is null)) {
					// Если состояние попытки не было установлено операцией запуска/остановки, то такая попытка считается "пустой". См. код Start/Stop.
					//
					if (completion.IsCanceled)
						loggingData = new RunControlAttemptLoggingData(runControl: RunControl, isStart: attemptState.IsStart, attemptNumber: attemptState.AttemptNumber, succeededAttemptCountBefore: attemptState.SucceededAttemptCountBefore, status: OperationCompletionStatusCode.Cancel, duration: endTimestamp.Subtract(ts: BeginTimestamp), correlationId: attemptState.CorrelationId);
					else {
						Exception completionException = default;
						try {
							completion.GetAwaiter().GetResult();
						}
						catch (Exception locException) {
							completionException = locException;
						}
						loggingData = new RunControlAttemptLoggingData(runControl: RunControl, isStart: attemptState.IsStart, attemptNumber: attemptState.AttemptNumber, succeededAttemptCountBefore: attemptState.SucceededAttemptCountBefore, status: OperationCompletionStatusCode.Fault, exception: completionException, duration: endTimestamp.Subtract(ts: BeginTimestamp), correlationId: attemptState.CorrelationId);
					}
				}
				else
					loggingData = null;
			}
			else {
				var success = completion.Result;
				loggingData = new RunControlAttemptLoggingData(runControl: success.RunControl, isStart: success.IsStart, attemptNumber: success.AttemptNumber, succeededAttemptCountBefore: success.SucceededAttemptCountBefore, status: OperationCompletionStatusCode.Success, duration: success.Duration, correlationId: success.CorrelationId);
			}
			if (!(loggingData is null))
				RunControl.TryFireLogAttemptAsynchronously(data: loggingData);
		}

	}

}