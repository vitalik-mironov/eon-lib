using System;

namespace Eon.Resources.XResource {

	public class XResourceException
		:InvalidOperationException {

		public XResourceException(string message, Exception innerException)
			: base(message: string.IsNullOrEmpty(message) ? "An exception occurred during XResource operation." : message, innerException: innerException) { }

		public XResourceException(string message)
			: this(message: message, innerException: null) { }

		public XResourceException()
			: this(message: null, innerException: null) { }

		public XResourceException(Exception innerException)
			: this(message: null, innerException: innerException) { }

	}

}