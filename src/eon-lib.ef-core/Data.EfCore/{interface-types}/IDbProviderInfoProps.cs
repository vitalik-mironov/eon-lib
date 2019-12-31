namespace Eon.Data.EfCore {

	public interface IDbProviderInfoProps
		:IAsReadOnly<IDbProviderInfoProps>, IValidatable {

		string ProviderInvariantName { get; set; }

		string ProviderManifestToken { get; set; }

		string DefaultSchemaName { get; set; }

	}

}