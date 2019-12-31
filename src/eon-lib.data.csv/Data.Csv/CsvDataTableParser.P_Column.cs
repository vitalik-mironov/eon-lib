#if TRG_NETSTANDARD1_5 || TRG_SL5
using System;
using System.Diagnostics;

namespace DigitalFlare.Data {

	public partial class CsvDataTableParser {

#region Nested types

		//[DebuggerNonUserCode]
		//[DebuggerStepThrough]
		sealed class P_Column
			:DisposableSlim, ICsvDataTableColumn {

			string _name;

			ICsvDataTable _table;

			// TODO: Put exception messages into the resources.
			//
			internal P_Column(ICsvDataTable table, string columnName) {
				if (table == null)
					throw new ArgumentNullException("table");
				if (columnName == null)
					throw new ArgumentNullException("columnName");
				if (!table.IsColumnNameValid(columnName))
					throw new ArgumentException(string.Format("Указанное имя колонки '{0}' не является допустимым.", columnName.TextView()), "columnName");
				_name = columnName;
				_table = table;
			}

			public string Name { get { return ReadDA(ref _name); } }

			public ICsvDataTable Table { get { return ReadDA(ref _table); } }

			protected override void Dispose(bool explicitDispose) {
				_name = null;
				_table = null;
				base.Dispose(explicitDispose);
			}

		}

#endregion

	}

}
#endif