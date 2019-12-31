using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Interaction {

	public class InteractionException
		:EonException {

		public InteractionException(string message)
			: this(message, null) { }

		public InteractionException(Exception innerException)
			: this(null, innerException) { }

		public InteractionException()
			: this(null, null) { }

		public InteractionException(string message, Exception innerException)
			: base(message: string.IsNullOrEmpty(message) ? FormatXResource(locator: typeof(InteractionException), subpath: "DefaultMessage") : message, innerException: innerException) { }

	}

}