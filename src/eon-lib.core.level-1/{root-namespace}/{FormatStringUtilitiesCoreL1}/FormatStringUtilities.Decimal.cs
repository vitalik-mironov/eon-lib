using System;
using System.Globalization;

namespace Eon {

	public static partial class FormatStringUtilities {

		public static string GInvariant(this FormatStringUtilitiesHandle<decimal?> hnd)
			=> hnd.Object.HasValue ? hnd.Object.Value.ToString("g", CultureInfo.InvariantCulture) : null;

		public static string GInvariant(this FormatStringUtilitiesHandle<decimal> hnd)
			=> hnd.Object.ToString("g");

		public static string Currency2(this FormatStringUtilitiesHandle<decimal?> handle)
			=> handle.Object.HasValue ? handle.Object.Value.ToString("c2") : "-";

		public static string Currency2(this FormatStringUtilitiesHandle<decimal?> handle, IFormatProvider formatProvider)
			=> handle.Object.HasValue ? handle.Object.Value.ToString("c2", formatProvider) : "-";

		public static string Currency2(this FormatStringUtilitiesHandle<decimal> handle)
			=> handle.Object.ToString("c2");

		public static string Currency2(this FormatStringUtilitiesHandle<decimal> handle, IFormatProvider formatProvider)
			=> handle.Object.ToString("c2", formatProvider);

		public static string Currency2Nbsp(this FormatStringUtilitiesHandle<decimal> handle)
			=> handle.Object.ToString("c2").Replace('\x0020', '\x00a0');

		public static string Currency2Nbsp(this FormatStringUtilitiesHandle<decimal> handle, IFormatProvider formatProvider)
			=> handle.Object.ToString("c2", formatProvider).Replace('\x0020', '\x00a0');

		public static string Currency2Nbsp(this FormatStringUtilitiesHandle<decimal?> handle)
			=> handle.Object.HasValue ? handle.Object.Value.ToString("c2").Replace('\x0020', '\x00a0') : "-";

		public static string Currency2Nbsp(this FormatStringUtilitiesHandle<decimal?> handle, IFormatProvider formatProvider)
			=> handle.Object.HasValue ? handle.Object.Value.ToString("c2", formatProvider).Replace('\x0020', '\x00a0') : "-";

		public static string Currency4(this FormatStringUtilitiesHandle<decimal?> handle)
			=> handle.Object.HasValue ? handle.Object.Value.ToString("c4") : "-";

		public static string Currency4(this FormatStringUtilitiesHandle<decimal?> handle, IFormatProvider formatProvider)
			=> handle.Object.HasValue ? handle.Object.Value.ToString("c4", formatProvider) : "-";

		public static string Currency4(this FormatStringUtilitiesHandle<decimal> handle)
			=> handle.Object.ToString("c4");

		public static string Currency4(this FormatStringUtilitiesHandle<decimal> handle, IFormatProvider formatProvider)
			=> handle.Object.ToString("c4", formatProvider);

		public static string Currency4Nbsp(this FormatStringUtilitiesHandle<decimal?> handle)
			=> handle.Object.HasValue ? handle.Object.Value.ToString("c4").Replace('\x0020', '\x00a0') : "-";

		public static string Currency4Nbsp(this FormatStringUtilitiesHandle<decimal?> handle, IFormatProvider formatProvider)
			=> handle.Object.HasValue ? handle.Object.Value.ToString("c4", formatProvider).Replace('\x0020', '\x00a0') : "-";

		public static string Currency4Nbsp(this FormatStringUtilitiesHandle<decimal> handle)
			=> handle.Object.ToString("c4").Replace('\x0020', '\x00a0');

		public static string Currency4Nbsp(this FormatStringUtilitiesHandle<decimal> handle, IFormatProvider formatProvider)
			=> handle.Object.ToString("c4", formatProvider).Replace('\x0020', '\x00a0');


	}

}