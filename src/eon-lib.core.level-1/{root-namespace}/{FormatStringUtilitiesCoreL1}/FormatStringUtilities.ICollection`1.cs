using System;
using System.Collections.Generic;
using Eon.Text;

namespace Eon {

	public static partial class FormatStringUtilities {

		/// <summary>
		/// Возвращает используемое по умолчанию максимальное количество элементов коллекции, выводимых при форматировании коллекции в текст (см. <seealso cref="GCollection{T}(FormatStringUtilitiesHandle{ICollection{T}}, IFormatProvider)"/> и др.).
		/// <para>Значение: '48'.</para>
		/// </summary>
		public static readonly int DefaultMaxCountOfCollectionOutputItems = 48;

		// TODO: Put strings into the resources.
		//
		public static string GCollection<T>(this FormatStringUtilitiesHandle<ICollection<T>> hnd, int maxCountOfOutputItems, IFormatProvider formatProvider, Func<int, T, IFormatProvider, string> itemFormatter) {
			maxCountOfOutputItems
				.Arg(nameof(maxCountOfOutputItems))
				.EnsureNotLessThanZero();
			//
			if (hnd.Object == null)
				return FormatStringUtilities.GetNullValueText(formatProvider: formatProvider);
			else {
				if (itemFormatter == null)
					itemFormatter = (locIndex, locItem, locFormatProvider) => locItem.FmtStr().G(formatProvider: formatProvider);
				using (var acquiredBuffer = StringBuilderUtilities.AcquireBuffer()) {
					var outputBuilder = acquiredBuffer.StringBuilder;
					//
					var collectionSize = hnd.Object.Count;
					outputBuilder.Append($"Коллекция (размер {collectionSize.ToString("d", formatProvider)}):");
					if (maxCountOfOutputItems > 0) {
						var itemCounter = 0;
						foreach (var item in hnd.Object) {
							checked { itemCounter++; }
							outputBuilder.Append(Environment.NewLine);
							outputBuilder.Append('\x0009');
							outputBuilder.Append(itemCounter.ToString("d", formatProvider));
							outputBuilder.Append(")");
							outputBuilder
								.Append(
									value:
									itemFormatter(arg1: itemCounter - 1, arg2: item, arg3: formatProvider)
									.FmtStr()
									.G(formatProvider: formatProvider)
									.FmtStr()
									.GNLI2());
							if (itemCounter == maxCountOfOutputItems)
								break;
						}
					}
					if (collectionSize > maxCountOfOutputItems) {
						outputBuilder.Append(Environment.NewLine);
						outputBuilder.Append('\x0009');
						outputBuilder.Append('…');
					}
					//
					return outputBuilder.ToString();
				}
			}
		}

		public static string GCollection<T>(
			this FormatStringUtilitiesHandle<ICollection<T>> hnd,
			int maxCountOfOutputItems,
			IFormatProvider formatProvider)
			=> GCollection(hnd: hnd, maxCountOfOutputItems: maxCountOfOutputItems, formatProvider: formatProvider, itemFormatter: null);

		public static string GCollection<T>(this FormatStringUtilitiesHandle<ICollection<T>> hnd, IFormatProvider formatProvider)
			=>
			GCollection(hnd: hnd, maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems, formatProvider: formatProvider, itemFormatter: null);

		public static string GCollection<T>(this FormatStringUtilitiesHandle<ICollection<T>> hnd, IFormatProvider formatProvider, Func<int, T, IFormatProvider, string> itemFormatter)
			=>
			GCollection(hnd: hnd, maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems, formatProvider: formatProvider, itemFormatter: itemFormatter);

		public static string GCollectionNlI<T>(this FormatStringUtilitiesHandle<ICollection<T>> hnd, int maxCountOfOutputItems, IFormatProvider formatProvider)
			=>
			Environment.NewLine
			+
			GCollection(hnd: hnd, maxCountOfOutputItems: maxCountOfOutputItems, formatProvider: formatProvider, itemFormatter: null)
			.IndentLines();

		public static string GCollectionNlI<T>(this FormatStringUtilitiesHandle<ICollection<T>> hnd, int maxCountOfOutputItems, IFormatProvider formatProvider, Func<int, T, IFormatProvider, string> itemFormatter)
			=>
			Environment.NewLine
			+
			GCollection(hnd: hnd, maxCountOfOutputItems: maxCountOfOutputItems, formatProvider: formatProvider, itemFormatter: itemFormatter)
			.IndentLines();

		public static string GCollectionNlI2<T>(this FormatStringUtilitiesHandle<ICollection<T>> hnd, int maxCountOfOutputItems, IFormatProvider formatProvider)
			=>
			Environment.NewLine
			+
			GCollection(hnd: hnd, maxCountOfOutputItems: maxCountOfOutputItems, formatProvider: formatProvider, itemFormatter: null)
			.IndentLines2();

		public static string GCollectionNlI2<T>(this FormatStringUtilitiesHandle<ICollection<T>> hnd, int maxCountOfOutputItems, IFormatProvider formatProvider, Func<int, T, IFormatProvider, string> itemFormatter)
			=>
			Environment.NewLine
			+
			GCollection(hnd: hnd, maxCountOfOutputItems: maxCountOfOutputItems, formatProvider: formatProvider, itemFormatter: itemFormatter)
			.IndentLines2();

		public static string GCollectionNlI<T>(this FormatStringUtilitiesHandle<ICollection<T>> hnd, IFormatProvider formatProvider)
			=>
			Environment.NewLine
			+
			GCollection(hnd: hnd, maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems, formatProvider: formatProvider, itemFormatter: null)
			.IndentLines();

		public static string GCollectionNlI<T>(this FormatStringUtilitiesHandle<ICollection<T>> hnd, IFormatProvider formatProvider, Func<int, T, IFormatProvider, string> itemFormatter)
			=>
			Environment.NewLine
			+
			GCollection(hnd: hnd, maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems, formatProvider: formatProvider, itemFormatter: itemFormatter)
			.IndentLines();

		public static string GCollectionNlI2<T>(this FormatStringUtilitiesHandle<ICollection<T>> hnd, IFormatProvider formatProvider)
			=>
			Environment.NewLine
			+
			GCollection(hnd: hnd, maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems, formatProvider: formatProvider, itemFormatter: null)
			.IndentLines2();

		public static string GCollectionNlI2<T>(this FormatStringUtilitiesHandle<ICollection<T>> hnd, IFormatProvider formatProvider, Func<int, T, IFormatProvider, string> itemFormatter)
			=>
			Environment.NewLine
			+
			GCollection(hnd: hnd, maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems, formatProvider: formatProvider, itemFormatter: itemFormatter)
			.IndentLines2();

	}

}