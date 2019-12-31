using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Eon.Threading;
using Eon.Threading.Tasks;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Представляет обработчик функциональной зависимости, получающий новый экземпляр зависимости посредством какого-либо фабричного компонента (метода, конструктора, класса и т.д.).
	/// </summary>
	[DataContract]
	public abstract class FactoryDependencyHandlerBase<TDependencyInstance>
		:DependencyHandlerBase2<TDependencyInstance>, IFactoryDependencyHandler
		where TDependencyInstance : class {

		#region Static & constant members

		protected static readonly Type DependencyInstanceType;

		static FactoryDependencyHandlerBase() {
			DependencyInstanceType = typeof(TDependencyInstance);
		}

		static async Task P_InitializeNewDependencyInstanceAsync(TDependencyInstance instance) {
			instance.EnsureNotNull(nameof(instance));
			//
			if (instance is IInitializable initializableInstance)
				await initializableInstance.InitializeAsync(ct: CancellationToken.None).ConfigureAwait(false);
		}

		#endregion

		IVh<TDependencyInstance> _sharedInstanceStore;

		SemaphoreSlim _sharedInstanceInitializationLock;

		protected FactoryDependencyHandlerBase(bool requireClassTDependencyInstance) {
			if (requireClassTDependencyInstance)
				DependencyInstanceType.Arg(nameof(TDependencyInstance)).EnsureClass();
			//
			_sharedInstanceInitializationLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
		}

		/// <summary>
		/// Возвращает признак, указывающий, что данный обработчик имеет аргументы (параметры, см. <seealso cref="FactoryArgs"/>), с которыми должен быть вызван фабричный компонент, создающий новый экземпляр зависимости.
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		public abstract bool HasFactoryArgs { get; }

		// TODO: Put strings into the resources.
		//
		/// <summary>
		/// Возвращает аргументы (параметры, см. <seealso cref="FactoryArgs"/>), с которыми должен быть вызван фабричный компонент, создающий новый экземпляр зависимости.
		/// <para>При обращении к данному свойству будет вызвано исключение <seealso cref="EonException"/>, если аргументы отсутствуют (см. <seealso cref="HasFactoryArgs"/>).</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		public IArgsTuple FactoryArgs {
			get {
				IArgsTuple factoryArgs;
				GetFactoryArgs(out factoryArgs);
				if (factoryArgs == null || !HasFactoryArgs)
					throw
						new EonException(message: $"Данный обработчик функциональной зависимости не имеет аргументов создания экземпляра зависимости.{Environment.NewLine}\tОбработчик:{this.FmtStr().GNLI2()}");
				else
					return factoryArgs;
			}
		}

		protected abstract void GetFactoryArgs(out IArgsTuple factoryArgs);

		protected override async Task<DependencyResult<TDependencyInstance>> ResolveDependencyAsync(IDependencyResolutionContext ctx) {
			var specs = ctx.EnsureNotNull(nameof(ctx)).Value.Specs;
			//
			if (specs.DependencyType.IsAssignableFrom(DependencyInstanceType)) {
				var newInstanceResult = default(DependencyResult<TDependencyInstance>);
				if (specs.IsNewInstanceRequired || !CanShareDependency) {
					try {
						newInstanceResult = await ResolveNewDependencyInstanceAsync(ctx: ctx).ConfigureAwait(false);
						if (newInstanceResult.Instance is null)
							return newInstanceResult;
						else if (!newInstanceResult.IsNewInstance)
							throw
								new EonException(
									message: $"Результат вызова метода '{nameof(ResolveNewDependencyInstanceAsync)}' не соответствует ожидаемому. В частности, свойство результата '{nameof(newInstanceResult)}.{nameof(newInstanceResult.IsNewInstance)}' не соответствует ожидаемому значение '{bool.TrueString}'.");
						else if (specs.PreventNewInstanceInitialization)
							return newInstanceResult;
						else {
							await P_InitializeNewDependencyInstanceAsync(instance: newInstanceResult.Instance).ConfigureAwait(false);
							return newInstanceResult;
						}
					}
					catch (Exception exception) {
						if (newInstanceResult.IsNewInstance)
							(newInstanceResult.Instance as IDisposable)?.Dispose(exception);
						throw;
					}
				}
				else {
					// В спецификации запроса отсутствует требование разрешения зависимости именно в новый экземпляр (см. specs.IsNewInstanceRequired) + обработчик может "шарить" экземпляр (см. CanShareDependency).
					//
					var sharedInstanceStore = ReadDA(ref _sharedInstanceStore);
					if (sharedInstanceStore is null) {
						var lck = ReadDA(ref _sharedInstanceInitializationLock);
						var lckAcquired = false;
						try {
							lckAcquired = await lck.WaitAsync(millisecondsTimeout: TaskUtilities.DefaultAsyncTimeoutMilliseconds).ConfigureAwait(false);
							if (!lckAcquired)
								throw new LockAcquisitionFailException(reason: LockAcquisitionFailReason.TimeoutElapsed);
							//
							sharedInstanceStore = ReadDA(ref _sharedInstanceStore);
							if (sharedInstanceStore is null) {
								try {
									newInstanceResult = await ResolveNewDependencyInstanceAsync(ctx: ctx).ConfigureAwait(false);
									if (newInstanceResult.Instance is null)
										WriteDA(location: ref _sharedInstanceStore, value: sharedInstanceStore = default(TDependencyInstance).ToValueHolder(ownsValue: false));
									else if (!newInstanceResult.IsNewInstance)
										throw
											new EonException(
												message: $"Результат вызова метода '{nameof(ResolveNewDependencyInstanceAsync)}' не соответствует ожидаемому. В частности, свойство результата '{nameof(newInstanceResult)}.{nameof(newInstanceResult.IsNewInstance)}' не соответствует ожидаемому значение '{bool.TrueString}'.");
									else if (specs.PreventNewInstanceInitialization)
										WriteDA(
											location: ref _sharedInstanceStore,
											value: sharedInstanceStore = newInstanceResult.Instance.ToValueHolder(ownsValue: true));
									else {
										try {
											await P_InitializeNewDependencyInstanceAsync(instance: newInstanceResult.Instance).ConfigureAwait(false);
											sharedInstanceStore = newInstanceResult.Instance.ToValueHolder(ownsValue: true);
										}
										catch (Exception exception) {
											sharedInstanceStore = new Vh<TDependencyInstance>(exception: exception);
										}
										WriteDA(location: ref _sharedInstanceStore, value: sharedInstanceStore);
									}
								}
								catch (Exception exception) {
									if (newInstanceResult.IsNewInstance && !ReferenceEquals(itrlck.Get(ref _sharedInstanceStore)?.ValueDisposeTolerant, newInstanceResult.Instance))
										(newInstanceResult.Instance as IDisposable)?.Dispose(exception);
									throw;
								}
							}
						}
						finally {
							if (lckAcquired)
								try { lck.Release(); }
								catch (ObjectDisposedException) { }
						}
					}
					if (sharedInstanceStore.Value is null)
						return DependencyResult<TDependencyInstance>.None;
					else
						return new DependencyResult<TDependencyInstance>(instance: sharedInstanceStore.Value, isNewInstance: false);
				}
			}
			else
				return DependencyResult<TDependencyInstance>.None;
		}

		protected abstract Task<DependencyResult<TDependencyInstance>> ResolveNewDependencyInstanceAsync(IDependencyResolutionContext ctx);

		protected override void Dispose(bool explicitDispose) {
			if (explicitDispose) {
				_sharedInstanceInitializationLock?.Dispose();
				_sharedInstanceStore?.Dispose();
			}
			_sharedInstanceInitializationLock = null;
			_sharedInstanceStore = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}