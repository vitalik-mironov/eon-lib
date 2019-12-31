
namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Описание использования (назначения, применения и т.д.) к.л. требуемой функциональной зависимости.
	/// </summary>
	public interface IDependencyUsageDesignation {

		bool IsMatch(IDependencyUsageDesignation designation);

	}

}