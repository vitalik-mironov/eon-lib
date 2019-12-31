#pragma warning disable IDE1006 // Naming Styles

using System;
using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

using Newtonsoft.Json;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Metadata {

	[DataContract(Name = "MetadataPathNameRef")]
	public sealed class MetadataPathNameRef<TMetadata>
		:MetadataReference<TMetadata>, IMetadataPathNameReference
		where TMetadata : class, IMetadata {

		MetadataPathName _name;
		[DataMember(Order = 0, Name = nameof(Name), IsRequired = true)]
		string m_NameDataMember {
			get { return _name; }
			set {
				try {
					_name = (MetadataPathName)value;
				}
				catch (Exception firstException) {
					throw new MemberDeserializationException(GetType(), nameof(Name), firstException);
				}
			}
		}

		public MetadataPathNameRef(MetadataPathName name) {
			name.EnsureNotNull(nameof(name));
			if (name.IsEmpty)
				throw new ArgumentException(message: FormatXResource(typeof(MetadataReference), "PathNameRef/NameCanNotBeEmpty"), paramName: nameof(name));
			//
			_name = name;
		}

		MetadataPathNameRef() { }

		[JsonConstructor]
		MetadataPathNameRef(SerializationContext ctx)
			: base(ctx: ctx) { }

		public MetadataPathName Name
			=> _name;

		protected override void OnDeserialized(StreamingContext context) {
			var name = _name;
			if (name == null)
				throw
					new MemberDeserializationException(
						message: FormatXResource(typeof(MetadataReference), "PathNameRef/NameCanNotBeNull"),
						type: GetType(),
						memberName: nameof(Name));
			else if (name.IsEmpty)
				throw
					new MemberDeserializationException(
						message: FormatXResource(typeof(MetadataReference), "PathNameRef/NameCanNotBeEmpty"),
						type: GetType(),
						memberName: nameof(Name));
			//
			base.OnDeserialized(context);
		}

		public override MetadataName TargetMetadataName
			=> _name == null || _name.IsEmpty ? null : _name[ _name.ComponentCount - 1 ];

		protected override IMetadataReferenceResolver GetResolver()
			=> MetadataPathNameRefResolver.Instance;

		public override string ToString()
			=> FormatXResource(typeof(MetadataReference), "PathNameRef/ToString", typeof(TMetadata), _name);

		public MetadataPathNameRef<T> CastToRefOf<T>()
			where T : MetadataBase
			=> new MetadataPathNameRef<T>() { _name = _name };

	}

}