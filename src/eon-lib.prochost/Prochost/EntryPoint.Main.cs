using System.Threading.Tasks;
using Eon.Prochost.Internal;

namespace Eon.Prochost {

	public static partial class EntryPoint {

#if DEBUG

		static async Task Main(string[ ] commandLineArgs)
			=> await MainAsync(commandLineArgs: commandLineArgs, callback: DebugRunCallback.Callback).ConfigureAwait(false);

#else

		static async Task Main(string[ ] commandLineArgs)
			=> await MainAsync(commandLineArgs: commandLineArgs, callback: null).ConfigureAwait(false);

#endif

	}

}