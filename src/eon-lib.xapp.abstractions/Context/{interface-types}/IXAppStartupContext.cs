namespace Eon.Context {

	/// <summary>
	/// Defines a XApp startup context (see <seealso cref="IXApp{TDescription}"/>).
	/// </summary>
	public interface IXAppStartupContext
		:IContext {

		/// <summary>
		/// Gets XApp container control (see <see cref="IXApp{TDescription}.ContainerControl"/>).
		/// <para>Can be <see langword="null"/>.</para>
		/// </summary>
		IXAppContainerControl ContainerControl { get; }

		/// <summary>
		/// Gets the hints to the component, acting as XApp host, which behavior should be applied on app startup.
		/// </summary>
		XAppStartupContextHostHints HostHints { get; }

	}

}