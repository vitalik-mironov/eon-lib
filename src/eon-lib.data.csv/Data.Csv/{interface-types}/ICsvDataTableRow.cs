
namespace DigitalFlare.Data.Csv {

	public interface ICsvDataTableRow
		:IOxyDisposable {

		ICsvDataTable Table { get; }

		string this[string columnName] { get; }

		bool TryGetValue(string columnName, out string value);

	}

}