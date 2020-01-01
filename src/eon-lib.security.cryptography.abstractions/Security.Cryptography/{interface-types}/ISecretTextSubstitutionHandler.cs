using System.Threading.Tasks;

using Eon.Context;

namespace Eon.Security.Cryptography {

	public interface ISecretTextSubstitutionHandler
		:IEonDisposable {

		Task<string> SubstituteAsync(string template, IContext ctx = default);

	}

}