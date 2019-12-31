using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Net.Http.Headers {

	[DataContract]
	public class Header
		:AsReadOnlyValidatableBase, IAsReadOnly<Header> {

		[DataMember(Order = 0, Name = nameof(Name), IsRequired = true)]
		string _name;

		[DataMember(Order = 1, Name = nameof(Value), IsRequired = true)]
		string _value;

		public Header()
			: this(name: null) { }

		public Header(string name = null, string value = null, bool isReadOnly = false)
			: base(isReadOnly: isReadOnly) {
			//
			_name = name;
			_value = value;
		}

		public Header(Header other, bool isReadOnly = false)
			: this(name: other.EnsureNotNull(nameof(other)).Value.Name, value: other.Value, isReadOnly: isReadOnly) { }

		[JsonConstructor]
		protected Header(SerializationContext ctx)
			: base(ctx: ctx) { }

		public string Name {
			get => _name;
			set {
				EnsureNotReadOnly();
				_name = value;
			}
		}

		public string Value {
			get => _value;
			set {
				EnsureNotReadOnly();
				_value = value;
			}
		}

		protected sealed override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locReadOnlyCopy);
			readOnlyCopy = locReadOnlyCopy;
		}

		protected virtual void CreateReadOnlyCopy(out Header readOnlyCopy)
			=> readOnlyCopy = new Header(other: this, isReadOnly: true);

		public new Header AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				CreateReadOnlyCopy(out var readOnlyCopy);
				return readOnlyCopy;
			}
		}

		protected override void OnValidate() {
			var name = Name;
			var value = Value;
			//
			name.ArgProp(nameof(Name)).EnsureNotNull().EnsureNotEmpty().EnsureHasMaxLength(maxLength: HeaderUtilities.ConventionalMaxLengthOfHeaderName).EnsureNoWhiteSpace();
			value.ArgProp(nameof(value)).EnsureNotNull().EnsureHasMaxLength(HeaderUtilities.ConventionalMaxLengthOfHeaderValue);
		}

	}

}