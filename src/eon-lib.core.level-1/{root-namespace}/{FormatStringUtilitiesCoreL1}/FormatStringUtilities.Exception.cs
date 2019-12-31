using System;
using Eon.Diagnostics;

namespace Eon {

	public static partial class FormatStringUtilities {

		public static string Info(this FormatStringUtilitiesHandle<Exception> hnd, ExceptionInfoFormattingOptions options, IFormatProvider formatProvider) {
			if (hnd.Object is null)
				return GetNullValueText(formatProvider: formatProvider);
			else
				return hnd.Object.ToExceptionInfo().ToString(options: options, singleInfoFormatter: null);
		}

		public static string Info(this FormatStringUtilitiesHandle<Exception> hnd, ExceptionInfoFormattingOptions options)
			=> Info(hnd: hnd, options: options, formatProvider: null);

		public static string InfoNLI(this FormatStringUtilitiesHandle<Exception> hnd, ExceptionInfoFormattingOptions options, IFormatProvider formatProvider)
			=> Environment.NewLine + Info(hnd: hnd, options: options, formatProvider: formatProvider).IndentLines();

		public static string InfoNLI(this FormatStringUtilitiesHandle<Exception> hnd, ExceptionInfoFormattingOptions options)
			=> Environment.NewLine + Info(hnd: hnd, options: options, formatProvider: null).IndentLines();

		public static string InfoNLI2(this FormatStringUtilitiesHandle<Exception> hnd, ExceptionInfoFormattingOptions options, IFormatProvider formatProvider)
			=> Environment.NewLine + Info(hnd: hnd, options: options, formatProvider: formatProvider).IndentLines2();

		public static string InfoNLI2(this FormatStringUtilitiesHandle<Exception> hnd, ExceptionInfoFormattingOptions options)
			=> Environment.NewLine + Info(hnd: hnd, options: options, formatProvider: null).IndentLines2();

		public static string InfoShort(this FormatStringUtilitiesHandle<Exception> hnd, IFormatProvider formatProvider)
			=> Info(hnd: hnd, options: ExceptionInfoFormattingOptions.IncludeType | ExceptionInfoFormattingOptions.IncludeNumberingMarker | ExceptionInfoFormattingOptions.IncludeMessage | ExceptionInfoFormattingOptions.IncludeErrorCodeIdentifier,
				formatProvider: formatProvider);

		public static string InfoShort(this FormatStringUtilitiesHandle<Exception> hnd)
			=> InfoShort(hnd: hnd, formatProvider: null);

		public static string InfoShortNLI(this FormatStringUtilitiesHandle<Exception> hnd, IFormatProvider formatProvider)
			=> Environment.NewLine + InfoShort(hnd: hnd, formatProvider: formatProvider).IndentLines();

		public static string InfoShortNLI(this FormatStringUtilitiesHandle<Exception> hnd)
			=> Environment.NewLine + InfoShort(hnd: hnd, formatProvider: null).IndentLines();

		public static string InfoShortNLI2(this FormatStringUtilitiesHandle<Exception> hnd, IFormatProvider formatProvider)
			=> Environment.NewLine + InfoShort(hnd: hnd, formatProvider: formatProvider).IndentLines2();

		public static string InfoShortNLI2(this FormatStringUtilitiesHandle<Exception> hnd)
			=> Environment.NewLine + InfoShort(hnd: hnd, formatProvider: null).IndentLines2();

		public static string InfoFull(this FormatStringUtilitiesHandle<Exception> hnd, IFormatProvider formatProvider)
			=> Info(hnd: hnd, options: ExceptionInfoFormattingOptions.Full, formatProvider: formatProvider);

		public static string InfoFull(this FormatStringUtilitiesHandle<Exception> hnd)
			=> InfoFull(hnd: hnd, formatProvider: null);

		public static string InfoFullNLI(this FormatStringUtilitiesHandle<Exception> hnd, IFormatProvider formatProvider)
			=> Environment.NewLine + InfoFull(hnd: hnd, formatProvider: formatProvider).IndentLines();

		public static string InfoFullNLI(this FormatStringUtilitiesHandle<Exception> hnd)
			=> Environment.NewLine + InfoFull(hnd: hnd, formatProvider: null).IndentLines();

		public static string InfoFullNLI2(this FormatStringUtilitiesHandle<Exception> hnd, IFormatProvider formatProvider)
			=> Environment.NewLine + InfoFull(hnd: hnd, formatProvider: formatProvider).IndentLines2();

		public static string InfoFullNLI2(this FormatStringUtilitiesHandle<Exception> hnd)
			=> Environment.NewLine + InfoFull(hnd: hnd, formatProvider: null).IndentLines2();


	}

}