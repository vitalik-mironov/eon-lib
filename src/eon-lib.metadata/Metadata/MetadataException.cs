using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Metadata {

	public class MetadataException
		:EonException {

		public MetadataException()
			: this(string.Empty) { }

		public MetadataException(string message)
			: this(message, null) { }

		public MetadataException(string message, Exception innerException)
			: base(message: FormatXResource(typeof(MetadataException), null) + ((string.IsNullOrEmpty(message)) ? string.Empty : Environment.NewLine + message), innerException: innerException) {
			this.SetErrorCode(code: MetadataErrorCodes.Fault);
		}

	}

}