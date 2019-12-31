
namespace Eon {

	public interface IArgsTuple<out TArg1, out TArg2, out TArg3, out TArg4>
		:IArgsTuple<TArg1, TArg2, TArg3> {

		TArg4 Arg4 { get; }

	}

}