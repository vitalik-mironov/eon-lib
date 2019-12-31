using System.Collections.Generic;
using System.IO;

namespace DigitalFlare.Data.Csv {

	public interface ICsvDataTableParser {

		ICsvDataTable Parse(TextReader inputTextReader);

		IEqualityComparer<string> ColumnNameEqualityComparer { get; }

		IComparer<string> ColumnNameComparer { get; }

	}

}