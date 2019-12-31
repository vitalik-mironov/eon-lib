#region Compilation conditional symbols

#define DO_NOT_USE_OXY_LOGGING_API

#endregion
//
using System;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel;
using Eon.Threading.Tasks;
using Eon.Triggers.Description;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Triggers {

	public class PeriodTrigger<TDescription>
		:TriggerBase<TDescription>
		where TDescription : class, IPeriodTriggerDescription {

		readonly TimeSpan _period;

		readonly TimeSpan _periodVariance;

		CancellationTokenSource _currentActivationCts;

		public PeriodTrigger(IXAppScopeInstance scope, TDescription description)
			: base(scope, description) {
			_period = description.Period;
			_periodVariance = description.PeriodVariance;
		}

		// TODO: Put strings into the resources.
		//
		protected override Task DoActivateAsync(IRunControlAttemptState state) {
			try {
				state.EnsureNotNull(nameof(state));
				//
				CancellationTokenSource cts = default;
				try {
					cts = CancellationTokenSource.CreateLinkedTokenSource(tokens: new CancellationToken[ ] { App.AppShutdownToken });
					var ct = cts.Token;
					if (UpdDAIfNullBool(ref _currentActivationCts, cts))
						P_ScheduleNextSignal(period: P_SelectNextPeriod(), ct: ct);
					else
						throw
							new EonException(
								message: $"Недопустимый вызов операции. Операция уже была выполнена ранее. Для повторного выполнения необходимо сначала выполнить операцию '{nameof(DoDeactivateAsync)}'.{Environment.NewLine}\tКомпонент:{this.FmtStr().GNLI2()}");
				}
				catch (Exception exception) {
					if (!(cts is null))
						ExceptionUtilities
							.TryCatch(
								op:
									() => {
										cts.Cancel(throwOnFirstException: false);
										itrlck.SetNullBool(location: ref _currentActivationCts, comparand: cts);
										cts.Dispose();
									},
								exception: exception);
					throw;
				}
				return TaskUtilities.FromVoidResult();
			}
			catch (Exception exception) {
				return TaskUtilities.FromError(error: exception);
			}
		}

		protected override async Task DoDeactivateAsync() {
			await base.DoDeactivateAsync().ConfigureAwait(false);
			//
			var cts = ReadDA(ref _currentActivationCts);
			if (!(cts is null)) {
				cts.Cancel(throwOnFirstException: false);
				itrlck.UpdateBool(location: ref _currentActivationCts, value: null, comparand: cts);
				cts.Dispose();
			}
		}

		// TODO: Put strings into the resources.
		//
		void P_ScheduleNextSignal(TimeSpan period, CancellationToken ct) {
			TaskUtilities
				.Delay(duration: period, cancellationToken: ct)
				.ContinueWith(
					continuationAction: doSignal,
					continuationOptions: TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion);
			//
			void doSignal(Task locDelayTask) {
				if (!IsDisposeRequested) {
					Exception locSignalException = null;
					try { Signal(signalProps: new TriggerSignalProperties(trigger: this)); }
					catch (Exception locCaughtException) { locSignalException = locCaughtException; }
					//
					if (!IsDisposeRequested && !ct.IsCancellationRequested)
						P_ScheduleNextSignal(period: P_SelectNextPeriod(), ct: ct);
					//
#if !DO_NOT_USE_OXY_LOGGING_API
					if (!(locSignalException is null) && !(locSignalException is ObjectDisposedException && IsDisposeRequested))
						this
							.IssueError(
								message: $"Ошибка вызова процедуры перевода триггера в сигнальное состояние (метод '{nameof(Signal)}').",
								error: locSignalException,
								includeErrorInIssueFaultException: true,
								severityLevel: locSignalException.GetMostHighSeverityLevel(baseLevel: SeverityLevel.Medium));
#endif
				}
			}
		}

		TimeSpan P_SelectNextPeriod() {
			if (_periodVariance == TimeSpan.Zero)
				return _period;
			else {
				var selectedPeriod = _period;
				var varianceLimitTicks = Math.Abs(_periodVariance.Ticks);
				var selectedVariance = new TimeSpan(ticks: RandomUtilities.NextInt64(minInclusive: 0L, maxExclusive: varianceLimitTicks)).RoundToMilliseconds();
				if (varianceLimitTicks < 0L)
					selectedPeriod = selectedPeriod.Subtract(ts: selectedVariance);
				else
					selectedPeriod = selectedPeriod.Add(ts: selectedVariance);
				return selectedPeriod;
			}

		}

	}

}