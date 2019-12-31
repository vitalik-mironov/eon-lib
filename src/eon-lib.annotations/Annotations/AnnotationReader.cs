using System;
using System.Collections.Generic;
using System.Linq;

using Eon.Reflection;

using static Eon.Resources.XResource.XResourceUtilities;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Annotations {

	internal sealed class AnnotationReader {

		#region Static members

		static AnnotationAttributeBase[ ] P_GetEffectiveAnnotations(Type annotatedType) {
			annotatedType.EnsureNotNull(nameof(annotatedType));
			//
			var annotationAttributeBaseType = typeof(AnnotationAttributeBase);
			var definedAnnotations = new Dictionary<AnnotationAttributeBase, Type>(); // Key - Annotation. Value - Annotated type.
			var addCurrentAnnotationToDefinedAnnotations = false;
			var currentAnnotatedTypeInheritanceLevel = -1;
			foreach (var currentAnnotatedType in annotatedType.GetHierarchyOfClass().Reverse()) {
				currentAnnotatedTypeInheritanceLevel++;
				foreach (var currentAnnotation in
					currentAnnotatedType
					.GetCustomAttributes(annotationAttributeBaseType, false)
					.Cast<AnnotationAttributeBase>()
					.Select(
						i => {
							i.SetTarget(new AnnotationTypeTarget(currentAnnotatedType, currentAnnotatedTypeInheritanceLevel));
							return i.AsReadOnly();
						})) {
					//
					currentAnnotation.ValidateData();
					if (currentAnnotatedType.IsSealed() && !currentAnnotation.Sealed)
						throw new AnnotationValidationException(FormatXResource(typeof(AnnotationReader), "ExceptionMessages/NotSealedAnnotationOnSealedType"), currentAnnotation, currentAnnotatedType);
					addCurrentAnnotationToDefinedAnnotations = true;
					foreach (var baseAnnotationKey in definedAnnotations.Keys.ToArray()) {
						if (currentAnnotation.Match(baseAnnotationKey) && currentAnnotatedType == definedAnnotations[ baseAnnotationKey ])
							throw new AnnotationValidationException(FormatXResource(typeof(AnnotationReader), "ExceptionMessages/DuplicatedAnnotation", baseAnnotationKey), currentAnnotation, currentAnnotatedType);
						if (baseAnnotationKey.IsOverridableBy(currentAnnotation)) {
							if (baseAnnotationKey.Sealed)
								throw new AnnotationValidationException(FormatXResource(typeof(AnnotationReader), "ExceptionMessages/OverridingSealed", baseAnnotationKey, definedAnnotations[ baseAnnotationKey ]), currentAnnotation, currentAnnotatedType);
							definedAnnotations.Remove(baseAnnotationKey);
							break;
						}
						else if (currentAnnotation.IsOverridableBy(baseAnnotationKey)) {
							if (currentAnnotation.Sealed)
								throw new AnnotationValidationException(FormatXResource(typeof(AnnotationReader), "ExceptionMessages/OverridingSealed", currentAnnotation, currentAnnotatedType), baseAnnotationKey, definedAnnotations[ baseAnnotationKey ]);
							addCurrentAnnotationToDefinedAnnotations = false;
							break;
						}
					}
					if (addCurrentAnnotationToDefinedAnnotations)
						definedAnnotations.Add(currentAnnotation, currentAnnotatedType);
				}
			}
			var result = definedAnnotations.Keys.ToArray();
			for (var i = 0; i < result.Length - 1; i++) {
				for (var j = i + 1; j < result.Length; j++) {
					if (result[ i ].IsOverridableBy(result[ j ]) || result[ j ].IsOverridableBy(result[ i ]))
						throw new AnnotationValidationException(FormatXResource(typeof(AnnotationReader), "ExceptionMessages/EffectiveAnnotationListIncorrect"), annotatedType);
				}
			}
			return result;
		}

		#endregion

		readonly Type _annotatedType;

		ValueHolderClass<EffectiveAnnotations> _annotations;

		internal AnnotationReader(Type annotatedType) {
			_annotatedType = annotatedType.EnsureNotNull(nameof(annotatedType)).EnsureClass();
		}

		public Type AnnotatedType
			=> _annotatedType;

		public EffectiveAnnotations Read() {
			var annotations = itrlck.Get(location: ref _annotations);
			if (annotations is null) {
				ValueHolderClass<EffectiveAnnotations> newAnnotations;
				try {
					newAnnotations = new ValueHolderClass<EffectiveAnnotations>(value: readNew());
				}
				catch (Exception exception) {
					newAnnotations = new ValueHolderClass<EffectiveAnnotations>(exception: exception);
				}
				annotations = itrlck.UpdateIfNull(location: ref _annotations, value: newAnnotations);
			}
			return annotations.Value;
			//
			EffectiveAnnotations readNew() {
				var locAnnotatedType = AnnotatedType;
				var effectiveAnnotationsArray = P_GetEffectiveAnnotations(annotatedType: locAnnotatedType);
				for (var i = 0; i < effectiveAnnotationsArray.Length; i++)
					effectiveAnnotationsArray[ i ].ValidateApplicationWith(otherEffectiveAnnotations: effectiveAnnotationsArray.Where(y => !ReferenceEquals(y, effectiveAnnotationsArray[ i ])));
				return new EffectiveAnnotations(effectiveAnnotationsArray);
			}
		}

	}

}