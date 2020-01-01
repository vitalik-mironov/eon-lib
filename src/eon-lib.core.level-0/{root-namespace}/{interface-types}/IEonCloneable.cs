
namespace Eon {

	public interface IEonCloneable {

		object Clone();

		object Clone(ICloneContext cloneCtx);

	}

}