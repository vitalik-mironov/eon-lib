using System;

namespace Eon.ComponentModel.Dependencies {

	public class DependencyResolutionSpecs<TDependency>
		:DependencyResolutionSpecs, IDependencyResolutionSpecs<TDependency>
		where TDependency : class {

		static readonly Type __DependencyType = typeof(TDependency);

		IDependencyResolutionSelectCriterion<TDependency> _selectCriterion;

		public DependencyResolutionSpecs(
			Type dependencyTypeConstraint = null,
			bool ensureResolution = false,
			bool isNewInstanceRequired = false,
			IArgsTuple newInstanceFactoryArgs = null,
			bool preventNewInstanceInitialization = false,
			IDependencyResolutionModel primaryResolutionModel = null,
			IDependencyResolutionSelectCriterion<TDependency> selectCriterion = null,
			IDisposeRegistry disposeRegistry = null)
			: base(
					dependencyType: dependencyTypeConstraint.Arg(nameof(dependencyTypeConstraint)).EnsureCompatible(__DependencyType).Value ?? __DependencyType,
					ensureResolution: ensureResolution,
					isNewInstanceRequired: isNewInstanceRequired,
					newInstanceFactoryArgs: newInstanceFactoryArgs,
					preventNewInstanceInitialization: preventNewInstanceInitialization,
					primaryResolutionModel: primaryResolutionModel,
					selectCriterion: selectCriterion,
					disposeRegistry: disposeRegistry) {
			_selectCriterion = selectCriterion;
		}

		public new IDependencyResolutionSelectCriterion<TDependency> SelectCriterion
			=> ReadDA(ref _selectCriterion);

		protected override void Dispose(bool explicitDispose) {
			_selectCriterion = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}