#define DO_NOT_USE_OXY_LOGGING_API

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Eon.Collections;
#if !DO_NOT_USE_OXY_LOGGING_API

using DigitalFlare.Diagnostics.Logging;

#endif
using Eon.Linq;
using Eon.Reflection;
using Eon.Text;
using Eon.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Eon.ComponentModel.Dependencies {

	[DebuggerDisplay("{ToString(),nq}")]
	public class DependencyScope
		:Disposable, IDependencyScope {

		#region Nested types

		abstract class P_ResolveDependencyInvokerBase {

			internal P_ResolveDependencyInvokerBase() { }

			public abstract bool Invoke(DependencyScope scope, IDependencyResolutionContext ctx, out object dependency);

		}

		sealed class P_ResolveDependencyInvoker<TDependency>
			:P_ResolveDependencyInvokerBase
			where TDependency : class {

			internal P_ResolveDependencyInvoker() { }

			public override bool Invoke(DependencyScope scope, IDependencyResolutionContext ctx, out object dependency) {
				scope.EnsureNotNull(nameof(scope));
				//
				var result = scope.ResolveDependency(ctx, out TDependency locDependency);
				dependency = locDependency;
				return result;
			}

		}

		sealed class P_ServiceProviderApi
			:IServiceProvider {

			readonly DependencyScope _scope;

			internal P_ServiceProviderApi(DependencyScope scope) {
				scope.EnsureNotNull(nameof(scope));
				//
				_scope = scope;
			}

			public object GetService(Type serviceType)
				=> _scope.GetService(serviceType: serviceType);

		}

		#endregion

		#region Static members

		static readonly IDictionary<Type, P_ResolveDependencyInvokerBase> __ResolveDependencyInvokers;

		static readonly PrimitiveSpinLock __ResolveDependencyInvokeRepoSpinLock;

		// TODO_HIGH: Get value of 'DefaultOfMaxCountOfRunningResolutions' from IDependencyResolutionSettings.
		//
		/// <summary>
		/// Value: defines as 'Math.Min(Math.Max(24, Environment.ProcessorCount), 48)'.
		/// </summary>
		public static readonly int DefaultOfMaxCountOfRunningResolutions;

		// TODO_HIGH: Get value of 'DefaultOfMaxCountOfResolutionRedirect' from IDependencyResolutionSettings.
		//
		/// <summary>
		/// Defines the default limit of max. count of the dependency handler redirects (per one dependency scope).
		/// <para>Any involved handler can return the result as a redirect to other handler (see <see cref="DependencyResult.RedirectHandler"/>, <see cref="DependencyResult{DependencyRequestResult}.RedirectHandler"/>). This limit intended to prevent the infinite count of that redirects.</para>
		/// <para>Value: '16'.</para>
		/// </summary>
		public static readonly int DefaultOfMaxCountOfResolutionRedirect;

		static DependencyScope() {
			DefaultOfMaxCountOfRunningResolutions = Math.Min(Math.Max(24, Environment.ProcessorCount), 48);
			DefaultOfMaxCountOfResolutionRedirect = 16;
			__ResolveDependencyInvokeRepoSpinLock = new PrimitiveSpinLock();
			__ResolveDependencyInvokers = new Dictionary<Type, P_ResolveDependencyInvokerBase>();
		}

		#endregion

		IVh<IOuterDependencyScopeGetter> _outerScopeGetter;

		IVh<IDependencyExporter> _exporter;

		DisposableLazy<IDependencyResolutionModel<DependencyScope>> _resolutionModelLazy;

		readonly bool _prohibitNewInstanceRequest;

		IDisposeRegistry _newDependencyInstancesDisposeRegistry;

		ISet<IDependencyResolutionContext> _runningResolutionsUniqueness;

		List<IDependencyResolutionContext> _runningResolutionsSequence;

		PrimitiveSpinLock _runningResolutionsSpinLock;

		IServiceProvider _serviceProviderApi;

		object _owner;

		public DependencyScope(IDependencyScope outerScope, IDependencyExporter exporter, bool ownsExporter = default, bool prohibitNewInstanceRequest = default, object owner = default) {
			_outerScopeGetter = new DefaultOuterDependencyScopeGetter(outerScope: outerScope).ToValueHolder(ownsValue: true);
			_prohibitNewInstanceRequest = prohibitNewInstanceRequest;
			P_CtorInitializer(exporter: exporter, ownsDependencyExporter: ownsExporter, owner: owner);
		}

		public DependencyScope(IOuterDependencyScopeGetter outerScopeGetter, IDependencyExporter exporter, bool ownsExporter = default, bool prohibitNewInstanceRequest = default, object owner = default) {
			outerScopeGetter.EnsureNotNull(nameof(outerScopeGetter));
			//
			_outerScopeGetter = outerScopeGetter.ToValueHolder(ownsValue: false);
			_prohibitNewInstanceRequest = prohibitNewInstanceRequest;
			P_CtorInitializer(exporter: exporter, ownsDependencyExporter: ownsExporter, owner: owner);
		}

		public DependencyScope(IServiceProvider outerServiceProvider)
			: this(outerServiceProvider: outerServiceProvider.Arg(nameof(outerServiceProvider))) { }

		public DependencyScope(ArgumentUtilitiesHandle<IServiceProvider> outerServiceProvider) {
			outerServiceProvider.EnsureNotNull();
			//
			_outerScopeGetter = new DefaultOuterDependencyScopeGetter(outerScope: null).ToValueHolder(ownsValue: true);
			_prohibitNewInstanceRequest = false;
			var outerServiceProviderHandler = new ServiceProviderHandler(serviceProvider: outerServiceProvider);
			P_CtorInitializer(
				exporter: new DefaultDependencyExporter(handler: outerServiceProviderHandler, ownsHandler: true), 
				ownsDependencyExporter: true, 
				outerServiceProvider: outerServiceProviderHandler.ServiceProvider,
				owner: null);
		}

		public DependencyScope(IServiceProviderHandler outerServiceProvider, bool ownsOuterServiceProvider = default)
			: this(outerServiceProvider: outerServiceProvider.Arg(nameof(outerServiceProvider)), ownsOuterServiceProvider: ownsOuterServiceProvider) { }

		public DependencyScope(ArgumentUtilitiesHandle<IServiceProviderHandler> outerServiceProvider, bool ownsOuterServiceProvider = default) {
			outerServiceProvider.EnsureNotNull();
			//
			_outerScopeGetter = new DefaultOuterDependencyScopeGetter(outerScope: null).ToValueHolder(ownsValue: true);
			_prohibitNewInstanceRequest = false;
			P_CtorInitializer(
				exporter: new DefaultDependencyExporter(handler: outerServiceProvider.Value, ownsHandler: ownsOuterServiceProvider),
				ownsDependencyExporter: true, 
				outerServiceProvider: outerServiceProvider.Value.ServiceProvider, 
				owner: null);
		}

		public DependencyScope(IServiceScope outerServiceScope, bool ownsOuterServiceScope = default)
			: this(outerServiceScope: outerServiceScope.Arg(nameof(outerServiceScope)), ownsOuterServiceScope: ownsOuterServiceScope) { }

		public DependencyScope(ArgumentUtilitiesHandle<IServiceScope> outerServiceScope, bool ownsOuterServiceScope = default) {
			outerServiceScope.EnsureNotNull();
			//
			_outerScopeGetter = new DefaultOuterDependencyScopeGetter(outerScope: null).ToValueHolder(ownsValue: true);
			_prohibitNewInstanceRequest = false;
			var outerServiceProviderHandler = new ServiceProviderHandler(serviceScope: outerServiceScope, ownsServiceScope: ownsOuterServiceScope);
			P_CtorInitializer(
				exporter: new DefaultDependencyExporter(handler: outerServiceProviderHandler, ownsHandler: true),
				ownsDependencyExporter: true,
				outerServiceProvider: outerServiceProviderHandler.ServiceProvider,
				owner: null);
		}

		void P_CtorInitializer(IDependencyExporter exporter, bool ownsDependencyExporter = default, IServiceProvider outerServiceProvider = default, object owner = default) {
			exporter.EnsureNotNull(nameof(exporter));
			//
			_exporter = exporter.ToValueHolder(ownsDependencyExporter);
			_owner = owner;
			//
			_resolutionModelLazy =
				new DisposableLazy<IDependencyResolutionModel<DependencyScope>>(
					factory: () => new DependencyResolutionModel<DependencyScope>(scope: this, handlerChainSource: ReadDA(ref _exporter, considerDisposeRequest: true).Value.ExportDependencies()),
					ownsValue: true);
			//
			_newDependencyInstancesDisposeRegistry = new DisposeRegistry();
			//
			_runningResolutionsSpinLock = new PrimitiveSpinLock();
			_runningResolutionsUniqueness = new HashSet<IDependencyResolutionContext>(comparer: ReferenceEqualityComparer<IDependencyResolutionContext>.Instance);
			_runningResolutionsSequence = new List<IDependencyResolutionContext>();
			//
			_serviceProviderApi = outerServiceProvider ?? new P_ServiceProviderApi(scope: this);
		}

		protected IOuterDependencyScopeGetter OuterScopeGetter
			=> ReadDA(ref _outerScopeGetter).Value;

		public IDependencyScope GetOuterScope()
			=> ReadDA(ref _outerScopeGetter).Value.GetOuterScope();

		public IDependencyExporter Exporter
			=> ReadDA(ref _exporter).Value;

		public bool ProhibitNewInstanceRequest {
			get {
				EnsureNotDisposeState();
				return _prohibitNewInstanceRequest;
			}
		}

		public int MaxCountOfRunningResolutions
			=> DefaultOfMaxCountOfRunningResolutions;

		public IEnumerable<IDependencyResolutionContext> RunningResolutions
			=> EnumerateDA(ReadDA(ref _runningResolutionsSequence));

		public object Owner
			=> ReadDA(ref _owner);

		public IServiceProvider ServiceProvider
			=> ReadDA(location: ref _serviceProviderApi);

		public virtual IDependencyResolutionModel<IDependencyScope> GetResolutionModel()
			=> ReadDA(ref _resolutionModelLazy).Value;

		// TODO: Put strings into the resources.
		//
		public virtual bool ResolveDependency<TDependency>(IDependencyResolutionContext ctx, out TDependency dependency)
			where TDependency : class {
			//
			ctx.EnsureNotNull(nameof(ctx));
			if (!ReferenceEquals(ctx.Scope, this))
				throw
					new ArgumentOutOfRangeException(
						paramName: nameof(ctx),
						message: $"Указанный контекст не принадлежит данной области функциональной зависимости.{Environment.NewLine}\tКонтекст:{ctx.FmtStr().GNLI2()}{Environment.NewLine}\tОбласть:{this.FmtStr().GNLI2()}");
			else if (!typeof(TDependency).IsAssignableFrom(ctx.Specs.DependencyType))
				throw
					new ArgumentOutOfRangeException(
						paramName: nameof(ctx),
						message: $"Тип функциональной зависимости '{ctx.Specs.DependencyType}', определенный параметрами указанного контекста '{ctx}', не совместим с типом функциональной зависимости '{typeof(TDependency)}', указанным для данного вызова разрешения функциональной зависимости.");
			//
			if (ctx.Specs.IsNewInstanceRequired && _prohibitNewInstanceRequest)
				throw new EonException(message: $"Для данной области функциональной зависимости запрещены запросы на разрешение зависимости в новый экземпляр.{Environment.NewLine}\tОбласть:{this.FmtStr().GNLI2()}");
#if DO_NOT_USE_OXY_LOGGING_API
			if (ctx.IsAdvancedLoggingEnabled)
				throw new NotSupportedException(message: $"В тек. версии расширенное логирование разрешения функциональной зависимости (см. {nameof(ctx)}.{nameof(ctx.IsAdvancedLoggingEnabled)}) не поддерживается.").SetErrorCode(code: GeneralErrorCodes.Operation.NotSupported);
#else
			var isContextAdvancedLoggingEnabled = ctx.IsAdvancedLoggingEnabled;
#endif
			//
			var ensureResolution = ctx.Specs.EnsureResolution;
			var isNewInstanceRequired = ctx.Specs.IsNewInstanceRequired;
			var selectCriterion = ctx.Specs.SelectCriterion;
			var errorMessagePrologue = $"Было затребовано разрешение функциональной зависимости.{Environment.NewLine}Функциональная зависимость:{Environment.NewLine}{typeof(TDependency).FmtStr().G().IndentLines()}{Environment.NewLine}Параметры разрешения:{Environment.NewLine}{ctx.Specs.ToString().IndentLines()}.";
			//
			List<IDependencyHandler2> currentScopeHandlersTraversal;
			IDependencyHandler2 currentHandler;
			object resultDependencyInstanceAsObject;
			IDisposable resultInstanceAsDisposable;
			var resultInstance = default(TDependency);
			var handlerResult = DependencyResult.None;
			//
			var newDependencyInstancesDisposeRegistry = ctx.Specs.DisposeRegistry ?? ReadDA(ref _newDependencyInstancesDisposeRegistry);
			//
			var runningResolutionsSpinLock = ReadDA(ref _runningResolutionsSpinLock);
			var runningResolutionsUniqueness = ReadDA(ref _runningResolutionsUniqueness);
			var runningResolutionsSequence = ReadDA(ref _runningResolutionsSequence);
			//
			var involvedScopes = new HashSet<IDependencyScope>(comparer: ReferenceEqualityComparer<IDependencyScope>.Instance);
			try {
				// Регистрация выполнения запроса ф. зависимости.
				//
				runningResolutionsSpinLock
					.Invoke(
						() => {
							EnsureNotDisposeState();
							//
							if (runningResolutionsSequence.Count >= MaxCountOfRunningResolutions)
								throw
									new EonException(
										message: $"{errorMessagePrologue}{Environment.NewLine}Для области функциональной зависимости достигнут предел '{MaxCountOfRunningResolutions}' максимального количества одновременно выполняющихся вызовов разрешения функциональной зависимости.{Environment.NewLine}\tОбласть функциональной зависимости:{this.FmtStr().GNLI2()}")
									.SetErrorCode(DependencyErrorCodes.Fault);
							else if (runningResolutionsUniqueness.Add(item: ctx))
								runningResolutionsSequence.Add(item: ctx);
							else
								throw
									new EonException(
										message: $"{errorMessagePrologue}{Environment.NewLine}Указанный контекст уже используется другим вызовом разрешения функциональной зависимости в данной области функциональной зависимости.{Environment.NewLine}\tКонтекст:{ctx.FmtStr().GNLI2()}{Environment.NewLine}\tОбласть функциональной зависимости:{this.FmtStr().GNLI2()}")
									.SetErrorCode(DependencyErrorCodes.Fault);
						});
				//
				var thisScope = (IDependencyScope)this;
				var previousScope = default(IDependencyScope);
				foreach (var currentScope in thisScope.TreeNode().SelfAndAncestors(parentSelector: locItem => locItem.GetOuterScope())) {
					if (!involvedScopes.Add(currentScope))
						throw
							new EonException(
								message: $"{errorMessagePrologue}{Environment.NewLine}Указанная область функциональной зависимости в составе окружающих (внешних) областей для данной области повторяется, что недопустимо.{Environment.NewLine}\tДанная область:{this.FmtStr().GNLI2()}{Environment.NewLine}\tУказанная область:{this.FmtStr().GNLI2()}{Environment.NewLine}\tОбласть, предшествующая указанной:{previousScope.FmtStr().GNLI2()}")
							.SetErrorCode(DependencyErrorCodes.Fault);
					// Составить упорядоченный список обхода обработчиков запроса ф. зависимости.
					//
					currentScopeHandlersTraversal = new List<IDependencyHandler2>(collection: currentScope.GetResolutionModel());
					if (involvedScopes.Count == 1) {
						// Сначала должны быть использованы обработчики запроса ф. зависимости первичной модели (если она задана), поэтому они добавляются в начало списка обхода.
						//
						var primaryModel = ctx.Specs.PrimaryResolutionModel.EmptyIfNull();
						if (currentScopeHandlersTraversal.Count == 0)
							currentScopeHandlersTraversal.AddRange(collection: primaryModel);
						else
							currentScopeHandlersTraversal.InsertRange(index: 0, collection: primaryModel);
						//
						previousScope = currentScope;
					}
					// Обход списка обработчиков запроса ф. зависимости.
					//
					var currentHandlerIndex = 0;
					var handlerRedirectCounter = 0;
					for (; resultInstance is null && currentHandlerIndex < currentScopeHandlersTraversal.Count;) {
						currentHandler = currentScopeHandlersTraversal[ currentHandlerIndex ];
						//
						if (currentHandler != null && ctx.TryInvolveHandler(handler: currentHandler)) {
							resultInstanceAsDisposable = (resultDependencyInstanceAsObject = (handlerResult = currentHandler.ExecuteResolution(resolutionCtx: ctx)).Instance) as IDisposable;
							if (resultDependencyInstanceAsObject is null) {
								if (!(handlerResult.RedirectHandler is null)) {
									if (handlerRedirectCounter++ < DefaultOfMaxCountOfResolutionRedirect) {
										var redirectHandlerIndex = currentHandlerIndex + 1;
										if (redirectHandlerIndex < currentScopeHandlersTraversal.Count)
											currentScopeHandlersTraversal.Insert(index: redirectHandlerIndex, item: handlerResult.RedirectHandler);
										else
											currentScopeHandlersTraversal.Add(item: handlerResult.RedirectHandler);
									}
									else
										throw
											new EonException(
												message: $"Dependency resolution was broken due to the count of dependency handlers redirects reached the limit — {DefaultOfMaxCountOfResolutionRedirect:d} (see {nameof(DependencyScope)}.{nameof(DefaultOfMaxCountOfResolutionRedirect)}).{Environment.NewLine}Redirect from handler:{currentHandler.FmtStr().GNLI2()}{Environment.NewLine}Redirect to handler:{handlerResult.RedirectHandler.FmtStr().GNLI2()}{Environment.NewLine}Scope:{currentScope.FmtStr().GNLI2()}")
											.SetErrorCode(code: DependencyErrorCodes.Fault);
								}
							}
							else if ((resultInstance = resultDependencyInstanceAsObject as TDependency) is null)
								throw
									new EonException(
										$"{errorMessagePrologue}{Environment.NewLine}Компонент, исполняющий разрешение функциональной зависимости, возвратил недопустимый результат. Тип '{resultDependencyInstanceAsObject.GetType()}' экземпляра, полученного от компонента, не совместим с требуемым типом '{typeof(TDependency)}'.{Environment.NewLine}\tКомпонент:{currentHandler.FmtStr().GNLI2()}")
									.SetErrorCode(DependencyErrorCodes.Fault);
							else if (ctx.Specs.IsNewInstanceRequired && !handlerResult.IsNewInstance)
								throw
									new EonException(
										$"{errorMessagePrologue}{Environment.NewLine}Компонент, исполняющий разрешение функциональной зависимости, возвратил недопустимый результат. От компонента было затребовано создание нового экземпляра, но компонент возвратил результат, указывающий, что экземпляр не является новым.{Environment.NewLine}\tКомпонент:{currentHandler.FmtStr().GNLI2()}")
									.SetErrorCode(DependencyErrorCodes.Fault);
							else if (!ctx.IsMatchSelectCriterion(resultInstance)) {
								if (handlerResult.IsNewInstance)
									resultInstanceAsDisposable?.Dispose();
								resultInstanceAsDisposable = null;
								resultDependencyInstanceAsObject = null;
								resultInstance = null;
							}
							else if (handlerResult.IsNewInstance && resultInstanceAsDisposable != null)
								newDependencyInstancesDisposeRegistry.Register(resultInstanceAsDisposable);
						}
						//
#if !DO_NOT_USE_OXY_LOGGING_API
						if (isContextAdvancedLoggingEnabled && !(resultDependencyInstance is null)) {
							string loggingInformationMessage;
							using (var acquiredBuffer = StringBuilderUtilities.AcquireBuffer()) {
								var sb = acquiredBuffer.StringBuilder;
								sb.AppendLine($"Функциональная зависимость разрешена.");
								sb.AppendLine($"\tПараметры разрешения:");
								sb.AppendLine(ctx.Specs.ToString().IndentLines(indentSize: 2));
								sb.AppendLine($"\tЭкземпляр зависимости:");
								sb.Append(resultDependencyInstance.ToString().IndentLines(indentSize: 2));
								loggingInformationMessage = sb.ToString();
							}
							context.IssueInformation($"Разрешение функциональной зависимости (ИД контекста '{contextSequentialId:d}').", loggingInformationMessage);
						}
#endif
						//
						currentHandlerIndex++;
					}
					//
					if (resultInstance != null)
						break;
				}
			}
			catch (Exception exception) {
				if (handlerResult.IsNewInstance)
					DisposableUtilities.DisposeMany(exception, handlerResult.Instance as IDisposable);
				throw;
			}
			finally {
				runningResolutionsSpinLock
					.Invoke(
						() => {
							if (!(Disposing || IsDisposed)) {
								runningResolutionsSequence.RemoveAll(match: locItem => ReferenceEquals(objA: locItem, objB: ctx));
								runningResolutionsUniqueness.Remove(item: ctx);
							}
						});
			}
			//
			if (resultInstance is null) {
				if (ensureResolution) {
					// Формирование текста сообщения ошибки.
					//
					// TODO: involvedExecutorsChainShowReportMaxCount сделать конфигурируемым.
					//
					var involvedExecutorsChainShowReportMaxCount = FormatStringUtilities.DefaultMaxCountOfCollectionOutputItems;
					string involvedExecutorsChainReport;
					IDependencyHandler2[ ] involvedHandlerChain;
					string involvedHandlerText(IDependencyHandler2 locHandler)
						=> $"Объект:{locHandler.FmtStr().GNLI()}";
					using (var buffer = StringBuilderUtilities.AcquireBuffer()) {
						var sb = buffer.StringBuilder;
						involvedHandlerChain = ctx.GetInvolvedHandlerChain();
						var involvedExecutorsChainLimit = Math.Min(involvedExecutorsChainShowReportMaxCount, involvedHandlerChain.Length);
						for (var k = 0; k < involvedExecutorsChainLimit; k++) {
							if (k > 0)
								sb.AppendLine();
							sb.Append($"{(k + 1):d}) {involvedHandlerText(involvedHandlerChain[ k ])}");
						}
						if (involvedHandlerChain.Length > involvedExecutorsChainShowReportMaxCount) {
							if (involvedHandlerChain.Length - involvedExecutorsChainShowReportMaxCount < 3)
								sb.Append($"{Environment.NewLine}{(involvedExecutorsChainShowReportMaxCount + 1):d}) {involvedHandlerText(involvedHandlerChain[ involvedExecutorsChainShowReportMaxCount ])}");
							else
								sb.Append($"{Environment.NewLine}…");
							//
							if (involvedHandlerChain.Length > (involvedExecutorsChainShowReportMaxCount + 1))
								sb.Append($"{Environment.NewLine}{(involvedHandlerChain.Length):d}) {involvedHandlerText(involvedHandlerChain[ involvedHandlerChain.Length - 1 ])}");
						}
						involvedExecutorsChainReport = sb.ToString();
					}
					throw
						new EonException(
							$"{errorMessagePrologue}{Environment.NewLine}Указанная зависимость не была разрешена.{Environment.NewLine}Детали:{Environment.NewLine}\tОбласть функциональной зависимости:{Environment.NewLine}{this.FmtStr().GI2()}{Environment.NewLine}\tПоследовательность задействованных обработчиков (длина {involvedHandlerChain.Length:d}):{Environment.NewLine}{involvedExecutorsChainReport.IndentLines2()}")
						.SetErrorCode(DependencyErrorCodes.Resolution_NotResolved);
				}
				else {
					dependency = null;
					return false;
				}
			}
			else {
				dependency = resultInstance;
				return true;
			}
		}

		public TDependency ResolveDependency<TDependency>(IDependencyResolutionContext context)
			where TDependency : class {
			//
			ResolveDependency(ctx: context, dependency: out TDependency result);
			return result;
		}

		protected virtual object GetService(Type serviceType) {
			serviceType.EnsureNotNull(nameof(serviceType)).EnsureClassOrInterface();
			//
			var invoker = default(P_ResolveDependencyInvokerBase);
			if (!__ResolveDependencyInvokeRepoSpinLock.Invoke(() => __ResolveDependencyInvokers.TryGetValue(key: serviceType, value: out invoker))) {
				invoker = ActivationUtilities.RequireConstructor<P_ResolveDependencyInvokerBase>(typeof(P_ResolveDependencyInvoker<>).MakeGenericType(typeArguments: serviceType))();
				__ResolveDependencyInvokeRepoSpinLock
					.Invoke(
						action:
							() => {
								try {
									__ResolveDependencyInvokers.Add(key: serviceType, value: invoker);
								}
								catch (ArgumentException) {
									invoker = __ResolveDependencyInvokers[ serviceType ];
								}
							});

			}
			object dependency;
			using (var ctx = new DependencyResolutionContext(scope: this, specs: new DependencyResolutionSpecs(dependencyType: serviceType, ensureResolution: false), ownsSpecs: true))
				invoker.Invoke(scope: this, ctx: ctx, dependency: out dependency);
			return dependency;
		}

		public IDependencyScope CreateScopeCopy(object copyOwner = null) {
			CreateScopeCopy(copyOwner ?? Owner, out var copy);
			return copy;
		}

		protected virtual void CreateScopeCopy(object copyOwner, out IDependencyScope copy)
			=> copy = new DependencyScope(outerScopeGetter: OuterScopeGetter, exporter: Exporter, ownsExporter: false, prohibitNewInstanceRequest: ProhibitNewInstanceRequest, owner: copyOwner);

		public IDependencyScope CreateChildScope(object childOwner = default) {
			CreateChildScope(childOwner ?? Owner, out var child);
			return child;
		}

		protected virtual void CreateChildScope(object childOwner, out IDependencyScope child)
			=> child = new DependencyScope(outerScope: this, exporter: NopDependencyExporter.Instance, ownsExporter: false, prohibitNewInstanceRequest: ProhibitNewInstanceRequest, owner: childOwner);

		public override string ToString()
			=> $"Owner:{_owner.FmtStr().GNLI()}";

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_newDependencyInstancesDisposeRegistry?.Dispose();
				//
				_runningResolutionsSpinLock?.EnterAndExitLock();
				_runningResolutionsUniqueness?.Clear();
				_runningResolutionsSequence?.Clear();
				//
				_resolutionModelLazy?.Dispose();
				//
				_exporter?.Dispose();
				//
				_outerScopeGetter?.Dispose();
			}
			_newDependencyInstancesDisposeRegistry = null;
			_runningResolutionsUniqueness = null;
			_runningResolutionsSequence = null;
			_runningResolutionsSpinLock = null;
			_resolutionModelLazy = null;
			_exporter = null;
			_outerScopeGetter = null;
			_serviceProviderApi = null;
			_owner = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}