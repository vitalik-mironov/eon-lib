using System;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel;
using Eon.Threading.Tasks;
using Eon.Triggers.Description;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Triggers {

	public abstract class TriggerBase<TDescription>
		:ActivatableXAppScopeInstanceBase<TDescription, TriggerBase<TDescription>>, ITrigger<TDescription>
		where TDescription : class, ITriggerDescription {

		readonly bool _isDisabled;

		TriggerSignalState _signalState;

		AsyncTaskCompletionSource<TriggerSignalEventArgs> _nextSignalAwaitable;

		protected TriggerBase(IXAppScopeInstance scope, TDescription description)
			: base(scope: scope, description: description) {
			//
			_isDisabled = description.IsDisabled;
		}

		public bool IsDisabled {
			get {
				EnsureNotDisposeState();
				return _isDisabled;
			}
		}

		protected override bool IsAfterActivationEnabled
			=> base.IsAfterActivationEnabled || Description.SignalOnActivation;

		IRunControl<ITrigger> ITrigger.ActivateControl
			=> ActivateControl;

		public TriggerSignalState SignalState
			=> itrlck.Get(ref _signalState);

		protected sealed override RunControlOptions DefineDefaultOfActivateControlOptions()
			/* Важно! Изменение возвращаемого значения обязательно требует ревью кода этого компонента. */
			=> RunControlOptions.SingleStart;

		protected override async Task DoAfterActivateAsync(IRunControlAttemptSuccess result) {
			await base.DoAfterActivateAsync(result: result).ConfigureAwait(false);
			//
			if (Description.SignalOnActivation && (!Description.SignalOnFirstActivationOnly || result.SucceededAttemptCountBefore == 0))
				SignalAtMostOne(signalProps: new TriggerSignalProperties(trigger: this, correlationId: result.CorrelationId));
		}

		protected override async Task DoDeactivateAsync() {
			itrlck.SetNull(ref _signalState);
			await Task.CompletedTask;
		}

		public Task<TriggerSignalEventArgs> NextSignalAwaitable() {
			try {
				var existingAwaitable = TryReadDA(ref _nextSignalAwaitable, considerDisposeRequest: true);
				if (existingAwaitable is null) {
					if (HasDeactivationRequested)
						return TaskUtilities.FromCanceled<TriggerSignalEventArgs>();
					else {
						AsyncTaskCompletionSource<TriggerSignalEventArgs> newAwaitable = default;
						try {
							try {
								UpdDAIfNullBool(location: ref _nextSignalAwaitable, factory: () => newAwaitable = new AsyncTaskCompletionSource<TriggerSignalEventArgs>(), current: out existingAwaitable);
							}
							catch (ObjectDisposedException) {
								if (IsDisposeRequested)
									return TaskUtilities.FromCanceled<TriggerSignalEventArgs>();
								else
									throw;
							}
						}
						finally {
							if (!ReferenceEquals(existingAwaitable, newAwaitable))
								newAwaitable?.TrySetCanceled(ct: CancellationToken.None);
						}
					}
				}
				return existingAwaitable.Task;
			}
			catch (Exception exception) {
				return TaskUtilities.FromError<TriggerSignalEventArgs>(error: exception);
			}
		}

		protected virtual void SignalCondition(ITriggerSignalProperties signalProps, TriggerSignalState inState, out TriggerSignalState outState) {
			signalProps.EnsureNotNull(nameof(signalProps));
			//
			if (inState is null)
				outState = new TriggerSignalState(signalCounter: 1L, lastSignalTimestamp: signalProps.Timestamp);
			else
				switch (inState.SignalCounter) {
					case long.MaxValue:
						outState = inState;
						break;
					default:
						outState = new TriggerSignalState(signalCounter: inState.SignalCounter + 1L, lastSignalTimestamp: signalProps.Timestamp);
						break;
				}
		}

		public bool Signal(ITriggerSignalProperties signalProps)
			=> P_Signal(signalProps: signalProps, condition: P_DefaultSignalCondition);

		TriggerSignalState P_DefaultSignalCondition(ITriggerSignalProperties signalProps, TriggerSignalState inState) {
			SignalCondition(signalProps: signalProps, inState: inState, outState: out var outState);
			return outState;
		}

		public bool SignalAtMostOne(ITriggerSignalProperties signalProps)
			=> P_Signal(signalProps: signalProps, condition: P_AtMostOneSignalCondition);

		TriggerSignalState P_AtMostOneSignalCondition(ITriggerSignalProperties signalProps, TriggerSignalState inState) {
			if ((inState?.SignalCounter ?? 0L) == 0L)
				return P_DefaultSignalCondition(signalProps: signalProps, inState: inState);
			else
				return inState;
		}

		// TODO: Put strings into the resources.
		//
		bool P_Signal(ITriggerSignalProperties signalProps, Func<ITriggerSignalProperties, TriggerSignalState, TriggerSignalState> condition) {
			signalProps.EnsureNotNull(nameof(signalProps));
			signalProps.Trigger.ArgProp($"{nameof(signalProps)}.{nameof(signalProps.Trigger)}").EnsureIs(operand: this);
			condition.EnsureNotNull(nameof(condition));
			//
			TriggerSignalEventArgs signalEventArgs = default;
			for (; P_CanSignal();) {
				var inState = itrlck.Get(ref _signalState);
				var outState = condition(arg1: signalProps, arg2: inState);
				if (ReferenceEquals(inState, outState))
					return false;
				else if (outState.SignalCounter < (inState?.SignalCounter ?? 0L))
					throw new EonException(message: $"Не выполняется условие '{nameof(inState)}.{nameof(inState.SignalCounter)} <= {nameof(outState)}.{nameof(outState.SignalCounter)}.'");
				else if (itrlck.UpdateBool(location: ref _signalState, value: outState, comparand: inState)) {
					if (P_CanSignal() && (inState?.SignalCounter ?? 0L) != outState.SignalCounter) {
						itrlck.SetNull(location: ref _nextSignalAwaitable)?.TrySetResult(result: signalEventArgs = signalEventArgs ?? new TriggerSignalEventArgs(signalProps: signalProps));
						return true;
					}
					else
						return false;
				}
			}
			return false;
		}

		bool P_CanSignal()
			=> !_isDisabled && IsActive;

		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose) {
				TryReadDA(ref _nextSignalAwaitable)?.TrySetCanceled(ct: CancellationToken.None);
			}
			//
			base.FireBeforeDispose(explicitDispose);
		}

		protected override void Dispose(bool explicitDispose) {
			_nextSignalAwaitable = null;
			_signalState = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}