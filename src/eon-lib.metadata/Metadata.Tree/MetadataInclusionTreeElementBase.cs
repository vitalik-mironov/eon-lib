using System;
using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

namespace Eon.Metadata.Tree {

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0034 // Simplify 'default' expression

	[DataContract]
	public abstract class MetadataInclusionTreeElementBase
		:MetadataTreeElementBase, IMetadataInclusionTreeElement {

		Uri _locationUri;
		[DataMember(Order = 0, Name = nameof(LocationUri), IsRequired = true)]
		string P_LocationUri_DataMember {
			get { return ReadDA(ref _locationUri)?.ToString(); }
			set {
				try {
					value.Arg(nameof(value)).EnsureNotNullOrWhiteSpace();
					//
					var valueAsUri = new Uri(value, UriKind.RelativeOrAbsolute);
					OnValidateLocationUri(valueAsUri);
					WriteDA(ref _locationUri, valueAsUri);
				}
				catch (Exception exception) {
					throw new MemberDeserializationException(GetType(), nameof(LocationUri), exception);
				}
			}
		}

		string _formatMediaTypeName;
		[DataMember(Order = 1, Name = nameof(FormatMediaTypeName), IsRequired = true)]
		string P_FormatMediaTypeName_DataMember {
			get { return ReadDA(ref _formatMediaTypeName); }
			set {
				try {
					OnValidateFormatMediaTypeName(name: value.Arg(nameof(value)));
					WriteDA(ref _formatMediaTypeName, value);
				}
				catch (Exception exception) {
					throw new MemberDeserializationException(GetType(), nameof(LocationUri), exception);
				}
			}
		}

		protected MetadataInclusionTreeElementBase(IMetadataTreeNode node, ReadOnlyStateTag readOnlyState = default(ReadOnlyStateTag))
			: base(node: node, readOnlyState: readOnlyState) { }

		protected MetadataInclusionTreeElementBase(ReadOnlyStateTag readOnlyState = default(ReadOnlyStateTag))
			: this(node: null, readOnlyState: readOnlyState) { }

		protected MetadataInclusionTreeElementBase(SerializationContext ctx)
			: base(ctx: ctx) { }

		public Uri LocationUri {
			get { return ReadDA(ref _locationUri); }
			set {
				value.EnsureNotNull(nameof(value));
				//
				Uri currentValue;
				if ((currentValue = ReadDA(ref _locationUri)) != value) {
					EnsureNotReadOnly();
					OnValidateLocationUri(value);
					WriteDA(ref _locationUri, value);
				}
			}
		}

		public string FormatMediaTypeName {
			get { return ReadDA(ref _formatMediaTypeName); }
			set {
				value.EnsureNotNull(nameof(value));
				//
				string currentValue;
				if (!value.EqualsOrdinalCS(currentValue = ReadDA(ref _formatMediaTypeName))) {
					EnsureNotReadOnly();
					//
					OnValidateFormatMediaTypeName(name: value.Arg(nameof(value)));
					WriteDA(ref _formatMediaTypeName, value);
				}
			}
		}

		protected virtual void OnValidateLocationUri(Uri locationUri) {
			locationUri.EnsureNotNull(nameof(locationUri));
			//
		}

		protected virtual void OnValidateFormatMediaTypeName(ArgumentUtilitiesHandle<string> name) {
			name
				.EnsureNotNull()
				.EnsureNotEmptyOrWhiteSpace();
			//
		}

		public override string ToString()
			=>
			base.ToString()
			+ $"{Environment.NewLine}URI расположения метаданных:{Environment.NewLine}{_locationUri.FmtStr().G().IndentLines()}";

		protected override void Dispose(bool explicitDispose) {
			_locationUri = null;
			_formatMediaTypeName = null;
			base.Dispose(explicitDispose);
		}

	}

#pragma warning restore IDE0034 // Simplify 'default' expression
#pragma warning restore IDE1006 // Naming Styles

}