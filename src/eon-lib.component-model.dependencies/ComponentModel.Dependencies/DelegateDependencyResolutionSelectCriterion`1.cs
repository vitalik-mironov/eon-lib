using System;

namespace Eon.ComponentModel.Dependencies {

	public sealed class DependencyResolutionSelectCriterionFunc<TDependency>
		:IDependencyResolutionSelectCriterion<TDependency>
		where TDependency : class {

		readonly Func<TDependency, bool> _selectCriterionFunc;

		public DependencyResolutionSelectCriterionFunc(Func<TDependency, bool> func) {
			func.EnsureNotNull(nameof(func));
			//
			//
			_selectCriterionFunc = func;
		}

		public bool IsMatch(object instance) {
			instance.EnsureNotNull(nameof(instance));
			//
			if (!(instance is TDependency instanceAsTDependency))
				return false;
			else
				return IsMatch(instanceAsTDependency);
		}

		public bool IsMatch(TDependency instance) {
			instance.EnsureNotNull(nameof(instance));
			//
			return _selectCriterionFunc(instance);
		}

	}

}