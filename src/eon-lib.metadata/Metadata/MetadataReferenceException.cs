using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Metadata {

	public class MetadataReferenceException
		:MetadataException {

		#region Static members

		static string P_ConstructMessage(IMetadataReference reference, IMetadata @base, string message, Exception innerException) {
			if (!(string.IsNullOrEmpty(message) && reference == null && @base == null)) {
				if (innerException == null)
					return FormatXResource(typeof(MetadataReferenceException), "Detailed", message, reference.FmtStr().G(), @base.FmtStr().G());
				return FormatXResource(typeof(MetadataReferenceException), "Detailed/InnerException", message, reference.FmtStr().G(), @base.FmtStr().G());
			} else if (innerException == null)
				return FormatXResource(typeof(MetadataReferenceException), subpath: null);
			return FormatXResource(typeof(MetadataReferenceException), "InnerException");
		}

		#endregion

		readonly string _referenceAsString;

		readonly string _baseAsString;

		readonly IMetadataReference _reference;

		readonly IMetadata _base;

		public MetadataReferenceException(IMetadataReference reference, IMetadata @base, string message)
			: this(reference, @base, message, null) { }

		public MetadataReferenceException(IMetadataReference reference, IMetadata @base, string message, Exception innerException)
			: base(P_ConstructMessage(reference, @base, message, innerException), innerException) {
			_reference = reference;
			_referenceAsString = reference?.ToString();
			_base = @base;
			_baseAsString = @base?.ToString();
		}

		public MetadataReferenceException()
			: this(null, null, null, null) { }

		public MetadataReferenceException(string message)
			: this(null, null, message, null) { }

		public MetadataReferenceException(string message, Exception innerException)
			: this(null, null, message, innerException) { }

		public string ReferenceAsString
			=> _referenceAsString;

		public IMetadataReference Reference
			=> _reference;

		public string BaseAsString
			=> _baseAsString;

		public IMetadata Base
			=> _base;

	}

}