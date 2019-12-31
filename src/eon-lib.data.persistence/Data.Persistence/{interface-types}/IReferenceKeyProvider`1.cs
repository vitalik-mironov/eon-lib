using System.Threading.Tasks;

using Eon.Context;

namespace Eon.Data.Persistence {

	/// <summary>
	/// Defines basic specs of the entity reference key provider.
	/// </summary>
	/// <typeparam name="TKey">Type of reference key value.</typeparam>
	public interface IReferenceKeyProvider<TKey>
		:IReferenceKeyProvider
		where TKey : struct {

		/// <summary>
		/// Takes the next available key.
		/// </summary>
		/// <param name="ctx">Operation context.</param>
		/// <returns></returns>
		Task<TKey> NextKeyAsync(IContext ctx = default);

	}

}