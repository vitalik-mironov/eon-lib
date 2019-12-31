using Eon.Context;
using Eon.Threading.Tasks;

namespace Eon {

	/// <summary>
	/// Defines the factory of an instance of <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">Type of an instance.</typeparam>
	public interface IFactory<out T> {

		/// <summary>
		/// Creates a new instance of <typeparamref name="T"/>.
		/// <para>Can't return <see langword="null"/>.</para>
		/// </summary>
		/// <param name="ctx">
		/// Operation context.
		/// </param>
		/// <returns>Instance of <typeparamref name="T"/>.</returns>
		T Create(IContext ctx = default);

		/// <summary>
		/// Creates a new instance of <typeparamref name="T"/>.
		/// <para>Can't return <see langword="null"/>.</para>
		/// </summary>
		/// <param name="ctx">
		/// Operation context.
		/// </param>
		/// <returns>Instance of <typeparamref name="T"/>.</returns>
		ITaskWrap<T> CreateAsync(IContext ctx = default);

	}

}