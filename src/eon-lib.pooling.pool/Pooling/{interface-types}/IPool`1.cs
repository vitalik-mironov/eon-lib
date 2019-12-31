using Eon.Context;
using Eon.Threading;
using Eon.Threading.Tasks;

namespace Eon.Pooling {

	public interface IPool<out T>
		:IOxyDisposable
		where T : class {

		ITaskWrap<IUsing<T>> TakeAsync(IContext ctx = default);

		int MaxSize { get; }

		TimeoutDuration ItemPreferredSlidingTtl { get; }

		string DisplayName { get; }

		(int size, int free)? GetBaseStats(bool disposeTolerant = default);

	}

}