
namespace Eon {

	public interface IOxyCloneable {

		object Clone();

		object Clone(ICloneContext cloneCtx);

	}

}