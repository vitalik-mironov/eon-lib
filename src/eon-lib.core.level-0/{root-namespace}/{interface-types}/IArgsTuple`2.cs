namespace Eon {

	public interface IArgsTuple<out TArg1, out TArg2>
		:IArgsTuple<TArg1> {

		TArg2 Arg2 { get; }

	}

}