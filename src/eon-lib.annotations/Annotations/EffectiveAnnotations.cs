using System.Collections.Generic;
using System.Linq;

namespace Eon.Annotations {

	public sealed class EffectiveAnnotations {

		readonly ICollection<AnnotationAttributeBase> _annotations;

		public EffectiveAnnotations(IEnumerable<AnnotationAttributeBase> annotations) {
			_annotations = annotations.Arg(nameof(annotations)).EnsureNotNull().EnsureNoNullElements().Value.ToArray().AsReadOnlyCollection();
		}

		public ICollection<AnnotationAttributeBase> Items
			=> _annotations;

	}

}