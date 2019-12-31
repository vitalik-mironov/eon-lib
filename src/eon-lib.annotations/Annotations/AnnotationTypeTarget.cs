using System;

namespace Eon.Annotations {

	public class AnnotationTypeTarget {

		readonly Type _type;

		readonly int _typeInheritanceLevel;

		public AnnotationTypeTarget(Type type, int typeInheritanceLevel) {
			type.EnsureNotNull(nameof(type));
			typeInheritanceLevel.EnsureNotLessThanZero(nameof(typeInheritanceLevel));
			//
			_type = type;
			_typeInheritanceLevel = typeInheritanceLevel;
		}

		public Type Type
			=> _type;

		public int TypeInheritanceLevel
			=> _typeInheritanceLevel;

		public static implicit operator Type(AnnotationTypeTarget value)
			=> value?._type;

		public override string ToString()
			=> _type.ToString();

	}

}