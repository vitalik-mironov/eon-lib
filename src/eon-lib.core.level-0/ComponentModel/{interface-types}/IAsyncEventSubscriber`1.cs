using System.Threading;
using System.Threading.Tasks;

namespace Eon.ComponentModel {

	public interface IAsyncEventSubscriber<in TEventArgs>
		:IOxyDisposable
		where TEventArgs : class {

		Task HandleEventAsync(object sender, TEventArgs eventArgs, CancellationToken ct);

	}

}