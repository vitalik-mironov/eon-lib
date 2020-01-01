
namespace Eon {

	public interface IEonCloneableOrigin
		:IEonCloneable {

		bool HasCloneOrigin { get; }

		object CloneOrigin { get; }

	}

}