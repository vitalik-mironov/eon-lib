using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eon.ComponentModel {

	public interface IAsyncEventFireManager<TEventArgs>
		:IDisposable
		where TEventArgs : class {

		Task FireAsync(object sender, TEventArgs eventArgs, CancellationToken ct);

	}

}