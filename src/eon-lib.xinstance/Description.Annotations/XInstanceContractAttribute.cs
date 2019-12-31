using System;
using System.Collections.Generic;
using System.Linq;

using Eon.Annotations;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Description.Annotations {

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class XInstanceContractAttribute
		:AnnotationAttributeBase {

		Type _contractType;

		bool _exclusiveContract;

		public XInstanceContractAttribute()
			: base(target: null, @sealed: true) {
			//
			_contractType = null;
			_exclusiveContract = false;
		}

		public XInstanceContractAttribute(AnnotationTypeTarget target = null, bool @sealed = false, Type contractType = null, bool exclusiveContract = false, bool isReadOnly = false)
		: base(target: target, @sealed: @sealed, isReadOnly: isReadOnly) {
			//
			_contractType = contractType;
			_exclusiveContract = exclusiveContract;
		}

		public XInstanceContractAttribute(
			XInstanceContractAttribute other,
			bool isReadOnly = false)
			: this(
				 target: other.EnsureNotNull(nameof(other)).Value.Target,
				 @sealed: other.Sealed,
				 contractType: other.ContractType,
				 exclusiveContract: other.ExclusiveContract,
				 isReadOnly: isReadOnly) { }

		public bool ExclusiveContract {
			get => _exclusiveContract;
			set {
				EnsureNotReadOnly();
				_exclusiveContract = value;
			}
		}

		public Type ContractType {
			get => _contractType;
			set {
				EnsureNotReadOnly();
				_contractType = value;
			}
		}

		public override bool Match(object obj) {
			var result = base.Match(obj);
			if (!result) {
				var other = obj as XInstanceContractAttribute;
				if (other != null)
					result = _contractType == other._contractType;
			}
			return result;
		}

		public override bool IsOverridableBy(AnnotationAttributeBase annotation) {
			XInstanceContractAttribute annotationAsXInstanceContract;
			if (base.IsOverridableBy(annotation) && (annotationAsXInstanceContract = annotation as XInstanceContractAttribute) != null) {
				var thisContractType = _contractType;
				var otherContractType = (annotationAsXInstanceContract)._contractType;
				return
					thisContractType == null
					? otherContractType == null
					: otherContractType != null && thisContractType.IsAssignableFrom(otherContractType);
			}
			else
				return false;
		}

		protected override void CreateReadOnlyCopy(out AnnotationAttributeBase readOnlyCopy)
			=> readOnlyCopy = new XInstanceContractAttribute(other: this, isReadOnly: true);

		public new XInstanceContractAttribute AsReadOnly()
			=> (XInstanceContractAttribute)base.AsReadOnly();

		// TODO: Put strings into the resources.
		//
		protected override void OnValidateData() {
			base.OnValidateData();
			//
			var contractType = _contractType;
			string errorMessage;
			if (contractType == null)
				throw
					new AnnotationValidationException(message: FormatXResource(typeof(XInstanceContractAttribute), "ExceptionMessages/ContractNotSpecified"), annotation: this, annotationTarget: Target);
			else if (!XInstanceContractUtilities.IsValidXInstanceContractType(contractType, out errorMessage))
				throw
					new AnnotationValidationException(message: $"Недопустимый тип контракта.{Environment.NewLine}{errorMessage}", annotation: this, annotationTarget: Target);
		}

		protected override void OnValidateApplicationWith(IEnumerable<AnnotationAttributeBase> otherEffectiveAnnotations) {
			base.OnValidateApplicationWith(otherEffectiveAnnotations);
			//
			XInstanceContractAttribute violator;
			if (_exclusiveContract && (violator = otherEffectiveAnnotations.OfType<XInstanceContractAttribute>().FirstOrDefault()) != null)
				throw new AnnotationValidationException(message: FormatXResource(typeof(XInstanceContractAttribute), "ExceptionMessages/ExclusivenessViolation", violator), annotation: this, annotationTarget: Target);
		}

		public override string ToString()
			=> FormatXResource(typeof(XInstanceContractAttribute), "ToString", base.ToString(), _contractType);

	}

}