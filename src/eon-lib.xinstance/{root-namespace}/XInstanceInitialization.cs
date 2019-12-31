using Eon.Context;

namespace Eon {

	/// <summary>
	/// Defines a method implementing initialization logic of <see cref="IXInstance"/>-component.
	/// </summary>
	/// <param name="instance">
	/// Initializing <see cref="IXInstance"/>-component.
	/// <para>Can't be <see langword="null"/>.</para>
	/// </param>
	/// <param name="ctx">
	/// Opeation context.
	/// </param>
	public delegate void XInstanceInitialization(IXInstance instance, IContext ctx = default);

}