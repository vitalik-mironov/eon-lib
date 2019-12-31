
namespace Eon.ComponentModel.Dependencies.Description {

	public interface IDependencyBindingDescription
		:IDependencyBindingDescriptionBase {

		IDependencyTarget DependencyTarget { get; }

	}

}