using System.Threading.Tasks;

namespace Eon.Prochost {

	public delegate Task RunCallback(RunCallbackState state = default, string[ ] commandLineArgs = default);

}