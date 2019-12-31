using System.Collections.Generic;

namespace Eon.Collections {

	public static class QueueUtilities {

		public static IReadOnlyList<T> TryDequeueAll<T>(this Queue<T> queue) {
			queue.EnsureNotNull(nameof(queue));
			//
			if (queue.Count > 0) {
				var array = new T[ queue.Count ];
				queue.CopyTo(array: array, arrayIndex: 0);
				var result = new ListReadOnlyWrap<T>(list: array);
				queue.Clear();
				return result;
			}
			else
				return ListReadOnlyWrap<T>.Empty;
		}

		public static IReadOnlyList<T> TryDequeue<T>(this Queue<T> queue, int maxCount) {
			queue.EnsureNotNull(nameof(queue));
			maxCount.Arg(nameof(maxCount)).EnsureNotLessThanZero();
			//
			if (queue.Count > 0 && maxCount > 0) {
				if (maxCount > queue.Count)
					return TryDequeueAll(queue: queue);
				else {
					var result = new List<T>();
					while (result.Count < maxCount && queue.Count > 0)
						result.Add(queue.Dequeue());
					return new ListReadOnlyWrap<T>(collection: result);
				}
			}
			else
				return ListReadOnlyWrap<T>.Empty;
		}

	}

}