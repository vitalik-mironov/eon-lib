using Eon.Context;
using Eon.Threading.Tasks;

namespace Eon {

	/// <summary>
	/// Defines the optional factory of an instance of <typeparamref name="T"/>.
	/// <para>Optional factory creates an instance if it can create it using the specified argument(s) (see <see cref="CanCreate(TArg)"/>.</para>
	/// </summary>
	/// <typeparam name="TArg">Type of argument(s) required for instance creation.</typeparam>
	/// <typeparam name="T">Type of an instance.</typeparam>
	public interface IOptionalFactory<in TArg, out T> {

		/// <summary>
		/// Indicates may or not this factory create an instance using the specified argument(s).
		/// </summary>
		/// <param name="arg">
		/// Argument(s).
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		bool CanCreate(TArg arg);

		/// <summary>
		/// Creates a new instance of <typeparamref name="T"/> using the specified argument(s).
		/// <para>Can't return <see langword="null"/>.</para>
		/// </summary>
		/// <param name="arg">
		/// Argument(s) required to create an instance.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="ctx">
		/// Operation context.
		/// </param>
		/// <returns>Instance of <typeparamref name="T"/>.</returns>
		T Create(TArg arg, IContext ctx = default);

		/// <summary>
		/// Creates a new instance of <typeparamref name="T"/> using the specified argument(s).
		/// <para>Can't return <see langword="null"/>.</para>
		/// </summary>
		/// <param name="arg">
		/// Argument(s) required to create an instance.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="ctx">
		/// Operation context.
		/// </param>
		/// <returns>Instance of <typeparamref name="T"/>.</returns>
		ITaskWrap<T> CreateAsync(TArg arg, IContext ctx = default);

	}

}