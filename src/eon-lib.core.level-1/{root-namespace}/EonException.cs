using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	public class EonException
		:InvalidOperationException {

		#region Static members

		public static readonly ExceptionFactoryDelegate2 Factory =
			(locMessage, locInnerException, locCode)
			=>
			new EonException(message: locMessage, innerException: locInnerException)
			.SetErrorCode(code: locCode);

		#endregion

		public EonException()
			: this(string.Empty) { }

		public EonException(string message)
			: this(message, null) { }

		public EonException(string message, Exception innerException)
			: base(
					message:
						innerException == null
						? (string.IsNullOrEmpty(message) ? FormatXResource(locator: typeof(EonException), subpath: null) : message)
						: FormatXResource(locator: typeof(EonException), subpath: "InnerException", args: new[ ] { string.IsNullOrEmpty(message) ? FormatXResource(locator: typeof(EonException), subpath: null) : message }),
					innerException: innerException) { }

	}

}