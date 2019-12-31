using System;
using Eon.Metadata;

namespace Eon {

	public static partial class FormatStringUtilitiesMetadata {

		public static string Short(this FormatStringUtilitiesHandle<MetadataPathName> hnd, IFormatProvider formatProvider = default) {
			if (hnd.Object is null)
				return FormatStringUtilities.GetNullValueText(formatProvider: formatProvider);
			else
				return hnd.Object.ToString(formatSpecifier: FormatStringUtilities.ShortFormatSpecifier, formatProvider: formatProvider);
		}

		public static string ShortNewLineIndented(this FormatStringUtilitiesHandle<MetadataPathName> hnd, IFormatProvider formatProvider = null)
			=> $"{Environment.NewLine}{Short(hnd: hnd, formatProvider: formatProvider).IndentLines()}";

		public static string ShortNewLineIndented2(this FormatStringUtilitiesHandle<MetadataPathName> hnd, IFormatProvider formatProvider = null)
			=> $"{Environment.NewLine}{Short(hnd: hnd, formatProvider: formatProvider).IndentLines2()}";

	}

}