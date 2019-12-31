using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Annotations {

#pragma warning disable CS3015 // Type has no accessible constructors which use only CLS-compliant types
	public abstract class AnnotationAttributeBase
#pragma warning restore CS3015 // Type has no accessible constructors which use only CLS-compliant types
		:Attribute, IAsReadOnly<AnnotationAttributeBase>, ITypeAnnotation {

		readonly bool _isReadOnly;

		bool _isDataValidated;

		bool _notSealed;

		AnnotationTypeTarget _target;

		protected AnnotationAttributeBase(AnnotationTypeTarget target = default, bool @sealed = default, bool isReadOnly = default) {
			_target = target;
			_notSealed = !@sealed;
			_isReadOnly = isReadOnly;
		}

		public override bool Match(object obj) {
			if (obj is null)
				return false;
			else
				return Equals(obj);
		}

		/// <summary>
		/// Возвращает объект (см. <seealso cref="AnnotationTypeTarget"/>), к которому относится данная аннотация.
		/// </summary>
		public AnnotationTypeTarget Target
			=> _target;

		// TODO: Put strings into the resources.
		//
		public void SetTarget(AnnotationTypeTarget target) {
			target.EnsureNotNull(nameof(target));
			//
			EnsureNotReadOnly();
			var originalTarget =
				Interlocked
				.CompareExchange(ref _target, target, null);
			if (originalTarget != null
				&& !ReferenceEquals(originalTarget, target)
				&& originalTarget.Type != target.Type)
				throw
					new InvalidOperationException(
						$"Невозможно для аннотации в качестве цели (свойство '{nameof(Target)}') установить указанный объект, так как для аннотации уже определена другая цель.{Environment.NewLine}\tАннотация:{this.FmtStr().GNLI2()}{Environment.NewLine}\tОбъект, устанавливаемый как цель аннотации:{target.FmtStr().GNLI2()}");
		}

		/// <summary>
		/// Указывает, что тип, наследуемый от типа, для которого определена данная аннотация, не может её переопределять.
		/// </summary>
		public bool Sealed {
			get { return !_notSealed; }
			set {
				EnsureNotReadOnly();
				_notSealed = !value;
			}
		}

		public virtual bool IsOverridableBy(AnnotationAttributeBase annotation) {
			annotation.EnsureNotNull(nameof(annotation));
			// Тип указанной аннотации (annotation) является наследником типа данной аннотации (this).
			//
#if TRG_SL5

			return GetType().IsAssignableFrom(annotation.GetType());

#else

			return GetType().GetTypeInfo().IsAssignableFrom(annotation.GetType());

#endif
		}

		public void ValidateApplicationWith(IEnumerable<AnnotationAttributeBase> otherEffectiveAnnotations)
			=>
			OnValidateApplicationWith(otherEffectiveAnnotations);

		public void ValidateData() {
			if (!(IsReadOnly && _isDataValidated)) {
				try {
					OnValidateData();
				}
				catch (Exception exception) when (!(exception is AnnotationValidationException)) {
					throw new AnnotationValidationException(FormatXResource(typeof(AnnotationAttributeBase), "ExceptionMessages/ValidateDataError"), this, Target, exception);
				}
				//
				if (IsReadOnly)
					_isDataValidated = true;
			}
		}

		protected virtual void OnValidateData() {
			if (Target is null)
				throw new EonException($"Не указан объект (свойство '{nameof(Target)}'), к которому относится данная аннотация.");
		}

		protected virtual void OnValidateApplicationWith(IEnumerable<AnnotationAttributeBase> otherEffectiveAnnotations) {
			otherEffectiveAnnotations.EnsureNotNull(nameof(otherEffectiveAnnotations));
			//
		}

		#region IAsReadOnly`1 impls.

		public bool IsReadOnly
			=> _isReadOnly;

		protected void EnsureNotReadOnly() {
			if (_isReadOnly)
				throw new InvalidOperationException(FormatXResource(typeof(InvalidOperationException), "CanNotModifyObjectDueReadOnly", this));
		}

		protected abstract void CreateReadOnlyCopy(out AnnotationAttributeBase readOnlyCopy);

		public AnnotationAttributeBase AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				AnnotationAttributeBase readOnlyCopy;
				CreateReadOnlyCopy(out readOnlyCopy);
				return readOnlyCopy;
			}
		}

		#endregion

		public override string ToString()
			=> FormatXResource(typeof(AnnotationAttributeBase), "ToString", GetType(), Sealed);

	}

}