using System;
using System.Threading.Tasks;

using Eon.Diagnostics;
using Eon.Runtime.Options;

using static Eon.TupleUtilities;

using vlt = Eon.Threading.VolatileUtilities;

namespace Eon.Triggers {

	public static class TriggerUtilities {

		#region Nested types

		abstract class P_SignalSubscriptionBase
			:Disposable {

			ITrigger _trigger;

			IUnhandledExceptionObserver _unhandledExceptionObserver;

			private protected P_SignalSubscriptionBase(ITrigger trigger, IUnhandledExceptionObserver unhandledExceptionObserver) {
				trigger.EnsureNotNull(nameof(trigger));
				unhandledExceptionObserver.EnsureNotNull(nameof(unhandledExceptionObserver));
				//
				_trigger = trigger;
				_unhandledExceptionObserver = unhandledExceptionObserver;
				//
				P_SubscribeToNextSignal();
			}

			void P_SubscribeToNextSignal() {
				var trigger = TryReadDA(ref _trigger, considerDisposeRequest: true);
				if (!(trigger is null)) {
					var nextSignalAwaitable = trigger.NextSignalAwaitable();
					nextSignalAwaitable
						.ContinueWith(
							continuationAction: locSignalTask => P_OnTriggerSignalReceived(signalProps: locSignalTask.Result.SignalProperties),
							continuationOptions: TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.PreferFairness);
					nextSignalAwaitable.ContinueWith(continuationAction: locTask => Dispose(), continuationOptions: TaskContinuationOptions.NotOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
				}
			}

			// TODO: Put strings into the resources.
			//
			void P_OnTriggerSignalReceived(ITriggerSignalProperties signalProps) {
				var unhandledExceptionObserver = TryReadDA(ref _unhandledExceptionObserver);
				if (!IsDisposeRequested) {
					try {
						Exception signalCaughtException = default;
						try {
							OnSignal(signalProps: signalProps);
						}
						catch (Exception exception) {
							signalCaughtException = new EonException(message: $"Ошибка обработчика сигнала триггера по подписке.{Environment.NewLine}\tСигнал:{signalProps.FmtStr().GNLI2()}", innerException: exception);
						}
						try { P_SubscribeToNextSignal(); }
						catch (Exception exception) {
							if (signalCaughtException is null)
								throw;
							else
								throw new AggregateException(signalCaughtException, exception);
						}
					}
					catch (Exception exception) {
						if (!unhandledExceptionObserver.ObserveException(exception: exception, component: signalProps))
							throw;
					}
				}
			}

			protected abstract void OnSignal(ITriggerSignalProperties signalProps);

			protected override void Dispose(bool explicitDispose) {
				_trigger = null;
				//
				base.Dispose(explicitDispose);
			}

		}

		sealed class P_SignalSubscription
			:P_SignalSubscriptionBase {

			Action<ITriggerSignalProperties, IDisposable> _onSignal;

			internal P_SignalSubscription(ITrigger trigger, IUnhandledExceptionObserver unhandledExceptionObserver, Action<ITriggerSignalProperties, IDisposable> onSignal)
				: base(trigger: trigger, unhandledExceptionObserver: unhandledExceptionObserver) {
				//
				onSignal.EnsureNotNull(nameof(onSignal));
				//
				_onSignal = onSignal;
			}

			protected override void OnSignal(ITriggerSignalProperties signalProps)
				=> TryReadDA(ref _onSignal, considerDisposeRequest: true)?.Invoke(arg1: signalProps, arg2: this);

			protected override void Dispose(bool explicitDispose) {
				_onSignal = null;
				//
				base.Dispose(explicitDispose);
			}

		}

		sealed class P_SignalSubscription<TState>
			:P_SignalSubscriptionBase {

			TState _state;

			Action<ITriggerSignalProperties, IDisposable, TState> _onSignal;

			internal P_SignalSubscription(ITrigger trigger, IUnhandledExceptionObserver unhandledExceptionObserver, Action<ITriggerSignalProperties, IDisposable, TState> onSignal, TState state)
				: base(trigger: trigger, unhandledExceptionObserver: unhandledExceptionObserver) {
				//
				onSignal.EnsureNotNull(nameof(onSignal));
				//
				_state = state;
				_onSignal = onSignal;
			}

			protected override void OnSignal(ITriggerSignalProperties signalProps) {
				var onSignal = vlt.Read(ref _onSignal);
				var state = vlt.Read(ref _state);
				if (!IsDisposeRequested)
					onSignal(arg1: signalProps, arg2: this, arg3: state);
			}

			protected override void Dispose(bool explicitDispose) {
				_state = default;
				_onSignal = null;
				//
				base.Dispose(explicitDispose);
			}

		}

		#endregion

		/// <summary>
		/// Минимальная периодичность перехода триггера в сигнальное состояние.
		/// <para>Значение: '00:00:00.503'.</para>
		/// </summary>
		public static TimeSpan PeriodMin = TimeSpan.FromMilliseconds(value: 503.0D); // no any magic. just first prime number greater than 500.

		/// <summary>
		/// Максимальная периодичность перехода триггера в сигнальное состояние.
		/// <para>Значение: '1.00:00:00'.</para>
		/// </summary>
		public static TimeSpan PeriodMax = TimeSpan.FromDays(value: 1.0D);

		public static IDisposable SubscribeToSignal(this ITrigger trigger, Action<ITriggerSignalProperties, IDisposable> onSignal, IUnhandledExceptionObserver exceptionObserver = default)
			=> new P_SignalSubscription(trigger, unhandledExceptionObserver: exceptionObserver ?? UnhandledExceptionObserverOption.Require(), onSignal);

		public static IDisposable SubscribeToSignal<TState>(this ITrigger trigger, Action<ITriggerSignalProperties, IDisposable, TState> onSignal, TState state, IUnhandledExceptionObserver exceptionObserver = default)
			=> new P_SignalSubscription<TState>(trigger: trigger, unhandledExceptionObserver: exceptionObserver ?? UnhandledExceptionObserverOption.Require(), onSignal: onSignal, state: state);

		public static IDisposable SubscribeToSignal<TState1, TState2>(
			this ITrigger trigger, 
			Action<ITriggerSignalProperties, IDisposable, TState1, TState2> onSignal, 
			TState1 state1, 
			TState2 state2, 
			IUnhandledExceptionObserver exceptionObserver = default) {
			//
			onSignal.EnsureNotNull(nameof(onSignal));
			//
			return
				new P_SignalSubscription<Tuple<TState1, TState2>>(
					trigger: trigger,
					unhandledExceptionObserver: exceptionObserver ?? UnhandledExceptionObserverOption.Require(),
					onSignal: (locSignalProps, locSubscription, locState) => onSignal(locSignalProps, locSubscription, locState.Item1, locState.Item2),
					state: Tuple(state1, state2));
		}

		public static IDisposable SubscribeToSignal<TState1, TState2, TState3>(
			this ITrigger trigger, 
			Action<ITriggerSignalProperties, IDisposable, TState1, TState2, TState3> onSignal, 
			TState1 state1, 
			TState2 state2, 
			TState3 state3, 
			IUnhandledExceptionObserver exceptionObserver = default) {
			//
			onSignal.EnsureNotNull(nameof(onSignal));
			//
			return
				new P_SignalSubscription<Tuple<TState1, TState2, TState3>>(
					trigger: trigger,
					unhandledExceptionObserver: exceptionObserver ?? UnhandledExceptionObserverOption.Require(),
					onSignal: (locSignalProps, locSubscription, locState) => onSignal(locSignalProps, locSubscription, locState.Item1, locState.Item2, locState.Item3),
					state: Tuple(state1, state2, state3));
		}

		public static ITriggerSignalProperties GetOriginator(this ITriggerSignalProperties signalProps) {
			signalProps.EnsureNotNull(nameof(signalProps));
			//
			var source = signalProps;
			for (; ; ) {
				var nextSource = source.Source;
				if (nextSource is null)
					return source;
				else
					source = nextSource;
			}
		}

	}

}