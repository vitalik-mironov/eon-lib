using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using Eon.Collections;
using Eon.Metadata;
using Eon.Runtime.Options;
using Eon.Runtime.Serialization;
using Eon.Text;
using Eon.Xml.Schema;
using Newtonsoft.Json;
using vlt = Eon.Threading.VolatileUtilities;

namespace Eon.Description {

	/// <summary>
	/// Locator (reference) to a description package.
	/// </summary>
	[DataContract(Namespace = EonXmlNamespaces.Description.Package)]
	[DebuggerDisplay("{ToString(),nq}")]
	public class DescriptionPackageLocator
		:IEquatable<DescriptionPackageLocator> {

		#region Nested types

		sealed class P_PackageNameEqualityComparer
			:IEqualityComparer<DescriptionPackageLocator> {

			internal P_PackageNameEqualityComparer() { }

			public bool Equals(DescriptionPackageLocator x, DescriptionPackageLocator y)
				=> ReferenceEquals(x, y) ? true : (x is null || y is null ? false : x.PackageNameEqualityKey == y.PackageNameEqualityKey);

			public int GetHashCode(DescriptionPackageLocator value) {
				value.EnsureNotNull(nameof(value));
				//
				value.PackageNameEqualityKey.Split(out var keyLeft, out var keyRight);
				unchecked {
					return (keyLeft * 37) ^ (keyRight * 19);
				}
			}

		}

		sealed class P_PackageVersionEqualityComparer
			:IEqualityComparer<DescriptionPackageLocator> {

			internal P_PackageVersionEqualityComparer() { }

			public bool Equals(DescriptionPackageLocator x, DescriptionPackageLocator y)
				=> ReferenceEquals(x, y) ? true : (x is null || y is null ? false : x.PackageVersionEqualityKey == y.PackageVersionEqualityKey);

			public int GetHashCode(DescriptionPackageLocator value) {
				value.EnsureNotNull(nameof(value));
				//
				value.PackageVersionEqualityKey.Split(out var keyLeft, out var keyRight);
				unchecked {
					return (keyLeft * 37) ^ (keyRight * 19);
				}
			}

		}

		sealed class P_FullEqualityComparer
			:IEqualityComparer<DescriptionPackageLocator> {

			internal P_FullEqualityComparer() { }

			public bool Equals(DescriptionPackageLocator x, DescriptionPackageLocator y)
				=>
				ReferenceEquals(x, y)
				? true
				: (x is null || y is null ? false : x.FullEqualityKey == y.FullEqualityKey);

			public int GetHashCode(DescriptionPackageLocator value) {
				value.EnsureNotNull(nameof(value));
				//
				value.FullEqualityKey.Split(out var keyLeft, out var keyRight);
				unchecked {
					return (keyLeft * 37) ^ (keyRight * 19);
				}
			}

		}

		#endregion

		#region Static & constant members

		protected const int PackageNameEqualityKeyType = 0;

		protected const int PackageVersionEqualityKeyType = 1;

		protected const int FullEqualityKeyType = 2;

		const string __DataStringDataMemberName = "Data";

		/// <summary>
		/// Value: '4096'.
		/// </summary>
		public static int DefaultDeserializationDataStringMaxLength = 4096;

		/// <summary>
		/// Value: <see cref="ArgumentUtilitiesCoreL1.KeyValuePairDefaultDelimiter"/>.
		/// </summary>
		public static readonly char DataComponentDelimiter = ArgumentUtilitiesCoreL1.KeyValuePairDefaultDelimiter;

		protected static readonly string DataComponentDelimiterString;

		/// <summary>
		/// Value: <seealso cref="ArgumentUtilitiesCoreL1.KeyValueDefaultDelimiter"/>.
		/// </summary>
		public static readonly char DataComponentKeyValueDelimiter = ArgumentUtilitiesCoreL1.KeyValueDefaultDelimiter;

		protected static readonly string DataComponentKeyValueDelimiterString;

		/// <summary>
		/// Value: <see cref="StringComparer.OrdinalIgnoreCase"/>.
		/// </summary>
		public static readonly StringComparer DataComponentKeyComparer;

		/// <summary>
		/// Value: 'package-base-uri'.
		/// </summary>
		public static readonly string PackageLocationBaseUriDataComponentKey = "package-base-uri";

		/// <summary>
		/// Value: 'l'.
		/// </summary>
		public static readonly string AltPackageLocationBaseUriDataComponentKey = "l";

		/// <summary>
		/// Value: 'package-name'.
		/// </summary>
		public static readonly string PackageNameDataComponentKey = "package-name";

		/// <summary>
		/// Value: 'n'.
		/// </summary>
		public static readonly string AltPackageNameDataComponentKey = "n";

		/// <summary>
		/// Value: 'package-version'.
		/// </summary>
		public static readonly string PackageVersionDataComponentKey = "package-version";

		/// <summary>
		/// Value: 'v'.
		/// </summary>
		public static readonly string AltPackageVersionDataComponentKey = "v";

		/// <summary>
		/// Value: 'satellite-package-name'.
		/// </summary>
		public static readonly string SatellitePackageNameDataComponentKey = "satellite-package-name";

		/// <summary>
		/// Value: 'sn'.
		/// </summary>
		public static readonly string AltSatellitePackageNameDataComponentKey = "sn";

		/// <summary>
		/// Value: 'package-publisher-scope-id'.
		/// </summary>
		public static readonly string PackagePublisherScopeIdDataComponentKey = "package-publisher-scope-id";

		/// <summary>
		/// Value: 's'.
		/// </summary>
		public static readonly string AltPackagePublisherScopeIdDataComponentKey = "s";

		public static readonly IEqualityComparer<DescriptionPackageLocator> PackageNameEqualityComparer;

		public static readonly IEqualityComparer<DescriptionPackageLocator> PackageVersionEqualityComparer;

		static readonly P_FullEqualityComparer __FullEqualityComparer;

		public static readonly IEqualityComparer<DescriptionPackageLocator> DefaultEqualityComparer;

		static DescriptionPackageLocator() {
			PackageNameEqualityComparer = new P_PackageNameEqualityComparer();
			PackageVersionEqualityComparer = new P_PackageVersionEqualityComparer();
			__FullEqualityComparer = new P_FullEqualityComparer();
			DefaultEqualityComparer = __FullEqualityComparer;
			//
			DataComponentDelimiterString = new string(DataComponentDelimiter, 1);
			DataComponentKeyValueDelimiterString = new string(DataComponentKeyValueDelimiter, 1);
			DataComponentKeyComparer = StringComparer.OrdinalIgnoreCase;
		}

		public static bool Equals(DescriptionPackageLocator a, DescriptionPackageLocator b)
			=> ReferenceEquals(a, b) ? true : (a is null ? false : a.Equals(other: b));

		public static void Parse(string data, out DescriptionPackageLocator locator, ArgumentPlaceholder<bool> useDefaults = default)
			=> Parse(data: data.Arg(nameof(data)), locator: out locator, useDefaults: useDefaults);

		// TODO: Put strings into the resources.
		//
		public static void Parse(ArgumentUtilitiesHandle<string> data, out DescriptionPackageLocator locator, ArgumentPlaceholder<bool> useDefaults = default) {
			data.EnsureNotNull();
			//
			var locLocator = new DescriptionPackageLocator(ctx: default);
			try {
				locLocator.Deserialize(data: data, useDefaults: useDefaults);
			}
			catch (Exception exception) {
				throw
					new FormatException(
						message: $"Ошибка преобразования указанной строки в объект локатора (ссылки).{Environment.NewLine}\tСтрока:{data.Value.FmtStr().GNLI2()}",
						innerException: exception);
			}
			locator = locLocator;
		}

		#endregion

		string _deserializationDataString;

		Uri _packageLocationBaseUri;

		MetadataName _packageName;

		Version _packageVersion;

		MetadataName _satellitePackageName;

		UriBasedIdentifier _packagePublisherScopeId;

		ValueHolderClass<long> _packageVersionEqualityKey;

		ValueHolderClass<long> _packageNameEqualityKey;

		ValueHolderClass<long> _fullEqualityKey;

		[DataMember(Order = 0, IsRequired = true, Name = __DataStringDataMemberName)]
		string P_DataString_DataMember {
			get { return Serialize(); }
			set {
				try {
					Deserialize(data: value.Arg(nameof(value)));
				}
				catch (Exception exception) {
					throw
						new MemberDeserializationException(innerException: exception, memberName: __DataStringDataMemberName, type: GetType());
				}
			}
		}

		[JsonConstructor]
		protected DescriptionPackageLocator(SerializationContext ctx) { }

		public DescriptionPackageLocator(
			MetadataName packageName,
			UriBasedIdentifier packagePublisherScopeId,
			MetadataName satellitePackageName = default,
			Uri packageLocationBaseUri = default,
			Version packageVersion = default,
			ArgumentPlaceholder<bool> useDefaults = default) {
			//
			if (useDefaults.HasExplicitValue ? useDefaults.ExplicitValue : UseDescriptionLocatorDefaultsOption.Require()) {
				packageName = packageName ?? DescriptionLocatorPackageNameOption.Require();
				packagePublisherScopeId = packagePublisherScopeId ?? DescriptionLocatorPackagePublisherScopeIdOption.Require();
				packageLocationBaseUri = packageLocationBaseUri ?? DescriptionLocatorPackageLocationBaseUriOption.Require();
			}
			else {
				packageName.EnsureNotNull(nameof(packageName));
				packagePublisherScopeId.EnsureNotNull(nameof(packagePublisherScopeId));
			}
			//
			_packageName = packageName;
			_packagePublisherScopeId = packagePublisherScopeId;
			_satellitePackageName = satellitePackageName;
			_packageLocationBaseUri = packageLocationBaseUri;
			_packageVersion = packageVersion;
		}

		public DescriptionPackageLocator(
			DescriptionPackageLocator other,
			ArgumentPlaceholder<MetadataName> packageName = default,
			ArgumentPlaceholder<UriBasedIdentifier> packagePublisherScopeId = default,
			ArgumentPlaceholder<MetadataName> satellitePackageName = default,
			ArgumentPlaceholder<Uri> packageLocationBaseUri = default,
			ArgumentPlaceholder<Version> packageVersion = default)
			: this(
					packageName: packageName.Substitute(value: other.EnsureNotNull(nameof(other)).Value.PackageName),
					packagePublisherScopeId: packagePublisherScopeId.Substitute(value: other.PackagePublisherScopeId),
					satellitePackageName: satellitePackageName.Substitute(value: other.SatellitePackageName),
					packageLocationBaseUri: packageLocationBaseUri.Substitute(value: other.PackageLocationBaseUri),
					packageVersion: packageVersion.Substitute(other.PackageVersion)) { }

		public MetadataName PackageName
			=> _packageName;

		public UriBasedIdentifier PackagePublisherScopeId
			=> _packagePublisherScopeId;

		public MetadataName SatellitePackageName
			=> _satellitePackageName;

		public Uri PackageLocationBaseUri
			=> _packageLocationBaseUri;

		public Version PackageVersion
			=> _packageVersion;

		public long FullEqualityKey {
			get {
				var key = vlt.Read(ref _fullEqualityKey);
				if (key is null) {
					ValueHolderClass<long> newComputedKey;
					try {
						newComputedKey = new ValueHolderClass<long>(value: ComputeEqualityKey(keyType: FullEqualityKeyType));
					}
					catch (Exception exception) {
						newComputedKey = new ValueHolderClass<long>(exception: exception);
					}
					key = Interlocked.CompareExchange(location1: ref _fullEqualityKey, value: newComputedKey, comparand: null) ?? newComputedKey;
				}
				return key.Value;
			}
		}

		public long PackageVersionEqualityKey {
			get {
				var key = vlt.Read(ref _packageVersionEqualityKey);
				if (key is null) {
					ValueHolderClass<long> newComputedKey;
					try {
						newComputedKey = new ValueHolderClass<long>(value: ComputeEqualityKey(keyType: PackageVersionEqualityKeyType));
					}
					catch (Exception exception) {
						newComputedKey = new ValueHolderClass<long>(exception: exception);
					}
					key = Interlocked.CompareExchange(location1: ref _packageVersionEqualityKey, value: newComputedKey, comparand: null) ?? newComputedKey;
				}
				return key.Value;
			}
		}

		public long PackageNameEqualityKey {
			get {
				var key = vlt.Read(ref _packageNameEqualityKey);
				if (key is null) {
					ValueHolderClass<long> newComputedKey;
					try {
						newComputedKey = new ValueHolderClass<long>(value: ComputeEqualityKey(keyType: PackageNameEqualityKeyType));
					}
					catch (Exception exception) {
						newComputedKey = new ValueHolderClass<long>(exception: exception);
					}
					key = Interlocked.CompareExchange(location1: ref _packageNameEqualityKey, value: newComputedKey, comparand: null) ?? newComputedKey;
				}
				return key.Value;
			}
		}

		protected long ComputeEqualityKey(int keyType) {
			byte[ ] computedHash;
			using (var hashAlg = new SHA1Managed()) {
				hashAlg.Initialize();
				//
				using (var memoryStream = new MemoryStream())
				using (var writer = new BinaryWriter(output: memoryStream, encoding: Encoding.UTF8)) {
					FillEqualityKeyMaterial(keyType: keyType, writer: writer);
					writer.Flush();
					memoryStream.Flush();
					memoryStream.Position = 0L;
					computedHash = hashAlg.ComputeHash(inputStream: memoryStream);
				}
			}
			unchecked {
				return (BitConverter.ToInt64(value: computedHash, startIndex: 0) * 23) ^ (BitConverter.ToInt64(value: computedHash, startIndex: computedHash.Length - 8) * 31);
			}
		}

		protected virtual void FillEqualityKeyMaterial(int keyType, BinaryWriter writer) {
			writer.EnsureNotNull(nameof(writer));
			//
			switch (keyType) {
				case FullEqualityKeyType:
				case PackageNameEqualityKeyType:
				case PackageVersionEqualityKeyType:
					writer.Write($"{PackageNameDataComponentKey}:{PackageName.ArgProp(nameof(PackageName)).EnsureNotNull().Value.Value.ToUpperInvariant()};");
					writer.Write($"{SatellitePackageNameDataComponentKey}:{SatellitePackageName?.Value.ToUpperInvariant() ?? string.Empty};");
					writer.Write($"{PackagePublisherScopeIdDataComponentKey}:{PackagePublisherScopeId.ArgProp(nameof(PackagePublisherScopeId)).EnsureNotNull().Value.StringValue.ToUpperInvariant()};");
					switch (keyType) {
						case PackageVersionEqualityKeyType:
							writer.Write($"{PackageVersionDataComponentKey}:{PackageVersion?.FmtStr().G() ?? string.Empty};");
							break;
						case FullEqualityKeyType:
							writer.Write($"{PackageVersionDataComponentKey}:{PackageVersion?.FmtStr().G() ?? string.Empty};");
							writer.Write($"{PackageLocationBaseUriDataComponentKey}:{PackageLocationBaseUri?.ToString() ?? string.Empty};");
							break;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(paramName: nameof(keyType));
			}
		}

		public override int GetHashCode()
			=> __FullEqualityComparer.GetHashCode(value: this);

		public virtual bool Equals(DescriptionPackageLocator other)
			=> other is null ? false : FullEqualityKey == other.FullEqualityKey;

		public override bool Equals(object obj)
			=> Equals(obj as DescriptionPackageLocator);

		public string Serialize() {
			var deserializationDataString = vlt.Read(ref _deserializationDataString);
			if (deserializationDataString is null) {
				using (var acquiredBuffer = EonStringBuilderUtilities.AcquireBuffer()) {
					var sb = acquiredBuffer.StringBuilder;
					//
					DoSerialize(sb: sb);
					//
					deserializationDataString = sb.ToString();
				}
				//
				return Interlocked.CompareExchange(location1: ref _deserializationDataString, value: deserializationDataString, comparand: null) ?? deserializationDataString;
			}
			else
				return _deserializationDataString;
		}

		protected virtual void DoSerialize(StringBuilder sb) {
			sb.EnsureNotNull(nameof(sb));
			//
			var keyValueDelimiter = DataComponentKeyValueDelimiterString;
			var componentDelimiter = DataComponentDelimiterString;
			var name = PackageName.ArgProp(nameof(PackageName)).EnsureNotNull().Value;
			var publisherScopeId = PackagePublisherScopeId;
			var satelliteName = SatellitePackageName;
			var version = PackageVersion;
			var locationBaseUri = PackageLocationBaseUri;
			sb.Append($"{AltPackageNameDataComponentKey}{keyValueDelimiter}{name.Value}{componentDelimiter}");
			if (!publisherScopeId.IsNullOrUndefined())
				sb.Append($"{AltPackagePublisherScopeIdDataComponentKey}{keyValueDelimiter}{publisherScopeId.StringValue}{componentDelimiter}");
			if (!(satelliteName is null))
				sb.Append($"{AltSatellitePackageNameDataComponentKey}{keyValueDelimiter}{satelliteName.Value}{componentDelimiter}");
			if (!(version is null))
				sb.Append($"{AltPackageVersionDataComponentKey}{keyValueDelimiter}{version.FmtStr().G()}{componentDelimiter}");
			if (!(locationBaseUri is null))
				sb.Append($"{AltPackageLocationBaseUriDataComponentKey}{keyValueDelimiter}{locationBaseUri}{componentDelimiter}");
		}

		public void Deserialize(ArgumentUtilitiesHandle<string> data, ArgumentPlaceholder<bool> useDefaults = default) {
			DoDeserialize(data: data, useDefaults: useDefaults.HasExplicitValue ? useDefaults.ExplicitValue : UseDescriptionLocatorDefaultsOption.Require());
			vlt.Write(location: ref _deserializationDataString, value: data.Value);
		}

		protected virtual void DoDeserialize(ArgumentUtilitiesHandle<string> data, bool useDefaults) {
			var dataComponents =
				data
				.EnsureNotNull()
				.EnsureHasMaxLength(maxLength: DeserializationDataStringMaxLength)
				.EnsureNotEmptyOrWhiteSpace()
				.ParseKeyValuePairs(
					keyComparer: DataComponentKeyComparer,
					keyValueDelimiter: DataComponentKeyValueDelimiter,
					keyValuePairDelimiter: DataComponentDelimiter);
			DoDeserialize(data: dataComponents, useDefaults: useDefaults);
		}

		protected virtual int DeserializationDataStringMaxLength
			=> DefaultDeserializationDataStringMaxLength;

		// TODO: Put strings into the resources.
		//
		protected virtual void DoDeserialize(IDictionary<string, string> data, bool useDefaults) {
			data.EnsureNotNull(nameof(data));
			//
			MetadataName name, satelliteName;
			UriBasedIdentifier publisherScopeId;
			Uri locationBaseUri;
			Version version;
			// Name.
			//
			if (!data.TryGetAnyKeyValue(PackageNameDataComponentKey, AltPackageNameDataComponentKey, out var nameString)) {
				if (useDefaults)
					name = DescriptionLocatorPackageNameOption.Require();
				else
					throw new ArgumentException(paramName: nameof(data), message: $"Отсутствует требуемый компонент '{PackageNameDataComponentKey}' (он же '{AltPackageNameDataComponentKey}').");
			}
			else {
				try {
					name = (MetadataName)nameString;
				}
				catch (Exception exception) {
					throw
						new ArgumentException(
							paramName: nameof(data),
							message: $"Недопустимое значение компонента '{PackageNameDataComponentKey}' (он же '{AltPackageNameDataComponentKey}').{Environment.NewLine}\tЗначение:{nameString.FmtStr().GNLI2()}",
							innerException: exception);
				}
			}
			// PublisherScopeId.
			//
			if (!data.TryGetAnyKeyValue(PackagePublisherScopeIdDataComponentKey, AltPackagePublisherScopeIdDataComponentKey, out var publisherScopeIdString))
				publisherScopeId = useDefaults ? DescriptionLocatorPackagePublisherScopeIdOption.Require() : UriBasedIdentifier.Undefined;
			else {
				try {
					publisherScopeId = new UriBasedIdentifier(publisherScopeIdString.Arg(nameof(publisherScopeIdString)));
				}
				catch (Exception exception) {
					throw
						new ArgumentException(
							paramName: nameof(data),
							message: $"Недопустимое значение компонента '{PackagePublisherScopeIdDataComponentKey}' (он же '{AltPackagePublisherScopeIdDataComponentKey}').{Environment.NewLine}\tЗначение:{publisherScopeIdString.FmtStr().GNLI2()}",
							innerException: exception);
				}
			}
			// SatelliteName.
			//
			if (!data.TryGetAnyKeyValue(SatellitePackageNameDataComponentKey, AltSatellitePackageNameDataComponentKey, out var satelliteNameString))
				satelliteName = null;
			else {
				try {
					satelliteName = (MetadataName)satelliteNameString;
				}
				catch (Exception exception) {
					throw
						new ArgumentException(
							paramName: nameof(data),
							message: $"Недопустимое значение компонента '{SatellitePackageNameDataComponentKey}' (он же '{AltSatellitePackageNameDataComponentKey}').{Environment.NewLine}\tЗначение:{satelliteNameString.FmtStr().GNLI2()}",
							innerException: exception);
				}
			}
			// Version.
			//
			if (!data.TryGetAnyKeyValue(PackageVersionDataComponentKey, AltPackageVersionDataComponentKey, out var versionString))
				version = null;
			else {
				try {
					version = new Version(versionString);
				}
				catch (Exception exception) {
					throw
						new ArgumentException(
							paramName: nameof(data),
							message: $"Недопустимое значение компонента '{PackageVersionDataComponentKey}' (он же '{AltPackageVersionDataComponentKey}').{Environment.NewLine}\tЗначение:{versionString.FmtStr().GNLI2()}",
							innerException: exception);
				}
			}
			// LocationBaseUri.
			//
			if (!data.TryGetAnyKeyValue(PackageLocationBaseUriDataComponentKey, AltPackageLocationBaseUriDataComponentKey, out var locationBaseUriString))
				locationBaseUri = useDefaults ? DescriptionLocatorPackageLocationBaseUriOption.Require() : null;
			else {
				try {
					locationBaseUri = new Uri(uriString: locationBaseUriString, uriKind: UriKind.RelativeOrAbsolute);
				}
				catch (Exception exception) {
					throw
						new ArgumentException(
							paramName: nameof(data),
							message: $"Недопустимое значение компонента '{PackageLocationBaseUriDataComponentKey}' (он же '{AltPackageLocationBaseUriDataComponentKey}').{Environment.NewLine}\tЗначение:{locationBaseUriString.FmtStr().GNLI2()}",
							innerException: exception);
				}
			}
			//
			_packageName = name;
			_packagePublisherScopeId = publisherScopeId;
			_satellitePackageName = satelliteName;
			_packageVersion = version;
			_packageLocationBaseUri = locationBaseUri;
			_packageNameEqualityKey = null;
			_packageVersionEqualityKey = null;
		}

		public sealed override string ToString()
			=> Serialize();

	}

}