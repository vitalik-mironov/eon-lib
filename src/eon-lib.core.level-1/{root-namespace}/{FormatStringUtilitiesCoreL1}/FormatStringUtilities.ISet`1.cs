using System;
using System.Collections.Generic;

namespace Eon {

	public static partial class FormatStringUtilities {

		public static string GCollection<T>(this FormatStringUtilitiesHandle<ISet<T>> hnd, int maxCountOfOutputItems, IFormatProvider formatProvider)
			=>
			GCollection(
				hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object),
				maxCountOfOutputItems: maxCountOfOutputItems,
				formatProvider: formatProvider);

		public static string GCollection<T>(this FormatStringUtilitiesHandle<ISet<T>> hnd, IFormatProvider formatProvider)
			=>
			GCollection(
				hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object),
				maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems,
				formatProvider: formatProvider);

		public static string GCollectionNlI<T>(this FormatStringUtilitiesHandle<ISet<T>> hnd, int maxCountOfOutputItems, IFormatProvider formatProvider)
			=>
			Environment.NewLine
			+
			GCollection(
				hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object),
				maxCountOfOutputItems: maxCountOfOutputItems,
				formatProvider: formatProvider)
			.IndentLines();

		public static string GCollectionNlI2<T>(this FormatStringUtilitiesHandle<ISet<T>> hnd, int maxCountOfOutputItems, IFormatProvider formatProvider)
			=>
			Environment.NewLine
			+
			GCollection(
				hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object),
				maxCountOfOutputItems: maxCountOfOutputItems,
				formatProvider: formatProvider)
			.IndentLines2();

		public static string GCollectionNlI<T>(this FormatStringUtilitiesHandle<ISet<T>> hnd, IFormatProvider formatProvider)
			=>
			Environment.NewLine
			+
			GCollection(
				hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object),
				maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems,
				formatProvider: formatProvider)
			.IndentLines();

		public static string GCollectionNlI2<T>(this FormatStringUtilitiesHandle<ISet<T>> hnd, IFormatProvider formatProvider)
			=>
			Environment.NewLine
			+
			GCollection(
				hnd: new FormatStringUtilitiesHandle<ICollection<T>>(hnd.Object),
				maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems,
				formatProvider: formatProvider)
			.IndentLines2();

	}

}