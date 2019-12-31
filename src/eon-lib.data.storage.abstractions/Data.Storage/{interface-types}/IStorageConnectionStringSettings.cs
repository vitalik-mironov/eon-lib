using System.Threading.Tasks;

using Eon.Context;
using Eon.Description;
using Eon.Security.Cryptography;

namespace Eon.Data.Storage {

	/// <summary>
	/// Defines the base specs of the storage connection string.
	/// </summary>
	public interface IStorageConnectionStringSettings
		:ISettings, IAsReadOnly<IStorageConnectionStringSettings> {

		/// <summary>
		/// Gets/sets the option whether to skip secret text substitution (see <see cref="ISecretTextSubstitutionHandler"/>).
		/// </summary>
		bool SkipSecretTextSubstitution { get; set; }

		/// <summary>
		/// Gets the connection string in clear text.
		/// </summary>
		/// <param name="ctx">
		/// Operation context.
		/// </param>
		Task<string> GetConnectionStringRawAsync(IContext ctx = default);

	}

}