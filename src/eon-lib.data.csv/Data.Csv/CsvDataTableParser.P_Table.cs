#if TRG_NETSTANDARD1_5 || TRG_SL5
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using rt = DigitalFlare.Resources.XResource.XResourceTreeManager;

namespace DigitalFlare.Data {

	public partial class CsvDataTableParser {

#region Nested types

		//[DebuggerNonUserCode]
		//[DebuggerStepThrough]
		sealed class P_Table
			:DisposableSlim, ICsvDataTable {

			ICsvDataTableColumn[ ] _columns;

			Dictionary<string, int> _columnNameIndexMapping;

			ICsvDataTableRow[ ] _rows;

			Type _rowType;

			IEqualityComparer<string> _columnNameEqualityComparer;

			// TODO: Put exception messages into the resources.
			//
			internal P_Table(Type rowType, IEqualityComparer<string> columnNameEqualityComparer) {
				if (rowType == null)
					throw new ArgumentNullException("rowType");
				if (columnNameEqualityComparer == null)
					throw new ArgumentNullException("columnNameEqualityComparer");
				_rowType = rowType;
				_columnNameEqualityComparer = columnNameEqualityComparer;
			}

			public int ColumnCount {
				get {
					var columns = ReadDA(ref _columns);
					return columns == null ? 0 : columns.Length;
				}
			}

			public IEnumerable<ICsvDataTableColumn> Columns {
				get {
					var columns = ReadDA(ref _columns);
					if (columns == null)
						yield break;
					ICsvDataTableColumn column;
					for (var i = 0; i < columns.Length; i++) {
						column = columns[ i ];
						if (column == null)
							EnsureNotDisposeState();
						yield return column;
					}
				}
			}

			public int RowCount {
				get {
					var rows = ReadDA(ref _rows);
					return rows == null ? 0 : rows.Length;
				}
			}

			public IEnumerable<ICsvDataTableRow> Rows {
				get {
					var rows = ReadDA(ref _rows);
					if (rows == null)
						yield break;
					ICsvDataTableRow row;
					for (var i = 0; i < rows.Length; i++) {
						row = rows[ i ];
						if (row == null)
							EnsureNotDisposeState();
						yield return row;
					}
				}
			}

			protected override void Dispose(bool explicitDispose) {
				if (explicitDispose) {
					var columns = _columns;
					if (columns != null && columns.Length > 0) {
						for (var i = 0; i < columns.Length; i++) {
							columns[ i ].TryDispose();
							columns[ i ] = null;
						}
					}
					var rows = _rows;
					if (rows != null && rows.Length > 0) {
						for (var i = 0; i < rows.Length; i++) {
							rows[ i ].TryDispose();
							rows[ i ] = null;
						}
					}
					var columnNameIndexMapping = _columnNameIndexMapping;
					if (columnNameIndexMapping != null)
						columnNameIndexMapping.Clear();
				}
				_columns = null;
				_rows = null;
				_rowType = null;
				_columnNameIndexMapping = null;
				_columnNameEqualityComparer = null;
				base.Dispose(explicitDispose);
			}

			public Type RowType { get { return ReadDA(ref _rowType); } }

			// TODO: Put exception messages into the resources.
			//
			internal void Internal_SetColumns(IEnumerable<ICsvDataTableColumn> columns) {
				var columnsArray = columns
					.Argument(() => columns)
					.EnsureNotNull()
					.EnsureNotEmpty()
					.EnsureNoNullElements()
					.Value
					.ToArray();
				var columnNameIndexMapping = new Dictionary<string, int>(ReadDA(ref _columnNameEqualityComparer));
				string columnName;
				for (var i = 0; i < columnsArray.Length; i++) {
					if (!object.ReferenceEquals(columnsArray[ i ].Table, this))
						throw new ArgumentException("Одна из указанных колонок не принадлежит данной таблице.", "columns");
					columnName = columnsArray[ i ].Name;
					if (columnName == null)
						throw new ArgumentException("Одна из колонок не имеет имени.", "columns");
					if (columnNameIndexMapping.ContainsKey(columnName))
						throw new ArgumentException(string.Format("Одна из колонок имеет неуникальное имя '{0}'.", columnName.TextView()), "columns");
					columnNameIndexMapping.Add(columnName, i);
				}
				if (WriteDA(ref _columns, columnsArray, null) != null)
					throw new DigitalFlareApplicationException("Набор колонок для таблицы уже ранее был установлен.");
				WriteDA(ref _columnNameIndexMapping, columnNameIndexMapping);
			}

			// TODO: Put exception messages into the resources.
			//
			internal void Internal_SetRows(IEnumerable<ICsvDataTableRow> rows) {
				var rowsArray =
					rows
					.Argument(nameof(rows))
					.EnsureNotNull()
					.EnsureNoNullElements()
					.Value
					.ToArray();
				var rowType = ReadDA(ref _rowType);
				for (var i = 0; i < rowsArray.Length; i++) {
					if (!ReferenceEquals(rowsArray[ i ].Table, this))
						throw new ArgumentException($"Одна из указанных строк (в позиции '{i.FmtStr().Decimal()}') не принадлежит данной таблице.", nameof(rows));
					else if (!rowType.IsAssignableFrom(rowsArray[ i ].GetType()))
						throw 
							new ArgumentException(
								$"Тип строки '{rowsArray[ i ].GetType().FmtStr().G()}' (в позиции '{i.FmtStr().Decimal()}') не совместим с типом '{rowType.FmtStr().G()}', указанным для данной таблицы.", 
								nameof(rows));
				}
				var columns = ReadDA(ref _columns);
				if (columns.IsNullOrEmpty())
					throw new DigitalFlareApplicationException("В данной таблице отсутствуют колонки. Добавление строк невозможно.");
				if (WriteDA(ref _rows, rowsArray, null) != null)
					throw new DigitalFlareApplicationException("Набор строк для таблицы уже ранее был установлен.");
			}

			public bool IsColumnNameValid(string columnName) {
				if (string.IsNullOrEmpty(columnName))
					throw new ArgumentException(rt.Format(typeof(string), "CanNotNullOrEmpty"), "columnName");
				return true;
			}

			// TODO: Put exception messages into the resources.
			//
			public int GetColumnOrdinal(string columnName) {
				if (columnName == null)
					throw new ArgumentNullException("columnName");
				if (!IsColumnNameValid(columnName))
					throw new ArgumentException(string.Format("Указанное имя колонки '{0}' не является допустимым.", columnName.TextView()), "columnName");
				var columnNameIndexMapping = ReadDA(ref _columnNameIndexMapping);
				int result;
				if (columnNameIndexMapping == null || !columnNameIndexMapping.TryGetValue(columnName, out result)) {
					EnsureNotDisposeState();
					throw new KeyNotFoundException(string.Format("Колонка с именем '{0}' отсутствует в таблице.", columnName.TextView()));
				}
				return result;
			}

			public bool HasColumn(string columnName) {
				if (columnName == null)
					throw new ArgumentNullException("columnName");
				if (!IsColumnNameValid(columnName))
					throw new ArgumentException(string.Format("Указанное имя колонки '{0}' не является допустимым.", columnName.TextView()), "columnName");
				var columnNameIndexMapping = ReadDA(ref _columnNameIndexMapping);
				return columnNameIndexMapping != null && columnNameIndexMapping.ContainsKey(columnName);
			}

		}

#endregion

	}

}
#endif