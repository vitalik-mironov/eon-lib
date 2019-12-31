using System.Threading;
using System.Threading.Tasks;

using Eon.Context;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.ComponentModel {

	public class RunControlAttemptState
		:RunControlAttemptPropertiesBase, IRunControlAttemptState {

		Task<IRunControlAttemptSuccess> _successCompletion;

		public RunControlAttemptState(in RunControlAttemptStateFactoryArgs args, ArgumentPlaceholder<object> overrideTag = default)
			: base(
					runControl: args.RunControl,
					isStart: args.IsStart,
					attemptNumber: args.AttemptNumber,
					succeededAttemptCountBefore: args.SucceededAttemptCountBefore,
					tag: overrideTag.HasExplicitValue ? overrideTag.ExplicitValue : args.Tag,
					correlationId: args.Context.ArgProp($"{nameof(args)}.{nameof(args.Context)}").EnsureNotNull().Value.FullCorrelationId) {
			//
			Completion = args.Completion.ArgProp($"{nameof(args)}.{nameof(args.Completion)}").EnsureNotNull().Value;
			//
			Context = args.Context;
			Ct = args.Context.Ct();
		}

		public CancellationToken Ct { get; private set; }

		public IContext Context { get; private set; }

		public Task<IRunControlAttemptSuccess> Completion { get; private set; }

		public Task<IRunControlAttemptSuccess> SuccessCompletion
			=>
			itrlck.UpdateIfNull(
				location: ref _successCompletion,
				factory: () => Completion.ContinueWith(continuationFunction: locTask => locTask.Result, cancellationToken: CancellationToken.None, continuationOptions: TaskContinuationOptions.OnlyOnRanToCompletion, scheduler: TaskScheduler.Default));

	}

}