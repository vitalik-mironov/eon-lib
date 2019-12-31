
namespace Eon {

	public interface IArgsTuple<out TArg1, out TArg2, out TArg3>
		:IArgsTuple<TArg1, TArg2> {

		TArg3 Arg3 { get; }

	}

}