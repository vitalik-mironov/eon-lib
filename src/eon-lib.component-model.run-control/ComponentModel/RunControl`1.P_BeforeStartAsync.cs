using System.Threading.Tasks;

using Eon.Context;

namespace Eon.ComponentModel {

	public sealed partial class RunControl<TComponent> {

		// TODO: Put strings into the resources.
		//
		async Task P_BeforeStartAsync(IRunControlAttemptState state) {
			state.EnsureNotNull(nameof(state));
			//
			state.Context.ThrowIfCancellationRequested();
			(Component as IOxyDisposable)?.EnsureNotDisposeState(considerDisposeRequest: true);
			var beforeStartTaskFactory = ReadDA(ref _beforeStartTaskFactory, considerDisposeRequest: true);
			if (!(beforeStartTaskFactory is null)) {
				var beforeStartTask = beforeStartTaskFactory(arg: state);
				if (beforeStartTask is null)
					throw
						new EonException(message: $"Невозможно выполнить операцию предзапуска компонента, так как функция создания задачи выполнения предзапуска возвратила недопустимый результат '{beforeStartTask.FmtStr().G()}'.");
				await beforeStartTask.ConfigureAwait(false);
			}
			P_FireBeforeStart(new RunControlBeforeStartEventArgs(state: state));
		}

	}

}