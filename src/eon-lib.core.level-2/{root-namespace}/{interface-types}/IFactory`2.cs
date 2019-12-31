
namespace Eon {

	/// <summary>
	/// Defines the factory of an instance of <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="TArg">Type of argument(s) required for instance creation.</typeparam>
	/// <typeparam name="T">Type of an instance.</typeparam>
	public interface IFactory<in TArg, out T>
		:IOptionalFactory<TArg, T> { }

}