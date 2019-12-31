using System;
using System.Collections.Generic;

namespace DigitalFlare.Data.Csv {

	public interface ICsvDataTable
		:IOxyDisposable {

		int RowCount { get; }

		IEnumerable<ICsvDataTableRow> Rows { get; }

		int ColumnCount { get; }

		IEnumerable<ICsvDataTableColumn> Columns { get; }

		Type RowType { get; }

		bool IsColumnNameValid(string columnName);

		int GetColumnOrdinal(string columnName);

		bool HasColumn(string columnName);

	}

}