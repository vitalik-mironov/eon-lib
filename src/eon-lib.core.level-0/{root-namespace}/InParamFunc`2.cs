namespace Eon {

	public delegate TResult InParamFunc<TArg, out TResult>(in TArg arg) where TArg : struct;

}