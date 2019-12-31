using Eon.ComponentModel.Properties.Annotations;

namespace Eon.Data.Persistence {

	/// <summary>
	/// Сущность данных.
	/// </summary>
	[PropertyValueCopyTypeOptions]
	public interface IPersistenceEntity<TReferenceKey>
		:IPersistenceEntity
		where TReferenceKey : struct {

		[PropertyValueCopyOptions]
		TReferenceKey? ReferenceKey { get; set; }

	}

}