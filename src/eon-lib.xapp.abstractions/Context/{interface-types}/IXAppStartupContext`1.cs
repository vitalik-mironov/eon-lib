using Eon.Description;
using Eon.Threading.Tasks;

namespace Eon.Context {

	/// <summary>
	/// XApp startup context.
	/// </summary>
	/// <typeparam name="TAppDescription">Type of XApp description (configuration).</typeparam>
	public interface IXAppStartupContext<out TAppDescription>
		:IXAppStartupContext
		where TAppDescription : class, IXAppDescription {

		/// <summary>
		/// Gets description locator (reference) of app to be started.
		/// <para>Can be <see langword="null"/>.</para>
		/// </summary>
		DescriptionLocator DescriptionLocator { get; }

		/// <summary>
		/// Loads description of app to be started.
		/// <para>Idempotent. Description loads once. All subsequent calls of this method returns result of the first call.</para>
		/// </summary>
		/// <param name="ctx">
		/// Operation context.
		/// </param>
		ITaskWrap<TAppDescription> LoadDescriptionAsync(IContext ctx = default);

	}

}