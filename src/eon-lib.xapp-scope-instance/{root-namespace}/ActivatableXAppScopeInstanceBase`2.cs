using System;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel;
using Eon.Context;
using Eon.Description;
using Eon.Threading.Tasks;

using vlt = Eon.Threading.VolatileUtilities;

namespace Eon {

	public abstract class ActivatableXAppScopeInstanceBase<TDescription, TXAppScopeInstance>
		:XAppScopeInstanceBase<TDescription>, IActivatableXAppScopeInstance<TXAppScopeInstance>
		where TDescription : class, IDescription
		where TXAppScopeInstance : ActivatableXAppScopeInstanceBase<TDescription, TXAppScopeInstance> {

		readonly bool _isAutoActivationEnabled;

		bool _hasDeactivatedBeforeInitialization;

		IRunControl<TXAppScopeInstance> _activateControl;

		protected ActivatableXAppScopeInstanceBase(IXAppScopeInstance scope, TDescription description)
			: base(scope: scope, description: description) {
			//
			_isAutoActivationEnabled = description.IsAutoActivationEnabled();
			_hasDeactivatedBeforeInitialization = false;
		}

		public bool IsAutoActivationEnabled {
			get {
				EnsureNotDisposeState();
				return _isAutoActivationEnabled;
			}
		}

		public IRunControl<TXAppScopeInstance> ActivateControl
			=> ReadIA(ref _activateControl, locationName: nameof(_activateControl));

		protected IRunControl<TXAppScopeInstance> ActivateControlDisposeTolerant {
			get {
				var control = TryReadDA(ref _activateControl);
				if (control is null) {
					EnsureInitialized();
					if (Disposing || IsDisposed)
						return null;
					else
						throw ArgumentUtilitiesCoreL1.NewNullReferenceException(varName: nameof(_activateControl), component: this);
				}
				else
					return control;
			}
		}

		IRunControl<IXAppScopeInstance> IActivatableXAppScopeInstance.ActivateControl
			=> ActivateControl;

		protected virtual bool IsAfterActivationEnabled
			=> false;

		// TODO: Put strings into the resources.
		//
		protected override async Task OnInitializeAsync(IContext ctx = default) {
			var ct = ctx.Ct();
			if (ct.IsCancellationRequested)
				throw new OperationCanceledException(token: ct);
			else {
				IRunControl<TXAppScopeInstance> control = default;
				try {
					CreateActivateControl(control: out control);
					if (control is null)
						throw new EonException(message: $"Activation control is not created.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
					else if (!ReferenceEquals(control.Component, this))
						throw new EonException(message: $"Created activation control is not associated with this component (see property '{nameof(control)}.{nameof(control.Component)}').{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
					else if (ct.IsCancellationRequested) {
						control.Dispose();
						throw new OperationCanceledException(token: ct);
					}
					else {
						AdjustActivateControl(control: control);
						if (ct.IsCancellationRequested) {
							control.Dispose();
							throw new OperationCanceledException(token: ct);
						}
						else {
							WriteDA(ref _activateControl, control);
							if (vlt.Read(ref _hasDeactivatedBeforeInitialization))
								await control.StopAsync(finiteStop: true).ConfigureAwait(false);
						}
					}
				}
				catch (Exception exception) {
					control?.Dispose(exception);
					throw;
				}
			}
		}

		protected abstract RunControlOptions DefineDefaultOfActivateControlOptions();

		// TODO: Put strings into the resources.
		//
		protected void CreateActivateControl(out IRunControl<TXAppScopeInstance> control) {
			TXAppScopeInstance component;
			try {
				component = (TXAppScopeInstance)this;
			}
			catch (Exception exception) {
				throw
					new InvalidCastException(
						message: $"Ошибка приведения компонента к требуемому типу.{Environment.NewLine}\tКомпонент:{this.FmtStr().GNLI2()}{Environment.NewLine}\tТребуемый тип:{typeof(TXAppScopeInstance).FmtStr().GNLI2()}",
						innerException: exception);
			}
			var appRunControlOptions = App.RunControl.Options;
			CreateActivateControl(
				options: DefineDefaultOfActivateControlOptions() | (appRunControlOptions & RunControlOptions.ForbidStart),
				component: component,
				attemptState: createAttemptState,
				beforeActivate: DoBeforeActivateAsync,
				activate: DoActivateAsync,
				deactivate: DoDeactivateAsync,
				deactivationToken: App.ShutdownToken,
				control: out control);
			//
			IRunControlAttemptState createAttemptState(in RunControlAttemptStateFactoryArgs locArgs) {
				CreateActivateAttemptState(args: in locArgs, state: out var locState);
				return locState;
			}
		}

		protected virtual void CreateActivateControl(
			RunControlOptions options,
			TXAppScopeInstance component,
			RunControlAttemptStateFactory attemptState,
			Func<IRunControlAttemptState, Task> beforeActivate,
			Func<IRunControlAttemptState, Task> activate,
			Func<Task> deactivate,
			CancellationToken deactivationToken,
			out IRunControl<TXAppScopeInstance> control)
			=>
			control =
			new RunControl<TXAppScopeInstance>(
				options: options,
				component: component,
				attemptState: attemptState,
				beforeStart: beforeActivate,
				start: activate,
				stop: deactivate,
				stopToken: deactivationToken);

		protected virtual void CreateActivateAttemptState(in RunControlAttemptStateFactoryArgs args, out IRunControlAttemptState state)
			=> state = new RunControlAttemptState(args: in args);

		protected virtual void AdjustActivateControl(IRunControl<TXAppScopeInstance> control) {
			control.EnsureNotNull(nameof(control));
			//
			if (IsAfterActivationEnabled)
				control.AfterStart += afterStartEventHandler;
			//
			void afterStartEventHandler(object locSender, RunControlAfterStartEventArgs locEventArgs) {
				locSender.EnsureNotNull(nameof(locSender));
				locEventArgs.EnsureNotNull(nameof(locEventArgs));
				//
				if (ReferenceEquals(objA: locSender, objB: ActivateControlDisposeTolerant) && !HasDeactivationRequested)
					TaskUtilities.RunOnDefaultScheduler(factory: DoAfterActivateAsync, state: locEventArgs.Result, ct: ShutdownToken);
			}
		}

		protected virtual async Task DoBeforeActivateAsync(IRunControlAttemptState state) {
			state.EnsureNotNull(nameof(state));
			//
			EnsureInitialized();
			await Task.CompletedTask;
		}

		/// <summary>
		/// Выполяет активацию.
		/// <para>Метод не предназначен для прямого вызова из кода и используется элементом управления активации/деактивации <see cref="ActivateControl"/>.</para>
		/// </summary>
		/// <param name="state">Объект, представляющий состояние операции активации.</param>
		/// <returns>Объект <see cref="Task"/>.</returns>
		protected abstract Task DoActivateAsync(IRunControlAttemptState state);

		protected virtual async Task DoAfterActivateAsync(IRunControlAttemptSuccess result) {
			result.EnsureNotNull(nameof(result));
			//
			await Task.CompletedTask;
		}

		/// <summary>
		/// Выполяет деактивацию.
		/// <para>Метод не предназначен для прямого вызова из кода и используется элементом управления активации/деактивации <see cref="ActivateControl"/>.</para>
		/// </summary>
		/// <returns>Объект <see cref="Task"/>.</returns>
		protected abstract Task DoDeactivateAsync();

		public async Task<IRunControlAttemptSuccess> ActivateAsync(IContext ctx = default)
			=> await ActivateControl.StartAsync(options: TaskCreationOptions.None, ctx: ctx).ConfigureAwait(false);

		public async Task<IRunControlAttemptSuccess> ActivateAsync(TaskCreationOptions options, IContext ctx = default)
			=> await ActivateControl.StartAsync(options: options, ctx: ctx).ConfigureAwait(false);

		public async Task DeactivateAsync(IContext ctx = default) {
			var control = TryReadDA(ref _activateControl);
			if (control is null) {
				if (!IsDisposeRequested)
					vlt.Write(ref _hasDeactivatedBeforeInitialization, true);
			}
			else
				await control.StopAsync(ctx: ctx).ConfigureAwait(false);
		}

		/// <summary>
		/// Возвращает признак, указывающий на состояние функциональной активности этого компонента.
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки.</para>
		/// <para>Определяется условиями:</para>
		/// <para>• Инициализирован (см. <see cref="IXInstance.State"/>).</para>
		/// <para>• Активирован (см. <see cref="ActivateControl"/>, <see cref="IRunControl.IsStarted"/>).</para>
		/// <para>• Отсутствие запроса деактивации (см. <see cref="ActivateControl"/>, <see cref="IRunControl.HasStopRequested"/>).</para>
		/// <para>• Отсутствие запроса выгрузки (см. <see cref="IEonDisposable.IsDisposeRequested"/>).</para>
		/// </summary>
		public bool IsActive {
			get {
				var control = TryReadDA(ref _activateControl, considerDisposeRequest: true);
				return control is null ? false : !control.HasStopRequested && control.IsStarted;
			}
		}

		// TODO_HIGH: Реализацию свойтсва перенести в IComponentRunControl.
		//
		public bool HasDeactivationRequested {
			get {
				if (HasShutdownRequested)
					return true;
				else {
					var control = TryReadDA(ref _activateControl, considerDisposeRequest: true);
					if (control is null)
						return vlt.Read(ref _hasDeactivatedBeforeInitialization) || IsDisposeRequested;
					else
						return control.HasStopRequested;
				}
			}
		}

		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose) {
				TryReadDA(ref _activateControl)?.StopAsync().WaitWithTimeout();
			}
			//
			base.FireBeforeDispose(explicitDispose);
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_activateControl?.Dispose();
			}
			_activateControl = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}