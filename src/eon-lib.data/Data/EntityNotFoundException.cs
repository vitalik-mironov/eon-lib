using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Data {

	public class EntityNotFoundException
		:EonException {

		public EntityNotFoundException()
			: this(message: string.Empty, innerException: null) { }

		public EntityNotFoundException(string message)
			: this(message: message, innerException: null) { }

		public EntityNotFoundException(string message, Exception innerException)
			: base(message: string.IsNullOrEmpty(message) ? FormatXResource(typeof(EntityNotFoundException), "DefaultMessage") : message, innerException: innerException) {
			//
			this.SetErrorCode(code: DataErrorCodes.Entity.NotFound);
		}

	}

}