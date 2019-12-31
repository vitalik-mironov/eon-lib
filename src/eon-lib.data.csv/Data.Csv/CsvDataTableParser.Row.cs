#if TRG_NETSTANDARD1_5 || TRG_SL5
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DigitalFlare.BusinessApp.Data;
using DigitalFlare.Resources.XResource;
using rt = DigitalFlare.Resources.XResource.XResourceTreeManager;

namespace DigitalFlare.Data {

	public partial class CsvDataTableParser {

#region Nested types

		//[DebuggerNonUserCode]
		//[DebuggerStepThrough]
		public abstract class Row
			:BusinessType, ICsvDataTableRow {

#region Nested types

			//[DebuggerNonUserCode]
			//[DebuggerStepThrough]
			sealed class P_DisplayTextSource
				:XResourceTreeNoArgsTextNodeRef {

				string _displayText;

				internal P_DisplayTextSource(string displayText)
					: base(typeof(CsvDataTableParser)) {
					_displayText = displayText;
				}

				public override string Text { get { return ReadDA(ref _displayText); } }

				protected override string GetTextForCulture(CultureInfo culture) { return ReadDA(ref _displayText); }

				protected override void Dispose(bool explicitDispose) {
					_displayText = null;
					base.Dispose(explicitDispose);
				}

			}

#endregion

			ICsvDataTable _table;

			string[] _values;

			// TODO: Put exception messages into the resources.
			//
			protected Row(ICsvDataTable table, IEnumerable<string> values) {
				if (table == null)
					throw new ArgumentNullException("table");
				if (values == null)
					throw new ArgumentNullException("values");
				var valuesArray = values.ToArray();
				if (valuesArray.Length < 1)
					throw new ArgumentException(rt.Format(typeof(Array), "CanNotEmpty"), "values");
				if (valuesArray.Length != table.ColumnCount)
					throw new ArgumentException("Количество элементов в последовательности значений не соответствует количеству колонок в таблице.", "values");
				_table = table;
				_values = valuesArray;
			}

			public string this[string columnName] {
				get {
					var result = ReadDA(ref _values)[Table.GetColumnOrdinal(columnName)];
					EnsureNotDisposeState();
					return result;
				}
			}

			public ICsvDataTable Table { get { return ReadDA(ref _table); } }

			protected override void Dispose(bool explicitDispose) {
				if (explicitDispose) {
					var values = _values;
					if (values != null && values.Length > 0)
						Array.Clear(values, 0, values.Length);
				}
				_table = null;
				_values = null;
				base.Dispose(explicitDispose);
			}

			// TODO: Put exception messages into the resources.
			//
			protected static FieldProperty<string> CreateColumnMetadataField(int order, string columnName) {
				return
					new FieldProperty<Row, string>(order, FieldPropertyUIOptions.NotEditable) {
						UI = new FieldPropertyUIFacet() {
							CaptionTextSource = new P_DisplayTextSource(columnName)
						},
						CustomGetter = (instance) => instance[columnName],
						CustomSetter = (instance, value, isInitValue) => {
							if (isInitValue)
								return;
							throw new DigitalFlareApplicationException(string.Format("Невозможно изменить значение поля '{0}' объекта '{1}'. Поле объекта недоступно для записи.", columnName.TextView(), instance.TextView()));
						}
					}.EndDefinition();
			}

			protected string GetValue(FieldProperty<string> fieldMetadata) { return GetValue<string>(fieldMetadata); }

			bool ICsvDataTableRow.TryGetValue(string columnName, out string value) {
				if (Table.HasColumn(columnName)) {
					value = this[columnName];
					return true;
				}
				value = null;
				return false;
			}

		}

#endregion

	}

}
#endif