using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	[DataContract]
	public abstract class AsReadOnlyValidatableBase
		:IValidatable {

		readonly bool _isReadOnly;

		volatile bool _isReadOnlyAndValid;

		protected AsReadOnlyValidatableBase(bool isReadOnly, bool isValid) {
			_isReadOnly = isReadOnly;
			_isReadOnlyAndValid = isReadOnly && isValid;
		}

		protected AsReadOnlyValidatableBase(bool isReadOnly = default) {
			_isReadOnly = isReadOnly;
			_isReadOnlyAndValid = false;
		}

		protected AsReadOnlyValidatableBase(SerializationContext ctx) {
			_isReadOnly = false;
			_isReadOnlyAndValid = false;
		}

		private protected AsReadOnlyValidatableBase(SerializationContext ctx, bool isReadOnly, bool isValid) {
			_isReadOnly = isReadOnly;
			_isReadOnlyAndValid = isReadOnly && isValid;
		}

		protected void EnsureNotReadOnly() {
			if (_isReadOnly)
				throw new InvalidOperationException(message: FormatXResource(locator: typeof(InvalidOperationException), subpath: "CanNotModifyObjectDueReadOnly", args: this));
		}

		public bool IsReadOnly
			=> _isReadOnly;

		protected abstract void CreateReadOnlyCopy(out AsReadOnlyValidatableBase copy);

		public AsReadOnlyValidatableBase AsReadOnly() {
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
				catch (Exception exception) {
					throw new ValidationException(message: $"Object validation has revealed errors.{Environment.NewLine}\tObject:{this.FmtStr().GNLI2()}", innerException: exception);
				}
		}

		[OnDeserialized]
		void P_OnDeserialized(StreamingContext ctx)
			=> OnDeserialized(ctx: ctx);

		protected virtual void OnDeserialized(StreamingContext ctx) { }

	}

}