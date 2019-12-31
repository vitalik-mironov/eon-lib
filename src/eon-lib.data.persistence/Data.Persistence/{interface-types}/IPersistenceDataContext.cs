using System;
using System.Data;

using Eon.ComponentModel;
using Eon.Transactions;

namespace Eon.Data.Persistence {

	/// <summary>
	/// Defines the context of persistent data storage.
	/// </summary>
	public interface IPersistenceDataContext
		:IDataContext2, IInitializable {

		/// <summary>
		/// Gets the provider of this context.
		/// <para>Consider, that provider owns this context.</para>
		/// <para>Can't be <see langword="null"/>.</para>
		/// </summary>
		new IPersistenceDataContextProvider<IPersistenceDataContext> Provider { get; }

		/// <summary>
		/// Gets the data change context (insert, update, delete).
		/// <para>Can't return <see langword="null"/>.</para>
		/// </summary>
		IPersistenceStorageOperationContext<IPersistenceEntity> GetStorageOperationContext();

		/// <summary>
		/// Gets the type-discriminator of this context (see <see cref="IPersistenceDataContext{TContextDiscriminator}"/>).
		/// <para>Can't be <see langword="null"/>.</para>
		/// </summary>
		Type ContextDiscriminatorType { get; }

		ITransactionScopeProxy BeginTx(IsolationLevel level);

	}

}