
namespace Eon {

	public interface IOxyCloneableOrigin
		:IOxyCloneable {

		bool HasCloneOrigin { get; }

		object CloneOrigin { get; }

	}

}