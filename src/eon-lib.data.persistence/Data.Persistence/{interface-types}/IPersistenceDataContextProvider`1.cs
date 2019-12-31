using System.Threading.Tasks;

using Eon.Context;

namespace Eon.Data.Persistence {

	/// <summary>
	/// Defines the provider of the context of persistent data storage (see <see cref="IPersistenceDataContext"/>).
	/// </summary>
	/// <typeparam name="TContext">Context type constraint.</typeparam>
	public interface IPersistenceDataContextProvider<out TContext>
		:IDataContextProvider<TContext>
		where TContext : class, IPersistenceDataContext {

		/// <summary>
		/// Requires the provider of reference key of type <typeparamref name="TKey"/>.
		/// </summary>
		/// <typeparam name="TKey">Type of reference key.</typeparam>
		/// <param name="keyTypeDescriptor">Key type descriptor.</param>
		/// <param name="ctx">Operation context.</param>
		Task<IReferenceKeyProvider<TKey>> RequireReferenceKeyProviderAsync<TKey>(PersistenceEntityReferenceKeyTypeDescriptor keyTypeDescriptor, IContext ctx = default)
			where TKey : struct;

	}

}