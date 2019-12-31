using System;
using System.Threading;

namespace Eon.Threading {

	public static class SynchronizationContextUtilities {

		// TODO: Put strings into the resources.
		//
		public static SynchronizationContext RequireSynchronizationContext() {
			var context = SynchronizationContext.Current;
			if (context == null)
				throw new EonException(message: "В текущем потоке выполнения отсутствует контекст синхронизации.");
			else
				return context;
		}

	}

}
