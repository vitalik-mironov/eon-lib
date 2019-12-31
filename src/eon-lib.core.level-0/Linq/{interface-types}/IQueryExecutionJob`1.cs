using System.Threading;
using System.Threading.Tasks;
using Eon.Threading.Tasks;

namespace Eon.Linq {

	public interface IQueryExecutionJob<out TResult>
		:IQueryExecutionJob {

		ITaskWrap<IQueryExecutionJobResult<TResult>> ExecuteAsync(CancellationToken cancellationToken, TaskCreationOptions taskCreationOptions);

		IQueryExecutionJobResult<TResult> Execute();

	}

}