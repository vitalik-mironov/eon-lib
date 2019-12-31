using System;
using System.Security.Cryptography.X509Certificates;

namespace Eon {

	public static partial class FormatStringUtilities {

		// TODO: Put strings into the resources.
		//
		public static string G(this FormatStringUtilitiesHandle<X509Certificate2> hnd, IFormatProvider formatProvider) {
			if (hnd.Object == null)
				return GetNullValueText(formatProvider: formatProvider);
			else
				return
					$"Issuer:{hnd.Object.IssuerName.Name.FmtStr().GNLI()}"
					+ $"{Environment.NewLine}Subject:{hnd.Object.SubjectName.Name.FmtStr().GNLI()}"
					+ $"{Environment.NewLine}Serial No.:{hnd.Object.SerialNumber.FmtStr().GNLI()}"
					+ $"{Environment.NewLine}Thumbprint:{hnd.Object.Thumbprint.FmtStr().GNLI()}";
		}

		public static string G(this FormatStringUtilitiesHandle<X509Certificate2> hnd)
			=> G(hnd: hnd, formatProvider: null);

		public static string GNLI(this FormatStringUtilitiesHandle<X509Certificate2> hnd, IFormatProvider formatProvider)
			=> Environment.NewLine + G(hnd: hnd, formatProvider: formatProvider).IndentLines();

		public static string GNLI(this FormatStringUtilitiesHandle<X509Certificate2> hnd)
			=> GNLI(hnd: hnd, formatProvider: null);

		public static string GNLI2(this FormatStringUtilitiesHandle<X509Certificate2> hnd, IFormatProvider formatProvider)
			=> Environment.NewLine + G(hnd: hnd, formatProvider: formatProvider).IndentLines2();

		public static string GNLI2(this FormatStringUtilitiesHandle<X509Certificate2> hnd)
			=> GNLI2(hnd: hnd, formatProvider: null);

	}

}