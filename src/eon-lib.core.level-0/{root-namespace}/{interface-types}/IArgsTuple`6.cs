
namespace Eon {

	public interface IArgsTuple<out TArg1, out TArg2, out TArg3, out TArg4, out TArg5, out TArg6>
		:IArgsTuple<TArg1, TArg2, TArg3, TArg4, TArg5> {

		TArg6 Arg6 { get; }

	}

}