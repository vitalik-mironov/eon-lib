using System;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel.Dependencies;
using Eon.Context;
using Eon.Description;
using Eon.Threading;

namespace Eon {
	using IXApp = IXApp<IXAppDescription>;

	public abstract class XAppScopeInstanceBase
		:XInstanceBase<IDescription>, IXAppScopeInstance {

		#region Nested types

		protected delegate IContext ScopedContextFactory(
			object localTag = default,
			ArgumentPlaceholder<CancellationToken> ct = default,
			ArgumentPlaceholder<IVh<IDisposeRegistry>> disposeRegistry = default,
			ArgumentPlaceholder<IDependencySupport> dependencies = default,
			IContext outerCtx = default);

		#endregion

		IXApp<IXAppDescription> _app;

		protected XAppScopeInstanceBase(IXApp app, IDescription description)
			: this(scope: app.EnsureNotNull(nameof(app)).Value, description: description.EnsureNotNull(nameof(description)).Value, app: app) { }

		protected XAppScopeInstanceBase(IXAppScopeInstance scope, IDescription description)
			: this(scope: scope.EnsureNotNull(nameof(scope)).Value, description: description.EnsureNotNull(nameof(description)).Value, app: scope.App.EnsureNotNull($"{nameof(scope)}.{nameof(scope.App)}").Value) { }

		XAppScopeInstanceBase(IXInstance scope, IDescription description, IXApp<IXAppDescription> app)
			: base(scope: scope, description: description) {
			_app = app;
		}

		public IXApp<IXAppDescription> App
			=> ReadDA(ref _app);

		public IXApp<IXAppDescription> AppDisposeTolerant
			=> TryReadDA(ref _app);

		/// <summary>
		/// Возвращает признак, указывающий на наличие запроса остановки и выгрузки компонента.
		/// <para>Возвращается <see langword="true"/>, если выполняется одно из условий:</para>
		/// <para>• <see cref="App"/>: <see cref="IXApp{TDescription}.HasAppShutdownRequested"/>  == <see langword="true"/>;</para>
		/// <para>• <see cref="App"/> == <see langword="null"/> и <see cref="IOxyDisposable.IsDisposeRequested"/> == <see langword="true"/>.</para>
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки.</para>
		/// </summary>
		public bool HasShutdownRequested {
			get {
				var app = TryReadDA(ref _app);
				if (app is null) {
					if (IsDisposeRequested)
						return true;
					else
						throw ArgumentUtilitiesCoreL1.NewNullReferenceException(varName: nameof(_app), component: this);
				}
				else
					return app.HasAppShutdownRequested;
			}
		}

		/// <summary>
		/// Возвращает токен отмены, который переходит в сигнальное состоянии при запросе остановки и выгрузки приложения (см. <seealso cref="App"/>, <see cref="IXApp{TDescription}.ShutdownAppAsync"/>).
		/// <para>Обращение к свойству вызовет исключение <seealso cref="InvalidOperationException"/>, если свойство <see cref="IXApp{TDescription}.AppShutdownToken"/> еще не инициализировано.</para>
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки.</para>
		/// </summary>
		public CancellationToken ShutdownToken {
			get {
				var app = TryReadDA(ref _app);
				if (app is null) {
					if (IsDisposeRequested)
						return CancellationTokenUtilities.Canceled;
					else
						throw ArgumentUtilitiesCoreL1.NewNullReferenceException(varName: nameof(_app), component: this);
				}
				else
					return app.AppShutdownToken;
			}
		}

		/// <summary>
		/// Executes initialization part of this component (<paramref name="initialization"/>, <see cref="XInstanceBase.RegisterInitialization(XInstanceInitialization)"/>).
		/// <para>This method is not intended for direct call from user code.</para>
		/// </summary>
		/// <param name="initialization">
		/// Initialization.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="ctx">
		/// Operation context.
		/// </param>
		protected override async Task ExecuteInitialization(Func<IContext, Task> initialization, IContext ctx = default) {
			var linkedCts = default(CancellationTokenSource);
			try {
				var shutdownCt = ShutdownToken;
				var ctxCt = ctx.Ct();
				if (shutdownCt == ctxCt)
					await base.ExecuteInitialization(initialization: initialization, ctx: ctx).ConfigureAwait(false);
				else {
					var linkedCt = CancellationTokenUtilities.SingleOrLinked(ct1: ctxCt, ct2: ShutdownToken, linkedCts: out linkedCts);
					if (linkedCt == ctxCt)
						await base.ExecuteInitialization(initialization: initialization, ctx: ctx).ConfigureAwait(false);
					else
						using (var locCtx = ContextUtilities.Create(outerCtx: ctx, ct: linkedCt))
							await base.ExecuteInitialization(initialization: initialization, ctx: locCtx).ConfigureAwait(false);
				}
			}
			finally {
				linkedCts?.Dispose();
			}
		}

		protected virtual IContext CreateScopedContext(
			CancellationToken ct = default,
			bool notLinkAppShutdownToken = false,
			object localTag = default,
			IContext outerCtx = default,
			ArgumentPlaceholder<IDependencySupport> dependencies = default,
			ScopedContextFactory factory = default) {
			//
			if (factory is null)
				factory =
					delegate (
						object locLocalTag,
						ArgumentPlaceholder<CancellationToken> locCt,
						ArgumentPlaceholder<IVh<IDisposeRegistry>> locDisposeRegistry,
						ArgumentPlaceholder<IDependencySupport> locDependencies,
						IContext locOuterCtx) {
							var locCreatedCtx = default(IContext);
							try {
								locCreatedCtx = locOuterCtx.New(localTag: locLocalTag, ct: locCt, disposeRegistry: locDisposeRegistry);
								if (locDependencies.HasExplicitValue)
									locCreatedCtx.Set(prop: DependencyContextProps.DependenciesProp, value: locDependencies.ExplicitValue.ToValueHolder(ownsValue: false).Arg(nameof(locDependencies)));
								return locCreatedCtx;
							}
							catch (Exception locException) {
								locCreatedCtx?.Dispose(exception: locException);
								throw;
							}
						};
			CancellationTokenSource linkedCts = default;
			IDisposeRegistry disposeRegistry = default;
			try {
				ct =
					CancellationTokenUtilities
					.SingleOrLinked(
						ct1: ct,
						ct2: outerCtx.Ct(),
						ct3: notLinkAppShutdownToken ? CancellationToken.None : (AppDisposeTolerant?.AppShutdownToken ?? CancellationToken.None),
						linkedCts: out linkedCts);
				if (!(linkedCts is null))
					disposeRegistry = new DisposeRegistry(disposable: linkedCts);
				return
					factory(
						localTag: localTag,
						ct: ct,
						disposeRegistry: disposeRegistry is null ? default : disposeRegistry.ToValueHolder(ownsValue: true).ArgPlaceholder(),
						dependencies: dependencies.HasExplicitValue ? dependencies : ((IDependencySupport)this).ArgPlaceholder(),
						outerCtx: outerCtx);
			}
			catch (Exception exception) {
				disposeRegistry?.Dispose(exception);
				linkedCts?.Dispose(exception);
				throw;
			}
		}

		protected override void Dispose(bool explicitDispose) {
			_app = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}