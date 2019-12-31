using System;
using System.Collections.Generic;

namespace Eon {

	public static partial class FormatStringUtilities {

		public static string GCollection<T>(this FormatStringUtilitiesHandle<IList<T>> hnd, int maxCountOfOutputItems, IFormatProvider formatProvider)
			=>
			GCollection(hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object), maxCountOfOutputItems: maxCountOfOutputItems, formatProvider: formatProvider);

		public static string GCollection<T>(this FormatStringUtilitiesHandle<IList<T>> hnd, int maxCountOfOutputItems, IFormatProvider provider, Func<int, T, IFormatProvider, string> itemFormatter)
			=>
			GCollection(hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object), maxCountOfOutputItems: maxCountOfOutputItems, formatProvider: provider, itemFormatter: itemFormatter);

		public static string GCollection<T>(this FormatStringUtilitiesHandle<IList<T>> hnd, IFormatProvider formatProvider)
			=>
			GCollection(hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object), maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems, formatProvider: formatProvider, itemFormatter: null);

		public static string GCollection<T>(this FormatStringUtilitiesHandle<IList<T>> hnd, IFormatProvider formatProvider, Func<int, T, IFormatProvider, string> itemFormatter)
			=>
			GCollection(hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object), maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems, formatProvider: formatProvider, itemFormatter: itemFormatter);

		public static string GCollectionNlI<T>(this FormatStringUtilitiesHandle<IList<T>> hnd, int maxCountOfOutputItems, IFormatProvider formatProvider)
			=>
			Environment.NewLine
			+
			GCollection(hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object), maxCountOfOutputItems: maxCountOfOutputItems, formatProvider: formatProvider, itemFormatter: null)
			.IndentLines();

		public static string GCollectionNlI<T>(this FormatStringUtilitiesHandle<IList<T>> hnd)
			=>
			Environment.NewLine
			+
			GCollection(hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object), maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems, formatProvider: null, itemFormatter: null)
			.IndentLines();

		public static string GCollectionNlI<T>(this FormatStringUtilitiesHandle<IList<T>> hnd, int maxCountOfOutputItems, IFormatProvider formatProvider, Func<int, T, IFormatProvider, string> itemFormatter)
			=>
			Environment.NewLine
			+
			GCollection(hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object), maxCountOfOutputItems: maxCountOfOutputItems, formatProvider: formatProvider, itemFormatter: itemFormatter)
			.IndentLines();

		public static string GCollectionNlI2<T>(this FormatStringUtilitiesHandle<IList<T>> hnd, int maxCountOfOutputItems, IFormatProvider formatProvider)
			=>
			Environment.NewLine
			+
			GCollection(hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object), maxCountOfOutputItems: maxCountOfOutputItems, formatProvider: formatProvider, itemFormatter: null)
			.IndentLines2();

		public static string GCollectionNlI2<T>(this FormatStringUtilitiesHandle<IList<T>> hnd, int maxCountOfOutputItems, IFormatProvider formatProvider, Func<int, T, IFormatProvider, string> itemFormatter)
			=>
			Environment.NewLine
			+
			GCollection(hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object), maxCountOfOutputItems: maxCountOfOutputItems, formatProvider: formatProvider, itemFormatter: itemFormatter)
			.IndentLines2();

		public static string GCollectionNlI<T>(this FormatStringUtilitiesHandle<IList<T>> hnd, IFormatProvider formatProvider)
			=>
			Environment.NewLine
			+
			GCollection(hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object), maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems, formatProvider: formatProvider, itemFormatter: null)
			.IndentLines();

		public static string GCollectionNlI<T>(this FormatStringUtilitiesHandle<IList<T>> hnd, IFormatProvider formatProvider, Func<int, T, IFormatProvider, string> itemFormatter)
			=>
			Environment.NewLine
			+
			GCollection(hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object), maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems, formatProvider: formatProvider, itemFormatter: itemFormatter)
			.IndentLines();

		public static string GCollectionNlI2<T>(this FormatStringUtilitiesHandle<IList<T>> hnd, IFormatProvider formatProvider)
			=>
			Environment.NewLine
			+
			GCollection(hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object), maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems, formatProvider: formatProvider, itemFormatter: null)
			.IndentLines2();

		public static string GCollectionNlI2<T>(this FormatStringUtilitiesHandle<IList<T>> hnd, IFormatProvider formatProvider, Func<int, T, IFormatProvider, string> itemFormatter)
			=>
			Environment.NewLine
			+
			GCollection(hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object), maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems, formatProvider: formatProvider, itemFormatter: itemFormatter)
			.IndentLines2();

	}

}