using System;
using System.Threading.Tasks;
using Eon.Reflection;
using Eon.Threading.Tasks;

namespace Eon.ComponentModel.Dependencies {

	public class CtorFactoryDependencyHandler<TDependencyInstance>
		:FactoryDependencyHandlerBase<TDependencyInstance>
		where TDependencyInstance : class {

		const int __ArgsCount = 0;

		#region Static members

		static readonly DisposableLazy<Func<TDependencyInstance>> __DependencyInstanceCtor;

		static CtorFactoryDependencyHandler() {
			__DependencyInstanceCtor =
				new DisposableLazy<Func<TDependencyInstance>>(() => ActivationUtilities.RequireConstructor<TDependencyInstance>());
		}

		#endregion

		Func<Func<TDependencyInstance>> _getDependencyCtor;

		Func<TDependencyInstance> _customCtor;

		readonly bool _canShareDependency;

		IArgsTuple _factoryArgs;

		#region Ctor(s)

		public CtorFactoryDependencyHandler()
			: this(canShareDependency: false, factoryArgs: null) { }

		public CtorFactoryDependencyHandler(bool canShareDependency, IArgsTuple factoryArgs = null)
			: this(customCtor: null, canShareDependency: canShareDependency, factoryArgs: factoryArgs) { }

		public CtorFactoryDependencyHandler(Func<TDependencyInstance> customCtor = null, bool canShareDependency = false, IArgsTuple factoryArgs = null)
			: base(requireClassTDependencyInstance: customCtor == null) {
			//
			factoryArgs
				?.ArgsCount
				.Arg($"{nameof(factoryArgs)}.{nameof(factoryArgs.ArgsCount)}")
				.EnsureEq(operand: __ArgsCount);
			//
			_customCtor = customCtor;
			_canShareDependency = canShareDependency;
			_factoryArgs = factoryArgs;
			//
			if (customCtor is null)
				_getDependencyCtor = () => __DependencyInstanceCtor.Value;
			else
				_getDependencyCtor = () => ReadDA(ref _customCtor);
		}

		#endregion

		#region Properties

		public override bool HasFactoryArgs
			=> ReadDA(ref _factoryArgs) != null;

		protected sealed override void GetFactoryArgs(out IArgsTuple factoryArgs)
			=> factoryArgs = ReadDA(ref _factoryArgs);

		public override bool CanShareDependency {
			get {
				EnsureNotDisposeState();
				return _canShareDependency;
			}
		}

		#endregion

		protected override Task<DependencyResult<TDependencyInstance>> ResolveNewDependencyInstanceAsync(IDependencyResolutionContext context) {
			try {
				var specs = context.EnsureNotNull(nameof(context)).Value.Specs;
				//
				if ((specs.NewInstanceFactoryArgs?.ArgsCount ?? 0) != __ArgsCount)
					return TaskUtilities.FromResult(DependencyResult<TDependencyInstance>.None);
				else
					return
						TaskUtilities
						.FromResult(
							result:
								new DependencyResult<TDependencyInstance>(
									instance: ReadDA(ref _getDependencyCtor, considerDisposeRequest: true)()(),
									isNewInstance: true));
			}
			catch (Exception exception) {
				return Task.FromException<DependencyResult<TDependencyInstance>>(exception);
			}
		}

		protected override void Dispose(bool explicitDispose) {
			_getDependencyCtor = null;
			_customCtor = null;
			_factoryArgs = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}