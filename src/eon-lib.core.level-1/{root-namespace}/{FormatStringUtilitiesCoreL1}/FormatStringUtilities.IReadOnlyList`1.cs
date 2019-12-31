using System;
using System.Collections.Generic;
using Eon.Collections;

namespace Eon {

	public static partial class FormatStringUtilities {

		public static string GCollection<T>(this FormatStringUtilitiesHandle<IReadOnlyList<T>> hnd, int maxCountOfOutputItems, IFormatProvider provider)
			=>
			GCollection(
				hnd: new FormatStringUtilitiesHandle<IList<T>>(obj: new ListReadOnlyWrap<T>(readOnlyList: hnd.Object)),
				maxCountOfOutputItems: maxCountOfOutputItems,
				provider: provider,
				itemFormatter: null);

		public static string GCollection<T>(this FormatStringUtilitiesHandle<IReadOnlyList<T>> hnd, int maxCountOfOutputItems, IFormatProvider provider, Func<int, T, IFormatProvider, string> itemFormatter)
			=>
			GCollection(
				hnd: new FormatStringUtilitiesHandle<IList<T>>(obj: new ListReadOnlyWrap<T>(readOnlyList: hnd.Object)),
				maxCountOfOutputItems: maxCountOfOutputItems,
				provider: provider,
				itemFormatter: itemFormatter);

		public static string GCollection<T>(this FormatStringUtilitiesHandle<IReadOnlyList<T>> hnd, IFormatProvider provider)
			=>
			GCollection(
				hnd: new FormatStringUtilitiesHandle<IList<T>>(obj: new ListReadOnlyWrap<T>(readOnlyList: hnd.Object)),
				maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems,
				provider: provider,
				itemFormatter: null);

		public static string GCollection<T>(this FormatStringUtilitiesHandle<IReadOnlyList<T>> hnd, IFormatProvider provider, Func<int, T, IFormatProvider, string> itemFormatter)
			=>
			GCollection(
				hnd: new FormatStringUtilitiesHandle<IList<T>>(obj: new ListReadOnlyWrap<T>(readOnlyList: hnd.Object)),
				maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems,
				provider: provider,
				itemFormatter: itemFormatter);

		public static string GCollectionNlI<T>(this FormatStringUtilitiesHandle<IReadOnlyList<T>> hnd, int maxCountOfOutputItems, IFormatProvider provider)
			=>
			Environment.NewLine
			+
			GCollection(
				hnd: new FormatStringUtilitiesHandle<IList<T>>(obj: new ListReadOnlyWrap<T>(readOnlyList: hnd.Object)),
				maxCountOfOutputItems: maxCountOfOutputItems,
				provider: provider,
				itemFormatter: null)
			.IndentLines();

		public static string GCollectionNlI<T>(this FormatStringUtilitiesHandle<IReadOnlyList<T>> hnd, int maxCountOfOutputItems, IFormatProvider provider, Func<int, T, IFormatProvider, string> itemFormatter)
			=>
			Environment.NewLine
			+
			GCollection(
				hnd: new FormatStringUtilitiesHandle<IList<T>>(obj: new ListReadOnlyWrap<T>(readOnlyList: hnd.Object)),
				maxCountOfOutputItems: maxCountOfOutputItems,
				provider: provider,
				itemFormatter: itemFormatter)
			.IndentLines();

		public static string GCollectionNlI2<T>(this FormatStringUtilitiesHandle<IReadOnlyList<T>> hnd, int maxCountOfOutputItems, IFormatProvider provider)
			=>
			Environment.NewLine
			+
			GCollection(
				hnd: new FormatStringUtilitiesHandle<IList<T>>(obj: new ListReadOnlyWrap<T>(readOnlyList: hnd.Object)),
				maxCountOfOutputItems: maxCountOfOutputItems,
				provider: provider,
				itemFormatter: null)
			.IndentLines2();

		public static string GCollectionNlI2<T>(this FormatStringUtilitiesHandle<IReadOnlyList<T>> hnd, int maxCountOfOutputItems, IFormatProvider provider, Func<int, T, IFormatProvider, string> itemFormatter)
			=>
			Environment.NewLine
			+
			GCollection(
				hnd: new FormatStringUtilitiesHandle<IList<T>>(obj: new ListReadOnlyWrap<T>(readOnlyList: hnd.Object)),
				maxCountOfOutputItems: maxCountOfOutputItems,
				provider: provider,
				itemFormatter: itemFormatter)
			.IndentLines2();

		public static string GCollectionNlI<T>(this FormatStringUtilitiesHandle<IReadOnlyList<T>> hnd, IFormatProvider provider)
			=>
			Environment.NewLine
			+
			GCollection(
				hnd: new FormatStringUtilitiesHandle<IList<T>>(obj: new ListReadOnlyWrap<T>(readOnlyList: hnd.Object)),
				maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems,
				provider: provider,
				itemFormatter: null)
			.IndentLines();

		public static string GCollectionNlI<T>(this FormatStringUtilitiesHandle<IReadOnlyList<T>> hnd, IFormatProvider provider, Func<int, T, IFormatProvider, string> itemFormatter)
			=>
			Environment.NewLine
			+
			GCollection(
				hnd: new FormatStringUtilitiesHandle<IList<T>>(obj: new ListReadOnlyWrap<T>(readOnlyList: hnd.Object)),
				maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems,
				provider: provider,
				itemFormatter: itemFormatter)
			.IndentLines();

		public static string GCollectionNlI2<T>(this FormatStringUtilitiesHandle<IReadOnlyList<T>> hnd, IFormatProvider provider)
			=>
			Environment.NewLine
			+
			GCollection(
				hnd: new FormatStringUtilitiesHandle<IList<T>>(obj: new ListReadOnlyWrap<T>(readOnlyList: hnd.Object)),
				maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems,
				provider: provider,
				itemFormatter: null)
			.IndentLines2();

		public static string GCollectionNlI2<T>(this FormatStringUtilitiesHandle<IReadOnlyList<T>> hnd, IFormatProvider provider, Func<int, T, IFormatProvider, string> itemFormatter)
			=>
			Environment.NewLine
			+
			GCollection(
				hnd: new FormatStringUtilitiesHandle<IList<T>>(obj: new ListReadOnlyWrap<T>(readOnlyList: hnd.Object)),
				maxCountOfOutputItems: DefaultMaxCountOfCollectionOutputItems,
				provider: provider,
				itemFormatter: itemFormatter)
			.IndentLines2();

	}

}