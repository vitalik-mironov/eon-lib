using System.Threading;
using System.Threading.Tasks;

using Eon.Context;

namespace Eon.ComponentModel {

	/// <summary>
	/// Provides initialization support.
	/// </summary>
	public interface IInitializable {

		Task InitializeAsync();

		Task InitializeAsync(CancellationToken ct);

		Task InitializeAsync(IContext ctx = default);

	}

}