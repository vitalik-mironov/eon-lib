using System;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Threading.Tasks;

namespace Eon {

	public static class AsyncOperatorUtilities {

		// TODO: Put strings into the resources.
		//
		public static TValue Execute<TValue>(this IAsyncOperator<TValue> op, int millisecondsTimeout) {
			op.EnsureNotNull(nameof(op));
			//
			if (op.IsCompleted)
				return op.Result;
			else if (millisecondsTimeout == 0)
				throw new TimeoutException(message: $"The operation was not completed within the specified timeout. Timeout expired.{Environment.NewLine}\tOperation:{op.FmtStr().GNLI2()}{Environment.NewLine}\tTimeout:{Environment.NewLine}{TimeSpan.FromMilliseconds(millisecondsTimeout).FmtStr().Constant().IndentLines2()}");
			else {
				var task = op.ExecuteAsync().Unwrap();
				if (task.Wait(millisecondsTimeout: millisecondsTimeout))
					return task.Result;
				else
					throw new TimeoutException(message: $"The operation was not completed within the specified timeout. Timeout expired.{Environment.NewLine}\tOperation:{op.FmtStr().GNLI2()}{Environment.NewLine}\tTimeout:{Environment.NewLine}{TimeSpan.FromMilliseconds(millisecondsTimeout).FmtStr().Constant().IndentLines2()}");
			}
		}

		public static TValue Execute<TValue>(this IAsyncOperator<TValue> op)
			=> Execute(op: op, millisecondsTimeout: TaskUtilities.DefaultAsyncTimeoutMilliseconds);

		public static IAsyncOperator<TValue> Create<TValue>(Func<TValue> factory, bool ownsResult, bool isReexecutable = false)
			=> new AsyncOperator<TValue>(factory: factory, ownsResult: ownsResult, isReexecutable: isReexecutable);

		public static IAsyncOperator<TValue> Create<TValue>(Func<Task<TValue>> factory, bool ownsResult, bool isReexecutable = false)
			=> new AsyncOperator<TValue>(asyncFactory: factory, ownsResult: ownsResult, isReexecutable: isReexecutable);

		public static IAsyncOperator<TValue> Create<TValue>(Func<IContext, Task<TValue>> factory, bool ownsResult, bool isReexecutable = false)
			=> new AsyncOperator<TValue>(asyncFactory: factory, ownsResult: ownsResult, isReexecutable: isReexecutable);

		public static IAsyncOperator<TValue> Create<TValue>(TValue result, bool ownsResult)
			=> new AsyncOperator<TValue>(result: result, ownsResult: ownsResult);

	}

}