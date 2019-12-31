using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eon.ComponentModel {

	public delegate IRunControl<TComponent> RunControlFactoryDelegate<TComponent>(
		RunControlOptions options,
		TComponent component,
		RunControlAttemptStateFactory attemptState,
		Func<IRunControlAttemptState, Task> beforeStart,
		Func<IRunControlAttemptState, Task> start,
		Func<Task> stop,
		CancellationToken stopToken);

}