using System.Threading.Tasks;

namespace Eon.Prochost {

	public static partial class EntryPoint {

		public static async Task MainAsync(string[ ] commandLineArgs, RunCallback callback = default) {
			if (!(callback is null))
				await callback(state: null);
		}

	}

}