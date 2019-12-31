using System.Threading.Tasks;

using Eon.Description;
using Eon.Runtime;

namespace Eon.Context {

	/// <summary>
	/// Encapsulates a method that creates XApp startup context (see <see cref="IXAppStartupContext"/>).
	/// <para>Can't return <see langword="null"/>.</para>
	/// </summary>
	/// <typeparam name="TDescription">
	/// App description type constraint.
	/// </typeparam>
	/// <param name="runtime">
	/// XApp runtime service.
	/// <para>Can't be <see langword="null"/>.</para>
	/// </param>
	/// <param name="locator">
	/// App description locator (reference). See <see cref="IXAppStartupContext{TAppDescription}.DescriptionLocator"/>.
	/// <para>Can be <see langword="null"/>.</para>
	/// <para>Must be <see langword="null"/> if <paramref name="description"/> is not <see langword="null"/>.</para>
	/// <para>If <paramref name="locator"/> and <paramref name="description"/> are <see langword="null"/>, then locator or description, that be used by context to be created, should be determined by the method.</para>
	/// </param>
	/// <param name="description">
	/// App description. See <see cref="IXAppStartupContext{TAppDescription}.DescriptionLocator"/>.
	/// <para>Can be <see langword="null"/>.</para>
	/// <para>Must be <see langword="null"/> if <paramref name="locator"/> is not <see langword="null"/>.</para>
	/// <para>If <paramref name="locator"/> and <paramref name="description"/> are <see langword="null"/>, then locator or description, that be used by context to be created, should be determined by the method.</para>
	/// </param>
	/// <param name="startupHints">
	/// Hints to the component, acting as XApp host, which behavior should be applied on app startup (see <see cref="IXAppStartupContext.HostHints"/>).
	/// </param>
	/// <param name="containerControl">
	/// XApp container control.
	/// </param>
	/// <param name="outerCtx">
	/// Context, acting as an outer for the context to be created.
	/// </param>
	public delegate Task<IXAppStartupContext<TDescription>> XAppStartupContextFactory<TDescription>(
		IXAppRuntimeService runtime,
		DescriptionLocator locator = default,
		TDescription description = default,
		ArgumentPlaceholder<XAppStartupContextHostHints> startupHints = default,
		ArgumentPlaceholder<IXAppContainerControl> containerControl = default,
		IContext outerCtx = default)
		where TDescription : class, IXAppDescription;

}