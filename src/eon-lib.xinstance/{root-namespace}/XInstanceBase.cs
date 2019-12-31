using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel.Dependencies;
using Eon.Context;
using Eon.Description;
using Eon.Runtime.Options;
using Eon.Threading;
using Eon.Threading.Tasks;

using itrlck = Eon.Threading.InterlockedUtilities;
using vlt = Eon.Threading.VolatileUtilities;

namespace Eon {

	[DebuggerDisplay("{ToString(),nq}")]
	public abstract partial class XInstanceBase
		:DependencySupport, IXInstance {

		#region Static members

		internal static readonly Type Internal_XInstanceBaseType = typeof(XInstanceBase);

		internal static readonly Type Internal_IXInstanceType = typeof(IXInstance);

		/// <summary>
		/// Gets the current default XInstance initialization timeout set by <see cref="XInstanceInitializationTimeoutOption"/> (see <see cref="RuntimeOptions"/>).
		/// </summary>
		public static TimeoutDuration DefaultInitializationTimeout
			=> XInstanceInitializationTimeoutOption.Require();

		#endregion

		IDescription _description;

		int _statesFlags;

		IXInstance _scope;

		ImmutableList<IXInstance> _scopedInstances;

		List<XInstanceInitialization> _initializations;

		PrimitiveSpinLock _initializationsSpinLock;

		TimeoutDuration _initializationTimeout;

		TaskCompletionSource<Nil> _initializeTask;

		protected XInstanceBase(IXInstance scope, IDescription description)
			: base(outerDependencies: scope) {
			//
			P_CtorInitializer(scope: scope, description: description);
		}

		protected XInstanceBase(IServiceProvider outerServiceProvider = default, IDependencySupport outerDependencies = default)
			: base(outerServiceProvider: outerServiceProvider, outerDependencies: outerDependencies) {
			//
			P_CtorInitializer(scope: null, description: null);
		}

		protected XInstanceBase(IDescription description, IServiceProvider outerServiceProvider = default, IDependencySupport outerDependencies = default)
			: base(outerServiceProvider: outerServiceProvider, outerDependencies: outerDependencies) {
			//
			P_CtorInitializer(scope: null, description: description);
		}

		void P_CtorInitializer(IXInstance scope, IDescription description) {
			description.Arg(nameof(description)).EnsureReadOnly().EnsureValid();
			//
			_description = description;
			_scope = scope;
			_initializationTimeout = description?.InitializationTimeout ?? DefaultInitializationTimeout;
			_initializationsSpinLock = new PrimitiveSpinLock();
			_initializations = new List<XInstanceInitialization>();
			_scopedInstances = ImmutableList<IXInstance>.Empty;
			//
			_statesFlags = (int)((description is null ? XInstanceStates.NoDescription : XInstanceStates.None) | XInstanceStates.Created);
		}

		/// <summary>
		/// Gets the flags of component's current state.
		/// <para>Dispose tolerant.</para>
		/// </summary>
		public XInstanceStates State
			=> (XInstanceStates)vlt.Read(ref _statesFlags);

		public bool HasDescription {
			get {
				EnsureCreatedState();
				return (State & XInstanceStates.NoDescription) == XInstanceStates.None;
			}
		}

		// TODO: Put strings into the resources.
		//
		public IDescription Description {
			// См. также XInstance`1.Description.
			//
			get {
				if (HasDescription)
					return ReadDA(ref _description);
				else
					throw new EonException(message: $"Component have not a settings.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
			}
		}

		// TODO: Put strings into the resources.
		//
		public IDescription DescriptionDisposeTolerant {
			get {
				if (HasDescription)
					return TryReadDA(ref _description);
				else
					throw new EonException(message: $"Компонент не имеет описания (конфигурации).{Environment.NewLine}\tКомпонент:{this.FmtStr().GNLI2()}");
			}
		}

		public IXInstance Scope
			=> ReadDA(ref _scope);

		public IXInstance ScopeDisposeTolerant
			=> TryReadDA(ref _scope);

		public IReadOnlyList<IXInstance> ScopedInstances
			=> ReadDA(ref _scopedInstances);

		public IReadOnlyList<IXInstance> ScopedInstancesDisposeTolerant
			=> TryReadDA(ref _scopedInstances) ?? ImmutableList<IXInstance>.Empty;

		#region Initialization

		// TODO: Put strings into the resources.
		//
		protected void EnsureInitialized() {
			if ((State & XInstanceStates.Initialized) == XInstanceStates.None)
				throw new EonException(message: $"Unable to perform a requested operation on this component. Component must be initialized before doing that operation. However, component not initialized yet.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
		}

		void IXInstance.EnsureInitialized()
			=> EnsureInitialized();

		// TODO: Put strings into the resources.
		//
		protected T ReadIA<T>(ref T location, bool isNotRequired = false, string locationName = default, bool considerDisposeRequest = default)
			where T : class {
			var result = ReadDA(location: ref location, considerDisposeRequest: considerDisposeRequest);
			if (result is null) {
				EnsureInitialized();
				if (!isNotRequired)
					throw
						new EonException(message: $"Specified location (var) has not initialized with a value different than '{((object)null).FmtStr().G()}'.{(string.IsNullOrEmpty(locationName) ? string.Empty : $"{Environment.NewLine}\tLocation (var) name:{locationName.FmtStr().GNLI2()}")}");
			}
			return result;
		}

		// TODO: Put strings into the resources.
		//
		public void RegisterInitialization(XInstanceInitialization initialization) {
			initialization.EnsureNotNull(nameof(initialization));
			//
			ReadDA(ref _initializationsSpinLock, considerDisposeRequest: true)
				.Invoke(
					action:
						() => {
							var locInitializations = ReadDA(ref _initializations, considerDisposeRequest: true);
							var locNotChangedInitializationsSize = locInitializations.Count;
							if (locInitializations.Contains(initialization))
								throw new EonException(message: $"Specified initialization has already been registered.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
							else
								try {
									locInitializations.Add(initialization);
									EnsureNotDisposeState(considerDisposeRequest: true);
									if ((State & XInstanceStates.InitializeSent) == XInstanceStates.InitializeSent)
										throw new EonException(message: $"Unable to register specified initialization. Initialization already started.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
								}
								catch {
									if (locNotChangedInitializationsSize < locInitializations.Count)
										locInitializations.RemoveAt(index: locInitializations.Count - 1);
									throw;
								}
						});
		}

		public TimeoutDuration InitializationTimeout {
			get {
				EnsureCreatedState();
				return ReadDA(ref _initializationTimeout);
			}
		}

		// TODO: Put strings into the resources.
		//
		public void Initialize() {
			var initializationTimeout = InitializationTimeout;
			if (initializationTimeout.IsInfinite)
				InitializeAsync().Wait();
			else if (!InitializeAsync().Wait(millisecondsTimeout: initializationTimeout.Milliseconds))
				throw
					new TimeoutException(
						message: $"Initialization was not completed within a timeout. Timeout expired.{Environment.NewLine}\tCompoenent:{this.FmtStr().GNLI2()}{Environment.NewLine}\tTimeout:{initializationTimeout.TimeSpan.ToString("c").FmtStr().GNLI2()}");
		}

		public async Task InitializeAsync()
			=> await InitializeAsync(ct: CancellationToken.None);

		public async Task InitializeAsync(CancellationToken ct) {
			if (ct.CanBeCanceled) {
				ct.ThrowExceptionIfCancellationRequested();
				using (var ctx = ContextUtilities.Create(ct: ct))
					await InitializeAsync(ctx: ctx).ConfigureAwait(false);
			}
			else
				await InitializeAsync(ctx: null).ConfigureAwait(false);
		}

		public async Task InitializeAsync(IContext ctx = default) {
			await initializeAsync(locCtx: ctx).WaitAsync(ctx: ctx).ConfigureAwait(false);
			//
			Task initializeAsync(IContext locCtx = default) {
				try {
					var locExisting = ReadDA(ref _initializeTask, considerDisposeRequest: true);
					if (locExisting is null) {
						var locNew = default(TaskCompletionSource<Nil>);
						try {
							locNew = new TaskCompletionSource<Nil>(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);
							if (itrlck.UpdateIfNullBool(location: ref _initializeTask, value: locNew, current: out locExisting)) {
								try {
									EnsureCreatedState();
									EnsureNotFaultedState();
									var locInitTask =
										TaskUtilities
										.RunOnDefaultScheduler(
											factory:
												async () => {
													await P_InitializeInnerAsync(ctx: locCtx).ConfigureAwait(false);
													EnsureNotDisposeState(considerDisposeRequest: true);
												},
											ct: locCtx.Ct());
									locInitTask
										.ContinueWith(
											continuationAction:
												locTask => {
													if (locTask.IsFaulted)
														ChangeState(newStates: XInstanceStates.Faulted);
													else
														ChangeState(newStates: XInstanceStates.Initialized);
												},
											continuationOptions: TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.NotOnCanceled);
									locInitTask.ContinueWithTryApplyResultTo(taskProxy: locNew);
								}
								catch (Exception locException) {
									if (!locNew.TrySetException(exception: locException))
										throw;
								}
							}
						}
						finally {
							if (!ReferenceEquals(objA: locExisting, objB: locNew))
								locNew?.TrySetCanceled();
						}
					}
					return locExisting.Task;
				}
				catch (Exception locException) {
					return Task.FromException(exception: locException);
				}
			}
		}

		// TODO: Put strings into the resources.
		//
		async Task P_InitializeInnerAsync(IContext ctx = default) {
			try {
				ChangeState(newStates: XInstanceStates.InitializeSent);
				var initializations = ReadDA(ref _initializationsSpinLock, considerDisposeRequest: true).Invoke(() => ReadDA(ref _initializations, considerDisposeRequest: true).ToArray());
				await ExecuteInitialization(initialization: OnInitializeAsync, ctx: ctx).ConfigureAwait(false);
				//
				for (var i = 0; i < initializations.Length; i++) {
					ctx.ThrowIfCancellationRequested();
					initializations[ i ](instance: this, ctx: ctx);
				}
			}
			catch (Exception exception) {
				if (exception.IsOperationCancellation())
					throw new OperationCanceledException(message: $"Cancellation requested during component initialization.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}", innerException: exception);
				else
					throw new EonException(message: $"Component initialization fault.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}", innerException: exception);
			}
		}

		/// <summary>
		/// Do initialization logic of this component.
		/// </summary>
		/// <param name="ctx">
		/// Operation context.
		/// </param>
		protected virtual Task OnInitializeAsync(IContext ctx = default)
			=> Task.CompletedTask;

		/// <summary>
		/// Executes initialization part of this component (<paramref name="initialization"/>, <see cref="RegisterInitialization(XInstanceInitialization)"/>).
		/// <para>This method is not intended for direct call from user code.</para>
		/// </summary>
		/// <param name="initialization">
		/// Initialization.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="ctx">
		/// Operation context.
		/// </param>
		protected virtual async Task ExecuteInitialization(Func<IContext, Task> initialization, IContext ctx = default) {
			initialization.EnsureNotNull(nameof(initialization));
			//
			await initialization(arg: ctx).ConfigureAwait(false);
		}

		#endregion

		protected internal XInstanceStates ChangeState(XInstanceStates newStates)
			=> (XInstanceStates)InterlockedUtilities.Or(ref _statesFlags, (int)newStates);

		// TODO: Put strings into the resources.
		//
		protected virtual void EnsureCreatedState() {
			if ((State & XInstanceStates.Created) == XInstanceStates.None)
				throw
					new InvalidOperationException(
						message: $"Компонент находится в состоянии, не позволяющем выполнить требуемую операцию. Процедура создания объекта еще не завершена.{Environment.NewLine}\tКомпонент:{this.FmtStr().GNLI2()}");
		}

		// TODO: Put strings into the resources.
		//
		protected virtual void EnsureNotFaultedState() {
			if ((State & XInstanceStates.Faulted) == XInstanceStates.Faulted)
				throw
					new InvalidOperationException($"Выполнение операции невозможно. Компонент находится с состоянии сбоя.{Environment.NewLine}\tКомпонент:{this.FmtStr().GNLI2()}");
		}

		public TInstance CreateScopedInstance<TInstance>(IDescription description, Type constraint = default, bool ignoreDisabilityOption = default)
			where TInstance : class, IXInstance {
			//
			CreateScopedInstance(description: description, constraint: constraint, args: ArgsTuple.Create(arg1: this, arg2: description), instance: out TInstance instance, ignoreDisabilityOption: ignoreDisabilityOption);
			return instance;
		}

		public TInstance CreateScopedInstance<TArg1, TInstance>(IDescription description, TArg1 arg1, Type constraint = default, bool ignoreDisabilityOption = default)
			where TInstance : class, IXInstance {
			CreateScopedInstance(description: description, constraint: constraint, args: ArgsTuple.Create(arg1: this, arg2: description, arg3: arg1), instance: out TInstance instance, ignoreDisabilityOption: ignoreDisabilityOption);
			return instance;
		}

		public TInstance CreateScopedInstance<TArg1, TArg2, TInstance>(IDescription description, TArg1 arg1, TArg2 arg2, Type constraint = default, bool ignoreDisabilityOption = default)
			where TInstance : class, IXInstance {
			CreateScopedInstance(description: description, constraint: constraint, args: ArgsTuple.Create(arg1: this, arg2: description, arg3: arg1, arg4: arg2), instance: out TInstance instance, ignoreDisabilityOption: ignoreDisabilityOption);
			return instance;
		}

		public TInstance CreateScopedInstance<TArg1, TArg2, TArg3, TInstance>(IDescription description, TArg1 arg1, TArg2 arg2, TArg3 arg3, Type constraint = default, bool ignoreDisabilityOption = default)
			where TInstance : class, IXInstance {
			CreateScopedInstance(description: description, constraint: constraint, args: ArgsTuple.Create(arg1: this, arg2: description, arg3: arg1, arg4: arg2, arg5: arg3), instance: out
			TInstance instance, ignoreDisabilityOption: ignoreDisabilityOption);
			return instance;
		}

		public TInstance CreateScopedInstance<TArg1, TArg2, TArg3, TArg4, TInstance>(IDescription description, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, Type constraint = default, bool ignoreDisabilityOption = default)
			where TInstance : class, IXInstance {
			CreateScopedInstance(description: description, constraint: constraint, args: ArgsTuple.Create(arg1: this, arg2: description, arg3: arg1, arg4: arg2, arg5: arg3, arg6: arg4), instance: out
			TInstance instance, ignoreDisabilityOption: ignoreDisabilityOption);
			return instance;
		}

		protected virtual void CreateScopedInstance<TInstance>(IDescription description, IArgsTuple args, out TInstance instance, Type constraint = default, bool ignoreDisabilityOption = default)
			where TInstance : class, IXInstance {
			//
			description.EnsureNotNull(nameof(description));
			constraint.Arg(nameof(constraint)).EnsureCompatible(type: typeof(TInstance));
			args.EnsureNotNull(nameof(args));
			//
			if (!ignoreDisabilityOption && description.IsDisabled())
				throw
					new EonException(message: $"Невозможно создать компонент на основе указанного описания, так как описанием отключено функциональное использование компонента (см. св-во '{nameof(IAbilityOption.IsDisabled)}').{Environment.NewLine}\tОписание:{description.FmtStr().GNLI2()}");
			TInstance createdInstance = null;
			try {
				EnsureCreatedState();
				EnsureNotDisposeState(considerDisposeRequest: true);
				createdInstance =
					DependencyUtilities
					.ResolveDependency<TInstance>(
						dependencyScope: this.RequireDependencyScope(),
						dependencyTypeConstraint: constraint,
						ensureResolution: true,
						isNewInstanceRequired: true,
						preventNewInstanceInitialization: true,
						primaryResolutionModel: description.GetDependencyScope().GetResolutionModel(),
						newInstanceFactoryArgs: args);
				createdInstance.AfterDisposed += onCreatedInstanceDisposed;
				UpdDABool(location: ref _scopedInstances, transform: locCurrent => locCurrent.Add(value: createdInstance));
				instance = createdInstance;
			}
			catch (Exception exception) {
				createdInstance?.Dispose(exception: exception);
				throw;
			}
			//
			void onCreatedInstanceDisposed(object locSender, DisposeEventArgs locEventArgs) {
				var locInstance = locSender.EnsureNotNull(nameof(locSender)).EnsureOfType<IXInstance>().Value;
				//
				for (; ; ) {
					var locCurrent = itrlck.Get(ref _scopedInstances);
					if (locCurrent?.IsEmpty != false)
						break;
					var locRemoveIndex = locCurrent.FindIndex(match: locItem => ReferenceEquals(locItem, locInstance));
					if (locRemoveIndex < 0 || itrlck.UpdateBool(location: ref _scopedInstances, value: locCurrent.RemoveAt(index: locRemoveIndex), comparand: locCurrent))
						break;
				}
			}
		}

		protected override void BuildDependencyExporter(IOuterDependencyScopeGetter outerScopeGetter, out IVh<IDependencyExporter> exporter) {
			outerScopeGetter.EnsureNotNull(nameof(outerScopeGetter));
			//
			exporter = (OuterDependencies?.GetService<IXInstanceDependencyExporterBuilder>() ?? outerScopeGetter.GetOuterScope().ResolveDependency<IXInstanceDependencyExporterBuilder>(ensureResolution: true)).BuildFor(instance: this);
		}

		protected virtual DependencyResult ExecuteDependencyResolution(IDependencyResolutionContext resolutionContext)
			=> DependencyResult.None;

		// TODO: Put strings into the resources.
		//
		public override string ToString() {
			string shortFullName;
			try {
				shortFullName = HasDescription ? Description.FullName.FmtStr().Short() : null;
			}
			catch (ObjectDisposedException) {
				shortFullName = null;
			}
			if (shortFullName is null)
				return GetType().ToString();
			else
				return $"{shortFullName}, {GetType().Name}";
		}

		string ITextViewSupport.ToShortTextView(IFormatProvider formatProvider)
			=> ToString();

		string ITextViewSupport.ToLongTextView(IFormatProvider formatProvider)
			=> ToString();

		public virtual IXInstanceStateSummary TryGetStateSummary()
			=> new XInstanceStateSummary(descriptionSummary: vlt.Read(ref _description)?.GetSummary(), state: (XInstanceStates)vlt.Read(ref _statesFlags));

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_initializationsSpinLock?.EnterAndExitLock();
				_initializations?.Clear();
			}
			_description = null;
			_initializations = null;
			_initializationsSpinLock = null;
			_initializeTask = null;
			_initializationTimeout = null;
			_scope = null;
			Interlocked.Exchange(ref _scopedInstances, null);
			//
			base.Dispose(explicitDispose);
		}

	}

}