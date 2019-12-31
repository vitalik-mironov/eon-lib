using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Metadata {

	public sealed class InvalidMetadataNameException
		:EonException {

		public InvalidMetadataNameException()
			: this(string.Empty) { }

		public InvalidMetadataNameException(string message)
			: this(message, (Exception)null) { }

		public InvalidMetadataNameException(string message, Exception innerException)
			: base(message: string.IsNullOrEmpty(message) ? FormatXResource(locator: typeof(InvalidMetadataNameException), subpath: null) : message, innerException: innerException) { }

	}

}