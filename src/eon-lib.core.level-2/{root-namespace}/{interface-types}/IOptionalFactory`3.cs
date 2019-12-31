using Eon.Context;
using Eon.Threading.Tasks;

namespace Eon {

	/// <summary>
	/// Defines the optional factory of an instance of <typeparamref name="T"/>.
	/// <para>Optional factory creates an instance if it can create it using the specified argument(s) (see <see cref="CanCreate(TArg1,TArg2)"/>.</para>
	/// </summary>
	/// <typeparam name="TArg1">Type of argument(s) required for instance creation.</typeparam>
	/// <typeparam name="TArg2">Type of argument(s) required for instance creation.</typeparam>
	/// <typeparam name="T">Type of an instance.</typeparam>
	public interface IOptionalFactory<in TArg1, in TArg2, out T> {

		/// <summary>
		/// Indicates may or not this factory create an instance using the specified argument(s).
		/// </summary>
		/// <param name="arg">
		/// Argument(s).
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="arg2">Argumen(s).</param>
		bool CanCreate(TArg1 arg, TArg2 arg2);

		/// <summary>
		/// Creates a new instance of <typeparamref name="T"/> using the specified argument(s).
		/// <para>Can't return <see langword="null"/>.</para>
		/// </summary>
		/// <param name="arg1">
		/// Argument(s) required to create an instance.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="arg2">Argumen(s).</param>
		/// <param name="ctx">
		/// Operation context.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <returns>Instance of <typeparamref name="T"/>.</returns>
		T Create(TArg1 arg1, TArg2 arg2, IContext ctx);

		/// <summary>
		/// Creates a new instance of <typeparamref name="T"/> using the specified argument(s).
		/// <para>Can't return <see langword="null"/>.</para>
		/// </summary>
		/// <param name="arg1">
		/// Argument(s) required to create an instance.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <param name="arg2">Argumen(s).</param>
		/// <param name="ctx">
		/// Operation context.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </param>
		/// <returns>Instance of <typeparamref name="T"/>.</returns>
		ITaskWrap<T> CreateAsync(TArg1 arg1, TArg2 arg2, IContext ctx);

	}

}