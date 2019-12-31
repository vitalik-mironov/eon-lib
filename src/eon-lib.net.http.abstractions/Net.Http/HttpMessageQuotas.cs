using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

namespace Eon.Net.Http {

	[DataContract]
	public class HttpMessageQuotas
		:AsReadOnlyValidatableBase, IHttpMessageQuotas {

		int? _maxContentLength;

		public HttpMessageQuotas(bool isReadOnly)
			: this(maxContentLength: null, isReadOnly: isReadOnly) { }

		public HttpMessageQuotas()
			: this(maxContentLength: default) { }

		public HttpMessageQuotas(IHttpMessageQuotas other, bool isReadOnly = default)
			: this(maxContentLength: other.EnsureNotNull(nameof(other)).Value.MaxContentLength, isReadOnly: isReadOnly) { }

		public HttpMessageQuotas(int? maxContentLength = default, bool isReadOnly = default)
			: base(isReadOnly: isReadOnly) {
			//
			_maxContentLength = maxContentLength;
		}

		protected HttpMessageQuotas(SerializationContext ctx)
			: base(ctx: ctx) { }

		[DataMember(Order = 0, IsRequired = false, EmitDefaultValue = false)]
		public int? MaxContentLength {
			get => _maxContentLength;
			set {
				EnsureNotReadOnly();
				_maxContentLength = value;
			}
		}

		protected sealed override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locReadOnlyCopy);
			readOnlyCopy = locReadOnlyCopy;
		}

		protected virtual void CreateReadOnlyCopy(out HttpMessageQuotas readOnlyCopy)
			=> readOnlyCopy = new HttpMessageQuotas(this, isReadOnly: true);

		public new HttpMessageQuotas AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				HttpMessageQuotas readOnlyCopy;
				CreateReadOnlyCopy(out readOnlyCopy);
				return readOnlyCopy;
			}
		}

		IHttpMessageQuotas IAsReadOnlyMethod<IHttpMessageQuotas>.AsReadOnly()
			=> AsReadOnly();

		protected override void OnValidate() {
			MaxContentLength.ArgProp(nameof(MaxContentLength)).EnsureNotLessThanZero();
		}

	}

}