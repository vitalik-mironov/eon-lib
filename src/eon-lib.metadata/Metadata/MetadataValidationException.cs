using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Metadata {

	/// <summary>
	/// Исключение, возникающее когда валидация метаданных, выяет одну или более ошибок.
	/// <para>Код ошибки для данного исключения соответствует <seealso cref="MetadataErrorCodes.Validation.NotValid"/>.</para>
	/// </summary>
	public class MetadataValidationException
		:MetadataException {

		readonly IMetadata _metadata;

		public MetadataValidationException()
			: this(null, null, null) { }

		public MetadataValidationException(IMetadata metadata)
			: this(null, metadata, null) { }

		public MetadataValidationException(
			string message,
			IMetadata metadata)
			: this(
					message,
					metadata,
					null) { }

		public MetadataValidationException(
			IMetadata metadata,
			Exception innerException)
			: this(
					null,
					metadata,
					innerException) { }

		public MetadataValidationException(
			string message,
			IMetadata metadata,
			Exception innerException)
			: base(
					message: string.IsNullOrEmpty(message) ? FormatXResource(typeof(MetadataValidationException), "DefaultMessage", metadata.FmtStr().G()) : FormatXResource(typeof(MetadataValidationException), "UserMessage", message, metadata.FmtStr().G()),
					innerException: innerException) {
			//
			_metadata = metadata;
			this.SetErrorCode(code: MetadataErrorCodes.Validation.NotValid);
		}

		public IMetadata Metadata
			=> _metadata;

	}

}