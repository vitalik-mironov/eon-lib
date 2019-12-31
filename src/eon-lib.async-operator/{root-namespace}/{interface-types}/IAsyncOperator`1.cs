using System.Threading;

using Eon.Context;
using Eon.Threading.Tasks;

namespace Eon {

	/// <summary>
	/// Provides support for lazy asynchronous operation.
	/// </summary>
	/// <typeparam name="TResult">Type of operation result.</typeparam>
	public interface IAsyncOperator<out TResult>
		:IAsyncOperatorBase {

		TResult Result { get; }

		ITaskWrap<TResult> ExecuteAsync(IContext ctx = default);

		ITaskWrap<TResult> ExecuteAsync(CancellationToken ct);

		ITaskWrap<TResult> ReexecuteAsync(IContext ctx = default);

		ITaskWrap<TResult> ReexecuteAsync(CancellationToken ct);

	}

}