using System;
using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon {

	[DataContract]
	public abstract class AsReadOnlyBase {

		readonly bool _isReadOnly;

		protected AsReadOnlyBase(bool isReadOnly = default) {
			_isReadOnly = isReadOnly;
		}

		protected AsReadOnlyBase(SerializationContext ctx) { }

		protected void EnsureNotReadOnly() {
			if (_isReadOnly)
				throw new InvalidOperationException(message: FormatXResource(locator: typeof(InvalidOperationException), subpath: "CanNotModifyObjectDueReadOnly", args: this));
		}

		public bool IsReadOnly
			=> _isReadOnly;

		protected abstract void CreateReadOnlyCopy(out AsReadOnlyBase copy);

		public AsReadOnlyBase AsReadOnly() {
			if (_isReadOnly)
				return this;
			else {
				CreateReadOnlyCopy(out var copy);
				return copy;
			}
		}

		[OnDeserialized]
		void P_OnDeserialized(StreamingContext ctx)
			=> OnDeserialized(ctx: ctx);

		protected virtual void OnDeserialized(StreamingContext ctx) { }

	}

}