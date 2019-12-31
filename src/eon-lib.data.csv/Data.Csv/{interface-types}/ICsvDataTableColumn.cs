
namespace DigitalFlare.Data.Csv {

	public interface ICsvDataTableColumn
		:IOxyDisposable {

		ICsvDataTable Table { get; }

		string Name { get; }

	}

}