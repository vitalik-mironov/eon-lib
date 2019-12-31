#if TRG_NETSTANDARD1_5 || TRG_SL5

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DigitalFlare.IO;
using DigitalFlare.Reflection;

namespace DigitalFlare.Data {

	//[DebuggerNonUserCode]
	//[DebuggerStepThrough]
	public sealed partial class CsvDataTableParser
		:ICsvDataTableParser {

		public const int MaxColumnNameLength = 64;

		public const int MaxRowCount = 262144;

		public CsvDataTableParser() { }

		// TODO: Put exception messages into the resources.
		//
		public ICsvDataTable Parse(TextReader inputTextReader) {
			if(inputTextReader == null)
				throw new ArgumentNullException("inputTextReader");
			P_Table table = null;
			var rows = new LinkedList<Row>();
			try {
				string textLine;
				string columnName;
				var rowCtor = default(Func<ICsvDataTable, IEnumerable<string>, Row>);
				var textLineNumber = 0;
				var textLineValues = new List<string>(48);
				var columnNamesOrder = default(Dictionary<string, int>);
				for(;;) {
					if(textLineNumber - 1 >= MaxRowCount)
						throw new DigitalFlareApplicationException(string.Format("Источник данных содержит слишком много строк. Макс. допустимое количество строк '{0}'.", MaxRowCount.TextViewInvariant("d")));
					textLine = inputTextReader.ReadLine();
					if(textLine == null)
						break;
					textLineNumber++;
					if(string.IsNullOrWhiteSpace(textLine))
						throw new DigitalFlareApplicationException(string.Format("Несоответствие данных источника формату CSV. Данные, считанные из '{0}' строки источника представляют либо пустую строку, либо строку, содержащую только пробельные символы.", textLineNumber.TextViewInvariant("d")));
					if(textLineNumber == 1) {
						// Table columns line.
						//
						textLineValues.Clear();
						CsvUtilities.SplitLine(textLine, textLineNumber, textLineValues);
						if(textLineValues.Count < 1)
							throw new DigitalFlareApplicationException("Несоответствие данных источника формату CSV. Невозможно определить структуру CSV-таблицы.");
						columnNamesOrder = new Dictionary<string, int>(ColumnNameEqualityComparer);
						for(var i = 0; i < textLineValues.Count; i++) {
							columnName = textLineValues[i];
							if(string.IsNullOrEmpty(columnName))
								throw new DigitalFlareApplicationException(string.Format("Несоответствие данных источника формату CSV. Для колонки в '{0}' позиции не задано имя.", (i + 1).TextViewInvariant("d")));
							if(columnName.Length > MaxColumnNameLength)
								throw new DigitalFlareApplicationException(string.Format("Для колонки в '{0}' позиции задано слишком длинное имя. Макс. длина имени составляет '{1}' символов.", (i + 1).TextViewInvariant("d"), MaxColumnNameLength.TextViewInvariant("d")));
							if(columnNamesOrder.ContainsKey(columnName))
								throw new DigitalFlareApplicationException(string.Format("Несоответствие данных источника формату CSV. Для колонки в '{0}' позиции задано неуникальное имя '{1}'.", (i + 1).TextViewInvariant("d"), columnName.TextView()));
							columnNamesOrder.Add(columnName, i);
						}
						table = new P_Table(P_GenerateRowType(columnNamesOrder, ColumnNameComparer), ColumnNameEqualityComparer);
						table.Internal_SetColumns(columnNamesOrder.OrderBy(x => x.Value).Select(i => new P_Column(table, i.Key)));
					} else {
						// Table row line.
						//
						textLineValues.Clear();
						CsvUtilities.SplitLine(textLine, textLineNumber, textLineValues);
						if(textLineValues.Count != columnNamesOrder.Count)
							throw new DigitalFlareApplicationException(string.Format("Несоответствие данных источника формату CSV. Строка данных (номер строки '{0}') не соответствует структуре таблицы по количеству колонок в таблице.", textLineNumber.TextViewInvariant("d")));
						if(rowCtor == null)
							rowCtor = ActivationUtilities.RequireConstructor<ICsvDataTable, IEnumerable<string>, Row>(concreteType: table.RowType);
						rows.AddLast(rowCtor(table, textLineValues));
					}
				}
				if(table == null)
					throw new DigitalFlareApplicationException("Источник данных не содержит какого-либо содержимого, на основе которого можно было бы создать CSV-таблицу.");
				table.Internal_SetRows(rows);
				return table;
			} catch(Exception firstException) {
				if(firstException.IsCritical())
					throw;
				DisposableUtilities.Dispose(firstException, table);
				throw new DigitalFlareApplicationException("Во время создания CSV-таблицы из указанного источника данных возникла ошибка.", firstException);
			}
		}

		public IEqualityComparer<string> ColumnNameEqualityComparer { get { return StringComparer.OrdinalIgnoreCase; } }

		public IComparer<string> ColumnNameComparer { get { return StringComparer.OrdinalIgnoreCase; } }

	}

}

#endif