using System;

using Eon.Data.Storage;

namespace Eon.Data.Persistence {

	public interface IStorageReferenceKeyProviderSettings
		:IReferenceKeyProviderSettings, IAsReadOnly<IStorageReferenceKeyProviderSettings> {

		IStorageSettings Storage { get; set; }

		string KeysStorageObjectSchemaName { get; set; }

		string KeysStorageObjectName { get; set; }

		Guid? ScopeUid { get; set; }

		string EntityTypeId { get; set; }

		int? KeyBufferPreferredSize { get; set; }

	}

}