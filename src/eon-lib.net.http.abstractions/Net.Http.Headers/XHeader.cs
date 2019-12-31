using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Net.Http.Headers {

	[DataContract]
	public class XHeader
		:Header, IAsReadOnly<XHeader> {

		#region Static members

		/// <summary>
		/// Value: <see cref="HeaderUtilities.DefaultXHeaderSuffix"/>.
		/// </summary>
		public static string DefaultSuffix
			=> HeaderUtilities.DefaultXHeaderSuffix;

		#endregion

		public XHeader()
			: this(name: null) { }

		public XHeader(string name = null, string value = null, bool isReadOnly = false)
			: base(name: name, value: value, isReadOnly: isReadOnly) { }

		public XHeader(Header other, bool isReadOnly = false)
			: this(name: other.EnsureNotNull(nameof(other)).Value.Name, value: other.Value, isReadOnly: isReadOnly) { }

		[JsonConstructor]
		protected XHeader(SerializationContext ctx)
			: base(ctx: ctx) { }

		protected sealed override void CreateReadOnlyCopy(out Header readOnlyCopy) {
			CreateReadOnlyCopy(out var locReadOnlyCopy);
			readOnlyCopy = locReadOnlyCopy;
		}

		protected virtual void CreateReadOnlyCopy(out XHeader readOnlyCopy)
			=> readOnlyCopy = new XHeader(other: this, isReadOnly: true);

		public new XHeader AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				CreateReadOnlyCopy(out var readOnlyCopy);
				return readOnlyCopy;
			}
		}

		protected override void OnValidate() {
			base.OnValidate();
			//
			Name.ArgProp(nameof(Name)).EnsureStartsWithOrdinalCI(startsWithValue: "X-");
		}

	}

}