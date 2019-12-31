using System;
using System.Globalization;

namespace Eon {

	public static partial class FormatStringUtilities {

		public static string H(this FormatStringUtilitiesHandle<byte> hnd)
			=> "0x" + hnd.Object.ToString("x2", provider: CultureInfo.InvariantCulture);

		public static string HNlI(this FormatStringUtilitiesHandle<byte> hnd)
			=> Environment.NewLine + H(hnd: hnd).IndentLines();

		public static string HNlI2(this FormatStringUtilitiesHandle<byte> hnd)
			=> Environment.NewLine + H(hnd: hnd).IndentLines2();

		public static string HNlI3(this FormatStringUtilitiesHandle<byte> hnd)
			=> Environment.NewLine + H(hnd: hnd).IndentLines3();

		public static string H(this FormatStringUtilitiesHandle<byte?> hnd)
			=> hnd.Object.HasValue ? "0x" + hnd.Object.Value.ToString("x2", provider: CultureInfo.InvariantCulture) : FormatStringUtilitiesCoreL0.GetNullValueText();

		public static string HNlI(this FormatStringUtilitiesHandle<byte?> hnd)
			=> Environment.NewLine + H(hnd: hnd).IndentLines();

		public static string HNlI2(this FormatStringUtilitiesHandle<byte?> hnd)
			=> Environment.NewLine + H(hnd: hnd).IndentLines2();

		public static string HNlI3(this FormatStringUtilitiesHandle<byte?> hnd)
			=> Environment.NewLine + H(hnd: hnd).IndentLines3();

	}

}