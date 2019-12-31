using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Eon.Reflection;
using Eon.Runtime.Options;
using Eon.Threading;

using static Eon.Reflection.ActivationUtilities;
using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Defines utilities for dependency functionality.
	/// </summary>
	public static class DependencyUtilities {

		#region Nested types

		static class P_DH {

			#region Nested types

			public sealed class Delegate
				:DependencyHandlerBase, IDependencyHandler2 {

				Func<IDependencyResolutionContext, DependencyResult> _executeResolution;

				internal Delegate(Func<IDependencyResolutionContext, DependencyResult> executeResolution) {
					executeResolution.EnsureNotNull(nameof(executeResolution));
					//
					_executeResolution = executeResolution;
				}

				public bool CanShareDependency {
					get {
						EnsureNotDisposeState();
						return true;
					}
				}

				public bool CanRedirect {
					get {
						EnsureNotDisposeState();
						return true;
					}
				}

				protected override DependencyResult ResolveDependency(IDependencyResolutionContext resolutionContext)
					=> ReadDA(ref _executeResolution)(resolutionContext);

				protected override void Dispose(bool explicitDispose) {
					_executeResolution = null;
					base.Dispose(explicitDispose);
				}

			}

			// TODO_HIGH: Вынести в пространство имен как самостоятельный тип.
			//
			public sealed class Shared<TInstance>
				:DependencyHandlerBase, IDependencyHandler2
				where TInstance : class {

				#region Static members

				static readonly Type __InstanceType;

				static readonly Lazy<Func<TInstance>> __InstanceCtor;

				static Shared() {
					__InstanceType = typeof(TInstance);
					__InstanceCtor = new Lazy<Func<TInstance>>(valueFactory: () => RequireConstructor<TInstance>(), mode: LazyThreadSafetyMode.PublicationOnly);
				}

				#endregion

				Func<IVh<TInstance>> _factory;

				DisposableLazy<IVh<TInstance>> _instance;

				internal Shared() {
					_factory = () => __InstanceCtor.Value().ToValueHolder(ownsValue: true);
					_instance = new DisposableLazy<IVh<TInstance>>(factory: ReadDA(ref _factory), ownsValue: true);
				}

				internal Shared(Func<IVh<TInstance>> factory) {
					factory.EnsureNotNull(nameof(factory));
					//
					_factory = factory;
					_instance = new DisposableLazy<IVh<TInstance>>(factory: ReadDA(ref _factory), ownsValue: true);
				}

				public bool CanShareDependency {
					get {
						EnsureNotDisposeState();
						return true;
					}
				}

				public bool CanRedirect {
					get {
						EnsureNotDisposeState();
						return false;
					}
				}

				protected override DependencyResult ResolveDependency(IDependencyResolutionContext ctx)
					=>
					ctx.IsSatisfiesExisting(dependencyInstanceType: __InstanceType)
					? ReadDA(ref _instance).Value.Fluent().If(condition: locValue => locValue is null, trueBody: locValue => DependencyResult.None, falseBody: locValue => new DependencyResult(instance: locValue, isNewInstance: false))
					: DependencyResult.None;

				protected override void Dispose(bool explicitDispose) {
					if (explicitDispose)
						_instance?.Dispose();
					_instance = null;
					//
					base.Dispose(explicitDispose);
				}

			}

			public sealed class New<TDependencyInstance>
				:DependencyHandlerBase, IDependencyHandler2
				where TDependencyInstance : class {

				#region Static members

				static readonly Type __DependencyInstanceType = typeof(TDependencyInstance);

				#endregion

				Func<TDependencyInstance> _dependencyInstanceFactory;

				internal New(Func<TDependencyInstance> dependencyInstanceFactory) {
					dependencyInstanceFactory.EnsureNotNull(nameof(dependencyInstanceFactory));
					//
					_dependencyInstanceFactory = dependencyInstanceFactory;
				}

				public bool CanShareDependency {
					get {
						EnsureNotDisposeState();
						return false;
					}
				}

				public bool CanRedirect {
					get {
						EnsureNotDisposeState();
						return false;
					}
				}

				protected override DependencyResult ResolveDependency(IDependencyResolutionContext resolutionContext)
					=>
					resolutionContext
					.IsSatisfiesNew(__DependencyInstanceType)
					? ReadDA(ref _dependencyInstanceFactory)()
						.Fluent()
						.If(condition: locValue => locValue is null, trueBody: locValue => DependencyResult.None, falseBody: locValue => new DependencyResult(instance: locValue, isNewInstance: true))
					: DependencyResult.None;
				protected override void Dispose(bool explicitDispose) {
					_dependencyInstanceFactory = null;
					//
					base.Dispose(explicitDispose);
				}

			}

			#endregion

		}

		sealed class P_DependencySupport
			:IDependencySupport {

			readonly IDependencyScope _scope;

			internal P_DependencySupport(IDependencyScope scope) {
				scope.EnsureNotNull(nameof(scope));
				//
				_scope = scope;
			}

			public IDependencySupport OuterDependencies 
				=> null;

			public IEnumerable<IVh<IDependencyHandler2>> LocalDependencies()
				=> Enumerable.Empty<IVh<IDependencyHandler2>>();

			public IDependencyScope GetDependencyScope()
				=> _scope;

		}

		#endregion

		static long __ResolutionContextSequentialIdDecrementCounter;

		static DependencyUtilities() {
			__ResolutionContextSequentialIdDecrementCounter = long.MaxValue;
		}

		/// <summary>
		/// Gets the current dependency resolution settings set by <see cref="DependencyResolutionSettingsOption"/> (see <see cref="RuntimeOptions"/>).
		/// </summary>
		public static IDependencyResolutionSettings DefaultResolutionSettings
			=> DependencyResolutionSettingsOption.Require();

		/// <summary>
		/// Создает объект <see cref="IDependencySupport"/> для указанной области функциональной зависимости.
		/// <para>Метод возвращает <see langword="null"/>, если <paramref name="dependencyScope"/> равно <see langword="null"/>.</para>
		/// </summary>
		/// <param name="dependencyScope">Область функциональной зависимости.</param>
		/// <returns>Объект <see cref="IDependencySupport"/>.</returns>
		public static IDependencySupport ToDependencySupport(this IDependencyScope dependencyScope)
			=> dependencyScope is null ? null : new P_DependencySupport(scope: dependencyScope);

		#region Dependency resolution

		// TODO: Put exception messages into the resources.
		//
		public static IDependencyScope RequireDependencyScope(this IDependencySupport dependencies) {
			dependencies.EnsureNotNull(nameof(dependencies));
			//
			var result = dependencies.GetDependencyScope();
			if (result is null)
				throw new EonException(message: $"Component doesn't have a dependency scope.{Environment.NewLine}\tComponent:{dependencies.FmtStr().GNLI2()}");
			else
				return result;
		}

		public static TDependency ResolveDependency<TDependency>(
			this IDependencyScope dependencyScope,
			Type dependencyTypeConstraint = default,
			bool ensureResolution = default,
			bool isNewInstanceRequired = default,
			bool preventNewInstanceInitialization = default,
			IDependencyResolutionModel primaryResolutionModel = default,
			Func<TDependency, bool> selectCriterionFunc = default,
			IArgsTuple newInstanceFactoryArgs = default,
			IDisposeRegistry disposeRegistry = default)
			where TDependency : class {
			//
			dependencyScope.EnsureNotNull(nameof(dependencyScope));
			//
			using (
				var context =
					new DependencyResolutionContext(
						scope: dependencyScope,
						specs:
							new DependencyResolutionSpecs<TDependency>(
								dependencyTypeConstraint: dependencyTypeConstraint,
								ensureResolution: ensureResolution,
								isNewInstanceRequired: isNewInstanceRequired,
								preventNewInstanceInitialization: preventNewInstanceInitialization,
								primaryResolutionModel: primaryResolutionModel,
								selectCriterion: selectCriterionFunc == null ? null : new DependencyResolutionSelectCriterionFunc<TDependency>(selectCriterionFunc),
								newInstanceFactoryArgs: newInstanceFactoryArgs,
								disposeRegistry: disposeRegistry),
						ownsSpecs: true))
				return dependencyScope.ResolveDependency<TDependency>(context: context);
		}

		public static TDependency ResolveDependency<TDependency>(
			this IDependencySupport dependencies,
			Type dependencyTypeConstraint = default,
			bool ensureResolution = default,
			bool isNewInstanceRequired = default,
			bool preventNewInstanceInitialization = default,
			IDependencyResolutionModel primaryResolutionModel = default,
			Func<TDependency, bool> selectCriterionFunc = default,
			IArgsTuple newInstanceFactoryArgs = default,
			IDisposeRegistry disposeRegistry = default)
			where TDependency : class
			=>
			dependencies
			.RequireDependencyScope()
			.ResolveDependency(
				dependencyTypeConstraint: dependencyTypeConstraint,
				ensureResolution: ensureResolution,
				isNewInstanceRequired: isNewInstanceRequired,
				preventNewInstanceInitialization: preventNewInstanceInitialization,
				primaryResolutionModel: primaryResolutionModel,
				selectCriterionFunc: selectCriterionFunc,
				newInstanceFactoryArgs: newInstanceFactoryArgs,
				disposeRegistry: disposeRegistry);

		public static TService GetService<TService>(this IDependencySupport dependencies)
			where TService : class
			=> dependencies
			.RequireDependencyScope()
			.ResolveDependency<TService>(ensureResolution: false);

		public static TService GetService<TService>(this IDependencySupport dependencies, Func<TService, bool> selectCriterionFunc)
			where TService : class
			=> dependencies
			.RequireDependencyScope()
			.ResolveDependency<TService>(ensureResolution: false, selectCriterionFunc: selectCriterionFunc);

		public static TService GetService<TService>(this IDependencySupport dependencies, bool isNewInstanceRequired)
			where TService : class
			=> dependencies
			.RequireDependencyScope()
			.ResolveDependency<TService>(ensureResolution: false, isNewInstanceRequired: isNewInstanceRequired);

		public static object GetService(this IDependencySupport dependencies, Type serviceType)
			=> dependencies.RequireDependencyScope().ServiceProvider.GetService(serviceType);

		public static TService RequireService<TService>(this IDependencySupport dependencies)
			where TService : class
			=> dependencies.RequireDependencyScope().ResolveDependency<TService>(ensureResolution: true);

		public static TService RequireService<TService>(this IDependencySupport dependencies, bool isNewInstanceRequired)
			where TService : class {
			return
				dependencies
				.RequireDependencyScope()
				.ResolveDependency<TService>(ensureResolution: true, isNewInstanceRequired: isNewInstanceRequired);
		}

		public static TService RequireServiceNew<TService>(this IDependencySupport dependencies)
			where TService : class
			=> dependencies.RequireDependencyScope().ResolveDependency<TService>(ensureResolution: true, isNewInstanceRequired: true);

		public static TService RequireServiceNew<TService, TArg>(this IDependencySupport dependencies, TArg arg)
			where TService : class
			=> dependencies.RequireDependencyScope().ResolveDependency<TService>(ensureResolution: true, isNewInstanceRequired: true, newInstanceFactoryArgs: ArgsTuple.Create(arg1: arg));

		public static TService RequireService<TService>(this IDependencySupport dependencies, Func<TService, bool> selectCriterionFunc)
			where TService : class {
			return
				dependencies
				.RequireDependencyScope()
				.ResolveDependency(ensureResolution: true, selectCriterionFunc: selectCriterionFunc);
		}

		public static TServiceBase RequireService<TServiceBase>(this IDependencySupport dependencies, Type serviceType, bool isNewInstanceRequired)
			where TServiceBase : class {
			serviceType.EnsureNotNull(nameof(serviceType));
			if (!(serviceType.IsClass() || serviceType.IsInterface()))
				throw new ArgumentOutOfRangeException(nameof(serviceType), FormatXResource(typeof(ArgumentException), "ValueIsInvalid/Type/NotClass", serviceType));
			else if (!typeof(TServiceBase).IsAssignableFrom(serviceType))
				throw new ArgumentOutOfRangeException(nameof(serviceType), FormatXResource(typeof(ArgumentException), "ValueIsInvalid/Type/InvalidDerivation", serviceType, typeof(TServiceBase)));
			//
			IDependencyScope dependencyScope;
			using (
				var context =
					new DependencyResolutionContext(
						scope: dependencyScope = dependencies.RequireDependencyScope(),
						specs:
							new DependencyResolutionSpecs(
								dependencyType: serviceType,
								ensureResolution: true,
								isNewInstanceRequired: isNewInstanceRequired),
						ownsSpecs: true))
				return dependencyScope.ResolveDependency<TServiceBase>(context);
		}

		#endregion

		// TODO: Put strings into the resources.
		//
		public static long NextResolutionContextSequentialId() {
			if (InterlockedUtilities.Decrement(location: ref __ResolutionContextSequentialIdDecrementCounter, minExclusive: -1, result: out var counterDecrement))
				return long.MaxValue - counterDecrement;
			else
				throw new EonException($"Невозможно получить следующий последовательный идентификатор контекста разрешения функциональной зависимости. Значение счетчика достигло предельного значения '{(long.MaxValue - counterDecrement):d}'.");
		}

		/// <summary>
		/// Проверяет, что требуемый тип функциональной зависимости, указанный контекстом разрешения <paramref name="ctx"/>, совместим с указанным типом <paramref name="dependencyType"/>.
		/// </summary>
		/// <param name="ctx">Контекст разрешения функциональной зависимости.</param>
		/// <param name="dependencyType">Тип.</param>
		/// <returns>True — требуемый тип функциональной зависимости совместим с типом <paramref name="dependencyType"/>;<para>False — иначе.</para></returns>
		public static bool IsRequiredOf(this IDependencyResolutionContext ctx, Type dependencyType) {
			ctx.EnsureNotNull(nameof(ctx));
			dependencyType.EnsureNotNull(nameof(dependencyType));
			//
			return dependencyType.IsAssignableFrom(ctx.Specs.DependencyType);
		}

		public static bool IsRequiredExistingOf(this IDependencyResolutionContext ctx, Type dependencyType) {
			ctx.EnsureNotNull(nameof(ctx));
			dependencyType.EnsureNotNull(nameof(dependencyType));
			//
			return !ctx.Specs.IsNewInstanceRequired && dependencyType.IsAssignableFrom(ctx.Specs.DependencyType);
		}

		public static bool IsRequiredNewOf(this IDependencyResolutionContext ctx, Type dependencyType) {
			var specs = ctx.EnsureNotNull(nameof(ctx)).Value.Specs;
			dependencyType.EnsureNotNull(nameof(dependencyType));
			//
			return specs.IsNewInstanceRequired && dependencyType.IsAssignableFrom(specs.DependencyType);
		}

		/// <summary>
		/// Выполняет проверку соответствия экземпляра функциональности <paramref name="dependencyInstance"/> критериям разрешения (поиска) функциональной зависимости.
		/// <para>При этом одним из критериев является то, что НЕ требуется разрешения функциональной зависимости в новый экземпляр функциональности (<seealso cref="IDependencyResolutionSpecs.IsNewInstanceRequired"/>).</para>
		/// </summary>
		/// <param name="ctx">Контекст разрешения функциональной зависимости.</param>
		/// <param name="dependencyInstance">Существующий экземпляр функциональности. Может быть null. В случае, когда параметр равен null, метод возвращает false.</param>
		/// <returns>Значение <see cref="bool"/>.</returns>
		public static bool IsSatisfiesExisting(this IDependencyResolutionContext ctx, object dependencyInstance) {
			var resolutionParams = ctx.EnsureNotNull(nameof(ctx)).Value.Specs;
			//
			if (dependencyInstance == null)
				return false;
			else
				return !resolutionParams.IsNewInstanceRequired && resolutionParams.DependencyType.IsAssignableFrom(dependencyInstance.GetType());
		}

		/// <summary>
		/// Выполняет проверку соответствия типа экземпляра функциональности <paramref name="dependencyInstanceType"/> критериям разрешения (поиска) функциональной зависимости.
		/// <para>При этом одним из критериев является то, что НЕ требуется разрешения функциональной зависимости в новый экземпляр функциональности (<seealso cref="IDependencyResolutionSpecs.IsNewInstanceRequired"/>).</para>
		/// </summary>
		/// <param name="ctx">Контекст разрешения функциональной зависимости.</param>
		/// <param name="dependencyInstanceType">Тип экземпляра функциональности.</param>
		/// <returns>Значение <see cref="bool"/>.</returns>
		public static bool IsSatisfiesExisting(this IDependencyResolutionContext ctx, Type dependencyInstanceType) {
			var resolutionParams = ctx.EnsureNotNull(nameof(ctx)).Value.Specs;
			dependencyInstanceType.EnsureNotNull(nameof(dependencyInstanceType));
			//
			return !resolutionParams.IsNewInstanceRequired && resolutionParams.DependencyType.IsAssignableFrom(dependencyInstanceType);
		}

		public static bool IsSatisfiesNew(this IDependencyResolutionContext ctx, object dependencyInstance) {
			ctx.EnsureNotNull(nameof(ctx));
			if (dependencyInstance is null)
				return false;
			else
				return ctx.Specs.IsNewInstanceRequired && ctx.Specs.DependencyType.IsAssignableFrom(dependencyInstance.GetType());
		}

		public static bool IsSatisfiesNew(this IDependencyResolutionContext ctx, Type dependencyInstanceType) {
			var resolutionParams = ctx.EnsureNotNull(nameof(ctx)).Value.Specs;
			dependencyInstanceType.EnsureNotNull(nameof(dependencyInstanceType));
			//
			return resolutionParams.IsNewInstanceRequired && resolutionParams.DependencyType.IsAssignableFrom(dependencyInstanceType);
		}

		/// <summary>
		/// Выполняет проверку соответствия экземпляра функциональности <paramref name="dependencyInstance"/> критериям разрешения (поиска) функциональной зависимости.
		/// </summary>
		/// <param name="ctx">Контекст разрешения функциональной зависимости.</param>
		/// <param name="dependencyInstance">Экземпляр функциональности. Может быть <see langword="null"/>. В случае, когда параметр равен <see langword="null"/>, метод возвращает <see langword="false"/>.</param>
		/// <returns>Значение <seealso cref="bool"/>.</returns>
		public static bool IsSatisfies(this IDependencyResolutionContext ctx, object dependencyInstance) {
			ctx.EnsureNotNull(nameof(ctx));
			if (dependencyInstance == null)
				return false;
			return ctx.Specs.DependencyType.IsAssignableFrom(dependencyInstance.GetType());
		}

		public static bool IsSatisfies(this IDependencyResolutionContext ctx, Type dependencyInstanceType) {
			ctx.EnsureNotNull(nameof(ctx));
			dependencyInstanceType.EnsureNotNull(nameof(dependencyInstanceType));
			//
			return ctx.Specs.DependencyType.IsAssignableFrom(dependencyInstanceType);
		}

		#region Dependency resolution executor

		/// <summary>
		/// Создает исполнителя разрешения функциональной зависимости на основе делегата, реализующего логику выполнения разрешения зависимости.
		/// </summary>
		/// <param name="executeResolution">Делегат, реализующий логику выполнения разршенеия зависимости.</param>
		/// <returns>Объект <see cref="IVh{IImmutableValueStore}"/>.</returns>
		public static IVh<IDependencyHandler2> CreateHandler(Func<IDependencyResolutionContext, DependencyResult> executeResolution)
			=> new P_DH.Delegate(executeResolution: executeResolution).ToValueHolder(ownsValue: true);

		/// <summary>
		/// Создает исполнителя разрешения функциональной зависимости для общего экземпляра зависимости.
		/// <para>Экземпляр зависимости будет создан исполнителем, используя непараметризованный конструктор типа <typeparamref name="TInstance"/>.</para>
		/// </summary>
		/// <typeparam name="TInstance">
		/// Тип экземпляра зависимости.
		/// <para>Важно. Данный параметр должен представлять конкретный тип экземпляра зависимости, тип в свою очередь должен иметь конструктор без параметров.</para>
		/// </typeparam>
		/// <returns>Объект <see cref="IVh{IImmutableValueStore}"/>.</returns>
		public static IVh<IDependencyHandler2> CreateHandlerForShared<TInstance>()
			where TInstance : class
			=>
			(new P_DH.Shared<TInstance>())
			.ToValueHolder(ownsValue: true);

		/// <summary>
		/// Создает исполнителя разрешения функциональной зависимости для общего экземпляра зависимости.
		/// <para>Экземпляр зависимости будет создан исполнителем, используя метод-фабрику, указанный в <paramref name="factory"/>.</para>
		/// </summary>
		/// <typeparam name="TInstance">
		/// Тип экземпляра зависимости.
		/// <para>Данный параметр должен представлять конкретный тип экземпляра зависимости.</para>
		/// </typeparam>
		/// <param name="factory">Метод-фабрика экземпляра зависимости.</param>
		/// <returns>Объект <see cref="IVh{IImmutableValueStore}"/>.</returns>
		public static IVh<IDependencyHandler2> CreateHandlerForShared<TInstance>(Func<IVh<TInstance>> factory)
			where TInstance : class
			=> new P_DH.Shared<TInstance>(factory: factory).ToValueHolder(ownsValue: true);

		/// <summary>
		/// Создает исполнителя разрешения функциональной зависимости для нового экземпляра зависимости.
		/// <para>Каждый раз при выполнении разрешения зависимости исполнителем будет создаваться новый экземпляр зависимости, используя непараметризованный конструктор типа <typeparamref name="TInstance"/>.</para>
		/// </summary>
		/// <typeparam name="TInstance">
		/// Тип экземпляра зависимости.
		/// <para>Важно. Данный параметр должен представлять конкретный тип экземпляра зависимости, тип в свою очередь должен иметь конструктор без параметров.</para>
		/// </typeparam>
		/// <returns>Объект <see cref="IVh{IImmutableValueStore}"/>.</returns>
		public static IVh<IDependencyHandler2> CreateHandlerForNew<TInstance>()
			where TInstance : class
			=> new CtorFactoryDependencyHandler<TInstance>().ToValueHolder(ownsValue: true);

		/// <summary>
		/// Создает исполнителя разрешения функциональной зависимости для нового экземпляра зависимости.
		/// <para>Каждый раз при выполнении разрешения зависимости исполнителем будет создаваться новый экземпляр зависимости посредством метода-фабрики, указанноого в <paramref name="factory"/>.</para>
		/// </summary>
		/// <typeparam name="TInstance">
		/// Тип экземпляра зависимости.
		/// <para>Важно. Данный параметр должен представлять конкретный тип экземпляра зависимости.</para>
		/// </typeparam>
		/// <param name="factory">Метод-фабрика экземпляра зависимости.</param>
		/// <returns>Объект <see cref="IVh{IImmutableValueStore}"/>.</returns>
		public static IVh<IDependencyHandler2> CreateHandlerForNew<TInstance>(Func<TInstance> factory)
			where TInstance : class
			=> new P_DH.New<TInstance>(dependencyInstanceFactory: factory).ToValueHolder(ownsValue: true);

		#endregion

	}

}