using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	[DataContract]
	public abstract class AsReadOnlyValidatableDisposableBase
		:Disposable, IValidatable {

		readonly bool _isReadOnly;

		volatile bool _isReadOnlyAndValid;

		protected AsReadOnlyValidatableDisposableBase(bool isReadOnly = false) {
			_isReadOnly = isReadOnly;
		}

		protected AsReadOnlyValidatableDisposableBase(SerializationContext context) { }

		public bool IsReadOnly
			=> _isReadOnly;

		protected void EnsureNotReadOnly() {
			if (_isReadOnly)
				throw new InvalidOperationException(FormatXResource(typeof(InvalidOperationException), "CanNotModifyObjectDueReadOnly", this));
		}

		protected abstract void CreateReadOnlyCopy(out AsReadOnlyValidatableDisposableBase copy);

		public AsReadOnlyValidatableDisposableBase AsReadOnly() {
			if (_isReadOnly)
				return this;
			else {
				CreateReadOnlyCopy(out var copy);
				return copy;
			}
		}

		protected abstract void OnValidate();

		// TODO: Put strings into the resources.
		//
		public void Validate() {
			if (!_isReadOnlyAndValid)
				try {
					OnValidate();
					_isReadOnlyAndValid = IsReadOnly;
				}
				catch (Exception firstException) {
					throw
						new ValidationException(
							message: $"Валидация объекта выявила ошибки.{Environment.NewLine}\tОбъект:{this.FmtStr().GNLI2()}",
							innerException: firstException);
				}
		}

	}

}