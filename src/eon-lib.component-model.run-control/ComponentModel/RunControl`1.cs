using System;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel.Internal;
using Eon.Context;
using Eon.Threading;
using Eon.Threading.Tasks;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.ComponentModel {

	public sealed partial class RunControl<TComponent>
		:Disposable, IRunControl<TComponent>
		where TComponent : class {

		#region Static & constant members

		// TODO: Put strings into the resources.
		//
		static void P_EnsureOptionsValid(RunControlOptions options) {
			switch (options & (RunControlOptions.SingleStart | RunControlOptions.MultipleStart)) {
				case RunControlOptions.MultipleStart:
				case RunControlOptions.SingleStart:
					break;
				case (RunControlOptions.SingleStart | RunControlOptions.MultipleStart):
					throw
						new ArgumentException(
							message: $"Указаны несовместимые опции элемента управления запуском/остановки компонента. Опция '{RunControlOptions.SingleStart.ToString()}' не совместима с опцией '{RunControlOptions.MultipleStart.ToString()}'.",
							paramName: nameof(options));
				case RunControlOptions.None:
					throw
						new ArgumentException(
							message: $"Указанные опции элемента управления запуском/остановки компонента '{options.ToString()}' не определяют режим запуска компонента: однократный или многократный.",
							paramName: nameof(options));
			}
		}

		static IRunControlAttemptState P_DefaultAttemptStateFactory(in RunControlAttemptStateFactoryArgs args)
			=> new RunControlAttemptState(args: in args);

		#endregion

		readonly RunControlOptions _options;

		TComponent _component;

		RunControlAttemptStateFactory _attemptStateFactory;

		Func<IRunControlAttemptState, Task> _beforeStartTaskFactory;

		Func<IRunControlAttemptState, Task> _startTaskFactory;

		Func<IRunControlAttemptState, Task> _stopTaskFactory;

		int _startOpAttemptNumber;

		int _startOpSucceededCount;

		int _stopOpAttemptNumber;

		int _stopOpSucceededCount;

		EventHandler<RunControlBeforeStartEventArgs> _eventHandler_BeforeStart;

		EventHandler<RunControlAfterStartEventArgs> _eventHandler_AfterStart;

		EventHandler<RunControlLogAttemptEventArgs> _eventHandler_LogAttempt;

		EventHandler<RunControlEventArgs> _eventHandler_StopRequested;

		RunControlStartOperationState<TComponent> _existingStartOp;

		RunControlStopOperationState<TComponent> _existingStopOp;

		CancellationToken _stopToken;

		CancellationTokenRegistration _stopTokenRegistration;

		public RunControl(RunControlOptions options, TComponent component, Func<IRunControlAttemptState, Task> start, Func<Task> stop, CancellationToken stopToken = default)
			: this(options: options, component: component, attemptState: null, beforeStart: null, start: start, stop: stop, stopToken: stopToken) { }

		public RunControl(RunControlOptions options, TComponent component, Func<IRunControlAttemptState, Task> beforeStart, Func<IRunControlAttemptState, Task> start, Func<Task> stop, CancellationToken stopToken = default)
			: this(options: options, component: component, attemptState: null, beforeStart: beforeStart, start: start, stop: stop, stopToken: stopToken) { }

		public RunControl(
			TComponent component,
			Func<IRunControlAttemptState, Task> beforeStart,
			Func<IRunControlAttemptState, Task> start,
			Func<Task> stop,
			CancellationToken stopToken = default)
			: this(options: RunControlOptions.SingleStart, component: component, attemptState: null, beforeStart: beforeStart, start: start, stop: stop, stopToken: stopToken) { }

		public RunControl(TComponent component, Func<IRunControlAttemptState, Task> start, Func<Task> stop, CancellationToken stopToken = default)
			: this(options: RunControlOptions.SingleStart, component: component, attemptState: null, beforeStart: null, start: start, stop: stop, stopToken: stopToken) { }

		public RunControl(RunControlOptions options, TComponent component, RunControlAttemptStateFactory attemptState, Func<IRunControlAttemptState, Task> beforeStart, Func<IRunControlAttemptState, Task> start, Func<Task> stop, CancellationToken stopToken = default)
			: this(options: options, component: component, attemptState: attemptState, beforeStart: beforeStart, start: start, stop: stop.EnsureNotNull(nameof(stop)).Value.Fluent().Select(locValue => new Func<IRunControlAttemptState, Task>(locState => locValue())).Value, stopToken: stopToken) { }

		public RunControl(RunControlOptions options, TComponent component, RunControlAttemptStateFactory attemptState, Func<IRunControlAttemptState, Task> beforeStart, Func<IRunControlAttemptState, Task> start, Func<IRunControlAttemptState, Task> stop, CancellationToken stopToken = default) {
			P_EnsureOptionsValid(options);
			component.EnsureNotNull(nameof(component));
			start.EnsureNotNull(nameof(start));
			stop.EnsureNotNull(nameof(stop));
			//
			_options = options;
			_component = component;
			if (attemptState is null)
				_attemptStateFactory = P_DefaultAttemptStateFactory;
			else
				_attemptStateFactory = attemptState;
			_beforeStartTaskFactory = beforeStart;
			_startTaskFactory = start;
			_stopTaskFactory = stop;
			//
			_startOpAttemptNumber = -1;
			_startOpSucceededCount = 0;
			_stopOpAttemptNumber = -1;
			_stopOpSucceededCount = 0;
			//
			_stopToken = stopToken;
			//
			if ((options & RunControlOptions.AutoStopOnTokenSignal) == RunControlOptions.AutoStopOnTokenSignal) {
				if (stopToken.IsCancellationRequested)
					StopAsync(finiteStop: true).WaitWithTimeout();
				else if (stopToken.CanBeCanceled)
					_stopTokenRegistration = stopToken.Register(callback: () => TaskUtilities.RunOnDefaultScheduler(factory: () => P_StopAsync(finiteStop: true, option: AsyncOperationCallOption.DefaultOrNew)));
			}
		}

		public TComponent Component
			=> ReadDA(ref _component);

		object IRunControl.Component
			=> Component;

		public bool HasStopRequested
			=> !(itrlck.Get(ref _existingStopOp) is null);

		public RunControlOptions Options
			=> _options;

		public bool IsStarting {
			get {
				var existingStartOp = TryReadDA(ref _existingStartOp);
				if (existingStartOp is null)
					return false;
				else
					return !existingStartOp.Completion.IsCompleted;
			}
		}

		public bool IsStarted {
			get {
				var existingStartOp = TryReadDA(ref _existingStartOp);
				if (existingStartOp is null)
					return false;
				else
					return existingStartOp.Completion.IsSucceeded();
			}
		}

		public event EventHandler<RunControlBeforeStartEventArgs> BeforeStart {
			add { AddEventHandler(ref _eventHandler_BeforeStart, value); }
			remove { RemoveEventHandler(ref _eventHandler_BeforeStart, value); }
		}

		public event EventHandler<RunControlAfterStartEventArgs> AfterStart {
			add { AddEventHandler(ref _eventHandler_AfterStart, value); }
			remove { RemoveEventHandler(ref _eventHandler_AfterStart, value); }
		}

		public event EventHandler<RunControlLogAttemptEventArgs> LogAttempt {
			add { AddEventHandler(ref _eventHandler_LogAttempt, value); }
			remove { RemoveEventHandler(ref _eventHandler_LogAttempt, value); }
		}

		public event EventHandler<RunControlEventArgs> StopRequested {
			add { AddEventHandler(ref _eventHandler_StopRequested, value); }
			remove { RemoveEventHandler(ref _eventHandler_StopRequested, value); }
		}

		void P_FireBeforeStart(RunControlBeforeStartEventArgs eventArgs) {
			eventArgs.EnsureNotNull(nameof(eventArgs));
			//
			ReadDA(ref _eventHandler_BeforeStart)?.Invoke(this, eventArgs);
		}

		void P_TryFireAfterStartAsynchronously(IRunControlAttemptSuccess result) {
			result.EnsureNotNull(nameof(result));
			// Если для объекта поступил запрос выгрузки, событие не генерируется. См. код запуска. Данный метод вызывается асинхронно.
			//
			var eventHandler = TryReadDA(location: ref _eventHandler_AfterStart, considerDisposeRequest: true);
			if (!(eventHandler is null))
				TaskUtilities.RunOnDefaultScheduler(action: () => eventHandler(sender: this, e: new RunControlAfterStartEventArgs(result: result)));
		}

		void P_TryFireStopRequestedAsynchronously() {
			var eventHandler = TryReadDA(ref _eventHandler_StopRequested);
			if (!(eventHandler is null))
				TaskUtilities.RunOnDefaultScheduler(action: () => eventHandler(sender: this, e: new RunControlEventArgs(runControl: this)));
		}

		internal void TryFireLogAttemptAsynchronously(IRunControlAttemptLoggingData data) {
			data.EnsureNotNull(nameof(data));
			//
			var eventHandler = TryReadDA(ref _eventHandler_LogAttempt);
			if (!(eventHandler is null))
				TaskUtilities.RunOnDefaultScheduler(action: locState => eventHandler(sender: locState.sender, e: new RunControlLogAttemptEventArgs(data: locState.data)), state: (sender: this, data));
		}

		IContext P_CreateStartCtx(RunControlStartOperationState<TComponent> startOp, IContext outerCtx = default) {
			startOp.EnsureNotNull(nameof(startOp));
			//
			var stopToken = ReadDA(ref _stopToken);
			CancellationToken linkedCt;
			CancellationTokenSource linkedCts = default;
			IDisposeRegistry disposeRegistry = default;
			try {
				linkedCt = CancellationTokenUtilities.LinkedOrNew(ct1: stopToken, ct2: outerCtx.Ct(), cts: out linkedCts);
				disposeRegistry = new DisposeRegistry(disposable: linkedCts);
				startOp.SetStopRequestTokenSource(cts: linkedCts);
				return ContextUtilities.Create(outerCtx: outerCtx, disposeRegistry: disposeRegistry.ToValueHolder(ownsValue: true).ArgPlaceholder(), ct: linkedCt);
			}
			catch {
				disposeRegistry?.Dispose();
				linkedCts?.Dispose();
				throw;
			}
		}

		// TODO: Put strings into the resources.
		//
		public async Task WaitStartCompletionAsync(TaskCreationOptions options = default, IContext ctx = default) {
			var component = Component;
			var existingStartOp = ReadDA(ref _existingStartOp);
			if (existingStartOp is null)
				throw new EonException(message: $"The start of this component has not yet been attempted.{Environment.NewLine}\tComponent:{component.FmtStr().GNLI2()}");
			else {
				var ct = ctx.Ct();
				var existingStartOpAwaitable = existingStartOp.Completion;
				for (; ; ) {
					if (existingStartOpAwaitable.IsSucceeded())
						break;
					else if (existingStartOpAwaitable.IsCompleted)
						throw
							new EonException(
								message: $"An attempt to start this component not succeeded. {(existingStartOp.Completion.IsCanceled ? "Attempt canceled." : "Attempt failed.")}{Environment.NewLine}\tComponent:{component.FmtStr().GNLI2()}");
					else
						await existingStartOpAwaitable.WaitCompletionAsync(options: options, ct: ct).ConfigureAwait(false);
				}
			}
		}

		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose) {
				_stopTokenRegistration.Dispose();
				StopAsync(finiteStop: true).WaitWithTimeout();
			}
			//
			base.FireBeforeDispose(explicitDispose);
		}

		protected override void Dispose(bool explicitDispose) {
			// _existingStopOp — очистка этой переменной не производится (см. код операции остановки).
			//
			_attemptStateFactory = null;
			_beforeStartTaskFactory = null;
			_component = null;
			_eventHandler_AfterStart = null;
			_eventHandler_BeforeStart = null;
			_eventHandler_StopRequested = null;
			_eventHandler_LogAttempt = null;
			_existingStartOp = null;
			_startTaskFactory = null;
			_stopTaskFactory = null;
			_stopToken = default;
			_stopTokenRegistration = default;
			//
			base.Dispose(explicitDispose);
		}

	}

}