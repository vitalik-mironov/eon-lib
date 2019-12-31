using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;

using Eon.Metadata;
using Eon.Runtime.Serialization;
using Eon.Xml.Schema;

using Newtonsoft.Json;

namespace Eon.Description {

	[DataContract(Namespace = EonXmlNamespaces.Description.Package)]
	[DebuggerDisplay("{ToString(),nq}")]
	public class DescriptionPackageIdentity
		:AsReadOnlyValidatableBase, IAsReadOnly<DescriptionPackageIdentity> {

		MetadataName _name;
		[DataMember(Order = 0, Name = nameof(Name), IsRequired = true)]
		string P_Name_DataMember {
			get { return _name?.Value; }
			set {
				try {
					_name = (MetadataName)value;
				}
				catch (Exception firstException) {
					throw
						new MemberDeserializationException(
							memberName: nameof(Name),
							type: GetType(),
							innerException: firstException);
				}
			}
		}

		[DataMember(Order = 1, Name = nameof(PublisherScopeId), IsRequired = true)]
		UriBasedIdentifier _publisherScopeId;

		Version _version;
		[DataMember(Order = 2, Name = nameof(Version), IsRequired = true)]
		string m_VersionDataMember {
			get { return _version?.FmtStr().G(formatProvider: CultureInfo.InvariantCulture); }
			set {
				try {
					_version = new Version(version: value);
				}
				catch (Exception firstException) {
					throw
						new MemberDeserializationException(
							memberName: nameof(Version),
							type: GetType(),
							innerException: firstException);
				}
			}
		}

		public DescriptionPackageIdentity()
			: this(name: null) { }

		public DescriptionPackageIdentity(
			MetadataName name = null,
			UriBasedIdentifier publisherScopeId = null,
			Version version = null,
			bool isReadOnly = false)
			: base(isReadOnly: isReadOnly) {
			//
			_name = name;
			_publisherScopeId = publisherScopeId;
			_version = version;
		}

		public DescriptionPackageIdentity(DescriptionPackageIdentity other, ArgumentPlaceholder<MetadataName> name = default, ArgumentPlaceholder<UriBasedIdentifier> publisherScopeId = default, ArgumentPlaceholder<Version> version = default, bool isReadOnly = default)
			: this(
					name:
						other
						.EnsureNotNull(nameof(other))
						.Value
						.Name
						.Fluent()
						.If(condition: name.HasExplicitValue, trueBody: locOtherName => name.ExplicitValue, falseBody: locOtherName => locOtherName),
					publisherScopeId: publisherScopeId.HasExplicitValue ? publisherScopeId.ExplicitValue : other.PublisherScopeId,
					version: version.HasExplicitValue ? version.ExplicitValue : other.Version,
					isReadOnly: isReadOnly) { }

		[JsonConstructor]
		protected DescriptionPackageIdentity(SerializationContext ctx)
			: base(ctx: ctx) { }

		public MetadataName Name {
			get => _name;
			set {
				EnsureNotReadOnly();
				_name = value;
			}
		}

		public UriBasedIdentifier PublisherScopeId {
			get => _publisherScopeId;
			set {
				EnsureNotReadOnly();
				_publisherScopeId = value;
			}
		}

		public Version Version {
			get => _version;
			set {
				EnsureNotReadOnly();
				_version = value;
			}
		}

		protected sealed override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase copy) {
			CreateReadOnlyCopy(copy: out var locCopy);
			copy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out DescriptionPackageIdentity copy)
			=> copy = new DescriptionPackageIdentity(other: this, isReadOnly: true);

		public new DescriptionPackageIdentity AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				CreateReadOnlyCopy(copy: out var copy);
				return copy;
			}
		}

		protected override void OnValidate() {
			Name
				.ArgProp(nameof(Name))
				.EnsureNotNull();
			//
			PublisherScopeId
				.ArgProp(nameof(PublisherScopeId))
				.EnsureNotNull();
			//
			Version
				.ArgProp(nameof(Version))
				.EnsureNotNull();
		}

		public override string ToString()
			=> $"{_name.FmtStr().G(formatProvider: CultureInfo.InvariantCulture)}, {_publisherScopeId.FmtStr().G(formatProvider: CultureInfo.InvariantCulture)}, {_version.FmtStr().PrefixVInvariant()}";

	}

}