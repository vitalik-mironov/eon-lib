using System;
using System.Threading.Tasks;

using Eon.Reflection;
using Eon.Threading.Tasks;

namespace Eon.ComponentModel.Dependencies {

	public class CtorFactoryDependencyHandler<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TDependencyInstance>
		:FactoryDependencyHandlerBase<TDependencyInstance>
		where TDependencyInstance : class {

		const int __ArgsCount = 6;

		#region Static members

		static readonly DisposableLazy<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TDependencyInstance>> __DependencyInstanceCtor;

		static CtorFactoryDependencyHandler() {
			__DependencyInstanceCtor =
				new DisposableLazy<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TDependencyInstance>>(() => ActivationUtilities.RequireConstructor<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TDependencyInstance>());
		}

		#endregion

		Func<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TDependencyInstance>> _getDependencyCtor;

		Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TDependencyInstance> _customCtor;

		readonly bool _canShareDependency;

		IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> _factoryArgs;

		public CtorFactoryDependencyHandler()
			: this(canShareDependency: false, factoryArgs: null) { }

		public CtorFactoryDependencyHandler(bool canShareDependency, IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> factoryArgs = default)
			: this(customCtor: null, canShareDependency: canShareDependency, factoryArgs: factoryArgs) { }

		public CtorFactoryDependencyHandler(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TDependencyInstance> customCtor = default, bool canShareDependency = false, IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> factoryArgs = default)
			: base(requireClassTDependencyInstance: customCtor is null) {
			factoryArgs?.ArgsCount.Arg($"{nameof(factoryArgs)}.{nameof(factoryArgs.ArgsCount)}").EnsureEq(operand: __ArgsCount);
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

		public override bool HasFactoryArgs
			=> !(ReadDA(ref _factoryArgs) is null);

		protected sealed override void GetFactoryArgs(out IArgsTuple factoryArgs)
			=> factoryArgs = ReadDA(ref _factoryArgs);

		public override bool CanShareDependency {
			get {
				EnsureNotDisposeState();
				return _canShareDependency;
			}
		}

		protected override Task<DependencyResult<TDependencyInstance>> ResolveNewDependencyInstanceAsync(IDependencyResolutionContext ctx) {
			try {
				var specs = ctx.EnsureNotNull(nameof(ctx)).Value.Specs;
				//
				var factoryArgs = ReadDA(ref _factoryArgs);
				if (factoryArgs is null) {
					if ((factoryArgs = specs.NewInstanceFactoryArgs as IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>) is null || factoryArgs.ArgsCount != __ArgsCount)
						return TaskUtilities.FromResult(DependencyResult<TDependencyInstance>.None);
				}
				else if (specs.NewInstanceFactoryArgs != null)
					return Task.FromResult(result: DependencyResult<TDependencyInstance>.None);
				//
				return
					Task
					.FromResult(
						result:
							new DependencyResult<TDependencyInstance>(
								instance:
									ReadDA(ref _getDependencyCtor)
									()
									(factoryArgs.Arg1, factoryArgs.Arg2, factoryArgs.Arg3, factoryArgs.Arg4, factoryArgs.Arg5, factoryArgs.Arg6),
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