
namespace Eon {

	/// <summary>
	/// Defines the factory of an instance of <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="TArg1">Type of argument(s) required for instance creation.</typeparam>
	/// <typeparam name="TArg2">Type of argument(s) required for instance creation.</typeparam>
	/// <typeparam name="T">Type of an instance.</typeparam>
	public interface IFactory<in TArg1, in TArg2, out T>
		:IOptionalFactory<TArg1, TArg2, T> { }

}