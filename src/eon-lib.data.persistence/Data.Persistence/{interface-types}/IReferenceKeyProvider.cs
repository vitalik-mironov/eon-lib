using System;

namespace Eon.Data.Persistence {

	/// <summary>
	/// Defines basic specs of the entity reference key provider.
	/// </summary>
	public interface IReferenceKeyProvider
		:IDisposeNotifying {

		Type KeyType { get; }

	}

}