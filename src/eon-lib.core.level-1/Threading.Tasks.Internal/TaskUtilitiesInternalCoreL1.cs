using System;
using System.Threading.Tasks;

namespace Eon.Threading.Tasks.Internal {

	internal static class TaskUtilitiesInternalCoreL1 {

		// TODO: Put strings into the resources.
		//
		static internal void SetException(Task task, Action<Exception> setter) {
			task.EnsureNotNull(nameof(task));
			setter.EnsureNotNull(nameof(setter));
			//
			if (task.IsFaulted) {
				AggregateException taskException = null;
				try {
					taskException = task.Exception;
					var rethrowFlattenException = taskException.Flatten();
					setter(rethrowFlattenException.InnerExceptions.Count == 1 ? rethrowFlattenException.InnerExceptions[ 0 ] : rethrowFlattenException);
				}
				catch (Exception exception) {
					if (taskException is null)
						throw;
					else
						throw new AggregateException(taskException, exception);
				}
			}
			else
				throw new EonException(message: $"Invalid operation. Specified task is not in valid completion state.");
		}

	}

}