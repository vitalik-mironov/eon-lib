using Microsoft.Extensions.Hosting;

namespace Eon.Hosting {

	/// <summary>
	/// Defines a hosted service (<see cref="IHostedService"/>) running XApp <see cref="IXApp{TDescription}"/>.
	/// </summary>
	public interface IXAppHostedService
		:IEonDisposable, IHostedService { }

}