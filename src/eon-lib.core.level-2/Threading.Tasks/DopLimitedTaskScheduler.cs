using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Eon.Threading.Tasks {

	public class DopLimitedTaskScheduler
		:TaskScheduler {

		readonly int _maxDop;

		readonly MuttableVh<ImmutableList<Task>> _tasksQueue;

		int _processQueueRunningCount;

		[ThreadStatic]
		static bool __IsThreadProcessingTasks;

		public DopLimitedTaskScheduler(int maxDop) {
			maxDop.EnsureNotLessThanOne(nameof(maxDop));
			//
			_maxDop = maxDop;
			_tasksQueue = ImmutableList<Task>.Empty;
			_processQueueRunningCount = 0;
		}

		public sealed override int MaximumConcurrencyLevel
			=> _maxDop;

		protected override IEnumerable<Task> GetScheduledTasks()
			=> _tasksQueue.Value;

		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) {
			task.EnsureNotNull(nameof(task));
			if (!__IsThreadProcessingTasks)
				return false;
			else if (taskWasPreviouslyQueued)
				if (TryDequeue(task))
					return TryExecuteTask(task);
				else
					return false;
			else
				return TryExecuteTask(task);
		}

		// TODO: Put strings into the resources.
		//
		protected override void QueueTask(Task task) {
			task.EnsureNotNull(nameof(task));
			//
			if (_tasksQueue.UpdateValue(o => o.Contains(task) ? o : o.Add(task))) {
				for (; ; ) {
					var processQueueRunningCount = VolatileUtilities.Read(ref _processQueueRunningCount);
					if (processQueueRunningCount < _maxDop) {
						if (processQueueRunningCount == Interlocked.CompareExchange(ref _processQueueRunningCount, processQueueRunningCount + 1, processQueueRunningCount)) {
							P_ProcessQueue();
							break;
						}
					}
					else
						break;
				}
			}
			else
				throw new InvalidOperationException($"Указанная задача уже ранее была поставлена в очередь шедулера '{this}'.");
		}

		protected override bool TryDequeue(Task task) {
			task.EnsureNotNull(nameof(task));
			return _tasksQueue.UpdateValue(o => o.Remove(task));
		}

		void P_ProcessQueue() {
			TaskUtilities
			.RunOnDefaultScheduler(
				action:
					() => {
						try {
							__IsThreadProcessingTasks = true;
							for (; ; ) {
								var task = default(Task);
								_tasksQueue
									.UpdateValue(
										locTaskList => {
											if (locTaskList.Count < 1)
												return locTaskList;
											else {
												task = locTaskList[ 0 ];
												return locTaskList.RemoveAt(0);
											}
										});
								if (task == null) {
									InterlockedUtilities.Decrement(ref _processQueueRunningCount, 0);
									break;
								}
								else
									TryExecuteTask(task);
							}
						}
						finally {
							__IsThreadProcessingTasks = false;
						}
					},
				ct: CancellationToken.None);
		}

	}

}