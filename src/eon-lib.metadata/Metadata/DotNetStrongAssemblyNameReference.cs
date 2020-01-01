using System;
using System.Globalization;
using System.Runtime.Serialization;

using Eon.Reflection;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

using static Eon.Resources.XResource.XResourceUtilities;

namespace Eon.Metadata {

	[DataContract]
	public sealed class DotNetStrongAssemblyNameReference
		:MetadataBase {

		public const int PublicKeyTokenLength = 8;

		public const int NameMaxLength = 256;

		public const char FullyQualifiedNameComponentDelimiter = EonTypeUtilities.TypeAssemblyQualifiedNamePartDelimiter;

		[DataMember(Name = nameof(AssemblyName), IsRequired = true, Order = 0)]
		string _assemblyName;

		Version _version;

		[DataMember(Name = nameof(Culture), IsRequired = true, Order = 2)]
		string _culture;

		byte[ ] _publicKeyToken;

		[DataMember(Name = nameof(Version), IsRequired = true, Order = 1)]
#pragma warning disable IDE1006 // Naming Styles
		string m_VersionDataMember {
#pragma warning restore IDE1006 // Naming Styles
			get { return ReadDA(ref _version)?.ToString(4); }
			set {
				if (value == null)
					_version = null;
				else
					try {
						_version = new Version(value);
					} catch (Exception firstException) {
						throw new MemberDeserializationException(GetType(), nameof(Version), firstException);
					}
			}
		}

		[DataMember(Name = nameof(PublicKeyToken), IsRequired = true, Order = 3)]
#pragma warning disable IDE1006 // Naming Styles
		string m_PublicKeyTokenDataMember {
#pragma warning restore IDE1006 // Naming Styles
			get { return ByteArrayUtilities.ToHexString(ReadDA(ref _publicKeyToken)); }
			set {
				try {
					_publicKeyToken = ByteArrayUtilities.FromHexString(value);
				} catch (Exception firstException) {
					throw new MemberDeserializationException(GetType(), nameof(PublicKeyToken), firstException);
				}
			}
		}

		DotNetStrongAssemblyNameReference(Metadata.MetadataName name)
			: base(name) { }

		[JsonConstructor]
		DotNetStrongAssemblyNameReference(SerializationContext ctx)
			: base(ctx: ctx) { }

		protected override void OnDeserialized(StreamingContext context) {
			base.OnDeserialized(context);
			//
			if (string.IsNullOrWhiteSpace(_assemblyName) || _assemblyName.Length > NameMaxLength || _assemblyName.IndexOf(FullyQualifiedNameComponentDelimiter) > -1 || _version == null || string.IsNullOrWhiteSpace(_culture) || _publicKeyToken == null || _publicKeyToken.Length != PublicKeyTokenLength)
#pragma warning disable CS0618 // Type or member is obsolete

				throw new SerializationException(FormatXResource(locator: typeof(DotNetStrongAssemblyNameReference), subpath: "ExceptionMessages/DeserializedNotValid", args: this));

#pragma warning restore CS0618 // Type or member is obsolete
		}

		public string AssemblyName { get { return ReadDA(ref _assemblyName); } }

		public Version Version { get { return ReadDA(ref _version); } }

		public string Culture { get { return ReadDA(ref _culture); } }

		public byte[ ] PublicKeyToken {
			get {
				var result = ReadDA(ref _publicKeyToken);
				return result.CloneAs();
			}
		}

		public string ToFullyQualifiedName() {
			var name = _assemblyName;
			var version = _version;
			var culture = _culture;
			var publicKeyToken = _publicKeyToken;
			EnsureNotDisposeState();
			return string.Format("{0}{4} Version={1}{4} Culture={2}{4} PublicKeyToken={3}", name, version, culture, publicKeyToken == null ? "null" : ByteArrayUtilities.ToHexString(publicKeyToken), new string(FullyQualifiedNameComponentDelimiter, 1));
		}

		public override string ToString() {
			var publicKeyToken = _publicKeyToken;
			return string.Format(CultureInfo.InvariantCulture, "{0} {1}", base.ToString(), FormatXResource(typeof(DotNetStrongAssemblyNameReference), "ToString", _assemblyName, _version, _culture, (publicKeyToken == null ? null : ByteArrayUtilities.ToHexString(publicKeyToken))));
		}

		protected override void Dispose(bool explicitDispose) {
			_culture = null;
			_assemblyName = null;
			_publicKeyToken = null;
			_version = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}