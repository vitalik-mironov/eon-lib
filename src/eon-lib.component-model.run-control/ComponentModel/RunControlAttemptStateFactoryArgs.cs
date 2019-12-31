using System.Threading.Tasks;

using Eon.Context;

namespace Eon.ComponentModel {

	public readonly ref struct RunControlAttemptStateFactoryArgs {

		public readonly IRunControl RunControl;

		public readonly Task<IRunControlAttemptSuccess> Completion;

		public readonly IContext Context;

		public readonly bool IsStart;

		public readonly int AttemptNumber;

		public readonly int SucceededAttemptCountBefore;

		public readonly object Tag;

		public RunControlAttemptStateFactoryArgs(IRunControl runControl, Task<IRunControlAttemptSuccess> completion, IContext context, bool isStart, int attemptNumber, int succeededAttemptCountBefore, object tag = default) {
			RunControl = runControl;
			Completion = completion;
			Context = context;
			IsStart = isStart;
			AttemptNumber = attemptNumber;
			SucceededAttemptCountBefore = succeededAttemptCountBefore;
			Tag = tag;
		}

	}

}