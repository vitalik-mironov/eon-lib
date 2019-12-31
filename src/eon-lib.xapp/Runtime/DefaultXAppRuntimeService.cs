using System;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel.Dependencies;
using Eon.Context;
using Eon.Description;
using Eon.Internal;
using Eon.Metadata;
using Eon.Runtime.Description;
using Eon.Runtime.Options;
using Eon.Threading;
using Eon.Threading.Tasks;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Runtime {
	using IRuntimeXApp = IRuntimeXApp<IRuntimeXAppDescription>;

	public partial class DefaultXAppRuntimeService
		:DependencySupport, IXAppRuntimeService {

		CancellationTokenSource _disposeCts;

		SemaphoreSlim _supplyRuntimeAppAsyncLock;

		IRuntimeXApp _runtimeApp;

		MetadataName _runtimeAppSatelliteDescriptionPackageName;

		MetadataPathName _runtimeAppSatelliteDescriptionPath;

		public DefaultXAppRuntimeService(IServiceProvider outerServiceProvider = default, IDependencySupport outerDependencies = default)
			: base(outerServiceProvider: outerServiceProvider, outerDependencies: outerDependencies) {
			_disposeCts = new CancellationTokenSource();
			_supplyRuntimeAppAsyncLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
		}

		public virtual bool IsRuntimeAppSupplied
			=> ReadDA(ref _runtimeApp) != null;

		// TODO: Put strings into the resources.
		//
		public virtual IRuntimeXApp RuntimeApp {
			get {
				var runtimeApp = ReadDA(ref _runtimeApp);
				if (runtimeApp is null)
					throw new EonException($"Runtime app has not been supplied.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
				else
					return runtimeApp;
			}
		}

		protected MetadataName RuntimeAppSatelliteDescriptionPackageName
			=> InitDA(location: ref _runtimeAppSatelliteDescriptionPackageName, factory: RequireRuntimeAppSatelliteDescriptionPackageName);

		protected MetadataPathName RuntimeAppSatelliteDescriptionPath
			=> InitDA(location: ref _runtimeAppSatelliteDescriptionPath, factory: RequireRuntimeAppSatelliteDescriptionPath);

		protected MetadataName RequireRuntimeAppSatelliteDescriptionPackageName()
			=> RuntimeAppSatelliteDescriptionPackageNameOption.Require();

		protected MetadataPathName RequireRuntimeAppSatelliteDescriptionPath()
			=> RuntimeAppSatelliteDescriptionPathOption.Require();

		public virtual IRuntimeXApp GetRuntimeAppIfSupplied()
			=> ReadDA(ref _runtimeApp);

		public async Task<UpdateResult<IRuntimeXApp>> SupplyRuntimeAppAsync(IXAppStartupContext<IRuntimeXAppDescription> startupCtx, XAppStartupContextHostHints? overrideStartupHints = default) {
			var trySupplyResult = await TrySupplyRuntimeAppAsync(startupCtx: startupCtx, overrideStartupHints: overrideStartupHints).ConfigureAwait(false);
			return P_HandleRuntimeAppSupplyTryResult(result: trySupplyResult);
		}

		public async Task<UpdateResult<IRuntimeXApp>> SupplyRuntimeAppAsync(IXAppStartupContext<ICustomXAppDescription> startupCtx, XAppStartupContextHostHints startupHints) {
			var trySupplyResult = await TrySupplyRuntimeAppAsync(startupCtx: startupCtx, startupHints: startupHints).ConfigureAwait(false);
			return P_HandleRuntimeAppSupplyTryResult(trySupplyResult);
		}

		// TODO: Put strings into the resources.
		//
		UpdateResult<IRuntimeXApp> P_HandleRuntimeAppSupplyTryResult(UpdateResult<IRuntimeXApp> result) {
			result.EnsureNotNull(nameof(result));
			//
			if (result.IsUpdated || result.IsProposedSameAsOriginal)
				return result;
			else
				throw
					new EonException(
						$"Невозможно установить приложение поставщика среды Lime. Ранее уже было установлено другое приложение.{Environment.NewLine}\tПоставщик среды Lime:{this.FmtStr().GNLI2()}{Environment.NewLine}\tРанее установленное приложение:{result.Current.FmtStr().GNLI2()}");
		}

		// TODO: Put strings into the resources.
		//
		public virtual async Task<UpdateResult<IRuntimeXApp>> TrySupplyRuntimeAppAsync(IXAppStartupContext<IRuntimeXAppDescription> startupCtx, XAppStartupContextHostHints? overrideStartupHints = default) {
			startupCtx.EnsureNotNull(nameof(startupCtx));
			//
			var effectiveStartupContextHostHints = overrideStartupHints ?? startupCtx.HostHints;
			var linkedCts = default(CancellationTokenSource);
			try {
				var locCt = CancellationTokenUtilities.SingleOrLinked(ct1: ReadDA(ref _disposeCts, considerDisposeRequest: true).Token, ct2: startupCtx.Ct(), linkedCts: out linkedCts);
				using (var locCtx = startupCtx.New(ct: locCt)) {
					// Получить описание приложения.
					//
					var appDescription = await startupCtx.LoadDescriptionAsync(ctx: locCtx).Unwrap().ConfigureAwait(false);
					if (appDescription is null)
						throw new EonException(message: $"Method '{nameof(startupCtx.LoadDescriptionAsync)}' of startup context has returned invalid value '{appDescription.FmtStr().G()}'.{Environment.NewLine}\tStartup context:{startupCtx.FmtStr().GNLI2()}");
					//
					var existingApp = ReadDA(location: ref _runtimeApp);
					if (existingApp is null) {
						var supplyRuntimeAppAsyncLock = ReadDA(location: ref _supplyRuntimeAppAsyncLock, considerDisposeRequest: true);
						var lckAcquired = false;
						try {
							lckAcquired = await ReadDA(ref _supplyRuntimeAppAsyncLock).WaitAsync(millisecondsTimeout: TaskUtilities.DefaultAsyncTimeoutMilliseconds, cancellationToken: locCt).ConfigureAwait(false);
							if (!lckAcquired)
								throw new LockAcquisitionFailException(reason: LockAcquisitionFailReason.TimeoutElapsed);
							//
							existingApp = ReadDA(ref _runtimeApp, considerDisposeRequest: true);
							if (existingApp is null) {
								var newApp = default(IRuntimeXApp);
								try {
									// Создать экземпляр приложения.
									//
									newApp =
										await
										CreateAppAsync<IRuntimeXAppDescription, IRuntimeXApp>(args: new XAppCtorArgs<IRuntimeXAppDescription>(description: appDescription, containerControl: startupCtx.ContainerControl, outerDependencies: this), ctx: locCtx)
										.ConfigureAwait(false);
									newApp.AfterDisposed += P_EH_RuntimeApp_AfterDisposed;
									WriteDA(location: ref _runtimeApp, value: newApp);
									await XAppInternalUtilities.FollowStartupContextHostHintsAsync(hints: effectiveStartupContextHostHints, app: newApp, ctx: locCtx).ConfigureAwait(false);
									return new UpdateResult<IRuntimeXApp>(current: newApp, original: null);
								}
								catch (Exception exception) {
									Exception rethrowException;
									try {
										itrlck.SetNull(location: ref _runtimeApp, comparand: newApp);
										if (!(newApp is null))
											await newApp.ShutdownAppAsync().ConfigureAwait(false);
										rethrowException = new AggregateException(exception);
									}
									catch (Exception secondException) {
										rethrowException = new AggregateException(exception, secondException);
									}
									throw rethrowException;
								}
							}
							else if (ReferenceEquals(objA: appDescription, objB: existingApp.Description)) {
								await XAppInternalUtilities.FollowStartupContextHostHintsAsync(hints: effectiveStartupContextHostHints, app: existingApp, ctx: locCtx).ConfigureAwait(false);
								return new UpdateResult<IRuntimeXApp>(current: existingApp, original: existingApp, isProposedSameAsOriginal: true);
							}
							else
								return new UpdateResult<IRuntimeXApp>(current: existingApp, original: existingApp, isProposedSameAsOriginal: false);

						}
						finally {
							if (lckAcquired)
								try { supplyRuntimeAppAsyncLock.Release(); }
								catch (ObjectDisposedException) { }
						}
					}
					else if (ReferenceEquals(objA: appDescription, objB: existingApp.Description)) {
						await XAppInternalUtilities.FollowStartupContextHostHintsAsync(hints: effectiveStartupContextHostHints, app: existingApp, ctx: locCtx).ConfigureAwait(false);
						return new UpdateResult<IRuntimeXApp>(current: existingApp, original: existingApp, isProposedSameAsOriginal: true);
					}
					else
						return new UpdateResult<IRuntimeXApp>(current: existingApp, original: existingApp, isProposedSameAsOriginal: false);
				}
			}
			finally {
				linkedCts?.Dispose();
			}
		}

		public virtual async Task<UpdateResult<IRuntimeXApp>> TrySupplyRuntimeAppAsync(IXAppStartupContext<ICustomXAppDescription> startupCtx, XAppStartupContextHostHints startupHints) {
			startupCtx.EnsureNotNull(nameof(startupCtx));
			var customAppLocator = startupCtx.DescriptionLocator.ArgProp($"{nameof(startupCtx)}.{nameof(startupCtx.DescriptionLocator)}").EnsureNotNull().Value;
			//
			var runtimeAppLocator = CreateRuntimeAppSatelliteDescriptionLocator(locator: customAppLocator);
			var runtimeAppStartupCtx = default(IXAppStartupContext<IRuntimeXAppDescription>);
			try {
				runtimeAppStartupCtx = await CreateAppStartupContextAsync<IRuntimeXAppDescription>(startupHints: startupHints, locator: runtimeAppLocator, outerCtx: startupCtx);
				return await TrySupplyRuntimeAppAsync(startupCtx: runtimeAppStartupCtx).ConfigureAwait(false);
			}
			catch (Exception exception) {
				if (runtimeAppStartupCtx is null)
					throw
						new EonException(
							message: $"Runtime app setup fail.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}{Environment.NewLine}\tApp startup context:{startupCtx.FmtStr().GNLI2()}{Environment.NewLine}\tRuntime app description locator (reference):{runtimeAppLocator.FmtStr().GNLI2()}",
							innerException: exception);
				else
					throw
						new EonException(
							message: $"Runtime app setup fail.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}{Environment.NewLine}\tApp startup context:{startupCtx.FmtStr().GNLI2()}{Environment.NewLine}\tRuntime app startup context:{runtimeAppStartupCtx.FmtStr().GNLI2()}",
							innerException: exception);
			}
			finally {
				runtimeAppStartupCtx?.Dispose();
			}
		}

		void P_EH_RuntimeApp_AfterDisposed(object sender, DisposeEventArgs e) {
			sender.EnsureNotNull(nameof(sender));
			//
			itrlck.SetNull(location: ref _runtimeApp, comparand: sender as IRuntimeXApp);
		}

		public virtual async Task<TApp> CreateAppAsync<TDescription, TApp>(IXAppCtorArgs<TDescription> args, IContext ctx = default)
			where TDescription : class, IXAppDescription
			where TApp : class, IXApp<TDescription> {
			//
			await Task.CompletedTask;
			args.EnsureNotNull(nameof(args));
			var description = args.Description.ArgProp($"{nameof(args)}.{nameof(args.Description)}").EnsureNotNull().Value;
			//
			return
				this
				.ResolveDependency<TApp>(
					ensureResolution: true,
					isNewInstanceRequired: true,
					preventNewInstanceInitialization: true,
					primaryResolutionModel: description.GetDependencyScope().GetResolutionModel(),
					newInstanceFactoryArgs: ArgsTuple.Create(arg1: args),
					// По умолчанию созданный в ходе разрешения зависимости экземпляр помещается в реестр выгружаемых объектов указанной области (см. dependencyScope: locCtx.Dependencies()).
					// Параметр disposeRegistry переопределяет реестр, в который помещается экземпляр.
					// Здесь используется NOP-реестр с тем, чтобы избежать выгрузки созданного экземпляра приложения, так как за выгрузку этого экземпляра отвечает вызывающий код.
					//
					disposeRegistry: NopDisposeRegistry.Instance);
		}

		public virtual async Task<IXAppStartupContext<TDescription>> CreateAppStartupContextAsync<TDescription>(
			DescriptionLocator locator = default,
			TDescription description = default,
			ArgumentPlaceholder<XAppStartupContextHostHints> startupHints = default,
			ArgumentPlaceholder<IXAppContainerControl> containerControl = default,
			IContext outerCtx = default)
			where TDescription : class, IXAppDescription {
			//
			await Task.CompletedTask;
			if (locator is null)
				description.EnsureNotNull(nameof(description));
			else
				description.Arg(nameof(description)).EnsureIsNull();
			//
			var createdCtx = default(IXAppStartupContext<TDescription>);
			try {
				if (locator is null)
					createdCtx = new XAppStartupContext<TDescription>(hostHints: startupHints.Substitute(XAppStartupContextHostHints.None), description: description, containerControl: containerControl, outerCtx: outerCtx);
				else
					createdCtx = new XAppStartupContext<TDescription>(hostHints: startupHints.Substitute(XAppStartupContextHostHints.None), locator: locator, containerControl: containerControl, outerCtx: outerCtx);
				createdCtx.Set(prop: DependencyContextProps.DependenciesProp, value: this.ToValueHolder(ownsValue: false));
				return createdCtx;
			}
			catch (Exception exception) {
				createdCtx?.Dispose(exception: exception);
				throw;
			}
		}

		public virtual DescriptionLocator CreateRuntimeAppSatelliteDescriptionLocator(DescriptionPackageLocator locator) {
			locator.EnsureNotNull(nameof(locator));
			//
			var svc = this.RequireService<IDescriptionPackageService>();
			return svc.CreateSatelliteDescriptionLocator(locator: locator, satelliteName: RuntimeAppSatelliteDescriptionPackageName, satelliteDescriptionPath: RuntimeAppSatelliteDescriptionPath);
		}

		protected override void FireBeforeDispose(bool explicitDispose) {
			if (explicitDispose) {
				var disposeCts = TryReadDA(ref _disposeCts);
				if (disposeCts?.IsCancellationRequested == false)
					disposeCts.Cancel();
				TryReadDA(ref _runtimeApp)?.ShutdownAppAsync().WaitWithTimeout();
			}
			//
			base.FireBeforeDispose(explicitDispose);
		}

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_supplyRuntimeAppAsyncLock?.Dispose();
				_runtimeApp?.Dispose();
				_disposeCts?.Dispose();
			}
			_supplyRuntimeAppAsyncLock = null;
			_runtimeApp = null;
			_runtimeAppSatelliteDescriptionPackageName = null;
			_runtimeAppSatelliteDescriptionPath = null;
			_disposeCts = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}