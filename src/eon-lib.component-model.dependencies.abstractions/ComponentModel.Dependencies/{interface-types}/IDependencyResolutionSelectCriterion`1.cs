
namespace Eon.ComponentModel.Dependencies {

	public interface IDependencyResolutionSelectCriterion<TDependency>
		:IDependencyResolutionSelectCriterion
		where TDependency : class {

		bool IsMatch(TDependency instance);

	}

}