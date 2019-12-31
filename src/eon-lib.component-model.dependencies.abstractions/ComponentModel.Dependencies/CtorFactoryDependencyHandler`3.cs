using System;
using System.Threading.Tasks;

using Eon.Reflection;
using Eon.Threading.Tasks;

namespace Eon.ComponentModel.Dependencies {

	public class CtorFactoryDependencyHandler<TArg1, TArg2, TDependencyInstance>
		:FactoryDependencyHandlerBase<TDependencyInstance>
		where TDependencyInstance : class {

		const int __ArgsCount = 2;

		#region Static members

		static readonly DisposableLazy<Func<TArg1, TArg2, TDependencyInstance>> __DependencyInstanceCtor;

		static CtorFactoryDependencyHandler() {
			__DependencyInstanceCtor =
				new DisposableLazy<Func<TArg1, TArg2, TDependencyInstance>>(() => ActivationUtilities.RequireConstructor<TArg1, TArg2, TDependencyInstance>());
		}

		#endregion

		Func<Func<TArg1, TArg2, TDependencyInstance>> _getDependencyCtor;

		Func<TArg1, TArg2, TDependencyInstance> _customCtor;

		readonly bool _canShareDependency;

		IArgsTuple<TArg1, TArg2> _factoryArgs;

		#region Ctor(s)

		public CtorFactoryDependencyHandler()
			: this(
				 canShareDependency: false,
				 factoryArgs: null) { }

		public CtorFactoryDependencyHandler(
			bool canShareDependency,
			IArgsTuple<TArg1, TArg2> factoryArgs = null)
			: this(
				 customCtor: null,
				 canShareDependency: canShareDependency,
				 factoryArgs: factoryArgs) { }

		public CtorFactoryDependencyHandler(
			Func<TArg1, TArg2, TDependencyInstance> customCtor = null,
			bool canShareDependency = false,
			IArgsTuple<TArg1, TArg2> factoryArgs = null)
			: base(requireClassTDependencyInstance: customCtor == null) {
			//
			factoryArgs
				?.ArgsCount
				.Arg($"{nameof(factoryArgs)}.{nameof(factoryArgs.ArgsCount)}")
				.EnsureEq(__ArgsCount);
			//
			_customCtor = customCtor;
			_canShareDependency = canShareDependency;
			_factoryArgs = factoryArgs;
			//
			if (customCtor == null)
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
				var specs =
					context
					.EnsureNotNull(nameof(context))
					.Value
					.Specs;
				//
				var factoryArgs = ReadDA(ref _factoryArgs);
				if (factoryArgs == null) {
					if ((factoryArgs = specs.NewInstanceFactoryArgs as IArgsTuple<TArg1, TArg2>) == null || factoryArgs.ArgsCount != __ArgsCount)
						return TaskUtilities.FromResult(DependencyResult<TDependencyInstance>.None);
				}
				else if (specs.NewInstanceFactoryArgs != null)
					return TaskUtilities.FromResult(DependencyResult<TDependencyInstance>.None);
				//
				return
					TaskUtilities
					.FromResult(
						result:
							new DependencyResult<TDependencyInstance>(
								instance:
									ReadDA(ref _getDependencyCtor)
									()
									(factoryArgs.Arg1, factoryArgs.Arg2),
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