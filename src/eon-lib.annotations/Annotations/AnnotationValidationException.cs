using System;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Annotations {

	public class AnnotationValidationException
		:EonException {

		public AnnotationValidationException()
			: this(message: null, annotation: null, annotationTarget: null, innerException: null) { }

		public AnnotationValidationException(string message, Type annotationTarget)
			: this(message, null, annotationTarget, (Exception)null) { }

		public AnnotationValidationException(AnnotationAttributeBase annotation, Type annotationTarget)
			: this((string)null, annotation, annotationTarget, (Exception)null) { }

		public AnnotationValidationException(AnnotationAttributeBase annotation, Type annotationTarget, Exception innerException)
			: this((string)null, annotation, annotationTarget, innerException) { }

		public AnnotationValidationException(string message, AnnotationAttributeBase annotation, Type annotationTarget)
			: this(message, annotation, annotationTarget, (Exception)null) { }

		public AnnotationValidationException(string message, AnnotationAttributeBase annotation, Type annotationTarget, Exception innerException)
			: base(string.IsNullOrEmpty(message) ? FormatXResource(typeof(AnnotationValidationException), "DefaultMessage", annotation, annotationTarget) : FormatXResource(typeof(AnnotationValidationException), "UserMessage", annotation, annotationTarget, message), innerException) { }

	}

}