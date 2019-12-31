using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

using Eon.Collections;
using Eon.Metadata;
using Eon.Runtime.Options;
using Eon.Runtime.Serialization;
using Eon.Xml.Schema;

using Newtonsoft.Json;

namespace Eon.Description {

	/// <summary>
	/// Description locator (reference).
	/// </summary>
	[DataContract(Namespace = EonXmlNamespaces.Description.Package)]
	[DebuggerDisplay("{ToString(),nq}")]
	public class DescriptionLocator
		:DescriptionPackageLocator, IEquatable<DescriptionLocator> {

		#region Nested types

		sealed class P_FullEqualityComparer
			:IEqualityComparer<DescriptionLocator> {

			internal P_FullEqualityComparer() { }

			public bool Equals(DescriptionLocator x, DescriptionLocator y)
				=> ReferenceEquals(x, y) ? true : (x is null || y is null ? false : x.FullEqualityKey == y.FullEqualityKey);

			public int GetHashCode(DescriptionLocator value) {
				value.EnsureNotNull(nameof(value));
				//
				value.FullEqualityKey.Split(out var keyLeft, out var keyRight);
				unchecked {
					return (keyLeft * 29) ^ (keyRight * 13);
				}
			}

		}

		#endregion

		#region Static members

		/// <summary>
		/// Value: 'description-path'.
		/// </summary>
		public static readonly string DescriptionPathDataComponentKey = "description-path";

		/// <summary>
		/// Value: 'd'.
		/// </summary>
		public static readonly string AltDescriptionPathDataComponentKey = "d";

		static readonly P_FullEqualityComparer __FullEqualityComparer;

		public new static readonly IEqualityComparer<DescriptionLocator> DefaultEqualityComparer;

		static DescriptionLocator() {
			__FullEqualityComparer = new P_FullEqualityComparer();
			DefaultEqualityComparer = __FullEqualityComparer;
		}

		public static bool Equals(DescriptionLocator a, DescriptionLocator b)
			=> ReferenceEquals(a, b) ? true : (a is null ? false : a.Equals(b));

		public static void Parse(string data, out DescriptionLocator locator, ArgumentPlaceholder<bool> useDefaults = default)
			=> Parse(data: data.Arg(nameof(data)), locator: out locator, useDefaults: useDefaults);

		// TODO: Put strings into the resources.
		//
		public static void Parse(ArgumentUtilitiesHandle<string> data, out DescriptionLocator locator, ArgumentPlaceholder<bool> useDefaults = default) {
			data.EnsureNotNull();
			//
			var locLocator = new DescriptionLocator(ctx: default);
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

		MetadataPathName _descriptionPath;

		[JsonConstructor]
		protected DescriptionLocator(SerializationContext ctx)
			: base(ctx: ctx) { }

		public DescriptionLocator(
			MetadataPathName descriptionPath,
			MetadataName packageName,
			UriBasedIdentifier packagePublisherScopeId,
			MetadataName satellitePackageName = default,
			Uri packageLocationBaseUri = default,
			Version packageVersion = default,
			ArgumentPlaceholder<bool> useDefaults = default)
			: base(
					packageName: packageName,
					packagePublisherScopeId: packagePublisherScopeId,
					satellitePackageName: satellitePackageName,
					packageLocationBaseUri: packageLocationBaseUri,
					packageVersion: packageVersion,
					useDefaults: useDefaults) {
			//
			if (useDefaults.HasExplicitValue ? useDefaults.ExplicitValue : UseDescriptionLocatorDefaultsOption.Require())
				descriptionPath = descriptionPath ?? DescriptionLocatorDescriptionPathOption.Require();
			else
				descriptionPath.EnsureNotNull(nameof(descriptionPath));
			//
			_descriptionPath = descriptionPath;
		}

		public DescriptionLocator(
			DescriptionLocator other,
			ArgumentPlaceholder<MetadataPathName> descriptionPath = default,
			ArgumentPlaceholder<MetadataName> packageName = default,
			ArgumentPlaceholder<UriBasedIdentifier> packagePublisherScopeId = default,
			ArgumentPlaceholder<MetadataName> satellitePackageName = default,
			ArgumentPlaceholder<Uri> packageLocationBaseUri = default,
			ArgumentPlaceholder<Version> packageVersion = default)
			: this(
					descriptionPath: descriptionPath.Substitute(other.EnsureNotNull(nameof(other)).Value.DescriptionPath),
					packageName: packageName.Substitute(other.PackageName),
					packagePublisherScopeId: packagePublisherScopeId.Substitute(other.PackagePublisherScopeId),
					satellitePackageName: satellitePackageName.Substitute(other.SatellitePackageName),
					packageLocationBaseUri: packageLocationBaseUri.Substitute(other.PackageLocationBaseUri),
					packageVersion: packageVersion.Substitute(other.PackageVersion)) { }

		public DescriptionLocator(
			DescriptionPackageLocator other,
			MetadataPathName descriptionPath,
			ArgumentPlaceholder<MetadataName> packageName = default,
			ArgumentPlaceholder<UriBasedIdentifier> packagePublisherScopeId = default,
			ArgumentPlaceholder<MetadataName> satellitePackageName = default,
			ArgumentPlaceholder<Uri> packageLocationBaseUri = default,
			ArgumentPlaceholder<Version> packageVersion = default)
			: this(
					descriptionPath: descriptionPath,
					packageName: packageName.Substitute(other.EnsureNotNull(nameof(other)).Value.PackageName),
					packagePublisherScopeId: packagePublisherScopeId.Substitute(other.PackagePublisherScopeId),
					satellitePackageName: satellitePackageName.Substitute(other.SatellitePackageName),
					packageLocationBaseUri: packageLocationBaseUri.Substitute(other.PackageLocationBaseUri),
					packageVersion: packageVersion.Substitute(other.PackageVersion)) { }


		public MetadataPathName DescriptionPath
			=> _descriptionPath;

		protected override void DoSerialize(StringBuilder sb) {
			base.DoSerialize(sb: sb);
			//
			var descriptionPath = DescriptionPath.ArgProp(nameof(DescriptionPath)).EnsureNotNull().Value;
			sb.Append($"{AltDescriptionPathDataComponentKey}{DataComponentKeyValueDelimiterString}{descriptionPath.Value}{DataComponentDelimiterString}");
		}

		protected override void DoDeserialize(IDictionary<string, string> data, bool useDefaults) {
			base.DoDeserialize(data: data, useDefaults: useDefaults);
			//
			MetadataPathName descriptionPath;
			//
			// Name.
			//
			if (!data.TryGetAnyKeyValue(DescriptionPathDataComponentKey, AltDescriptionPathDataComponentKey, out var descriptionPathString)) {
				if (useDefaults)
					descriptionPath = DescriptionLocatorDescriptionPathOption.Require();
				else
					throw new ArgumentException(paramName: nameof(data), message: $"Required data component '{DescriptionPathDataComponentKey}' ('{AltDescriptionPathDataComponentKey}') is missing.");
			}
			else {
				try {
					descriptionPath = (MetadataPathName)descriptionPathString;
				}
				catch (Exception exception) {
					throw new ArgumentException(paramName: nameof(data), message: $"Value of data component '{DescriptionPathDataComponentKey}' is not valid.{Environment.NewLine}\tValue:{descriptionPathString.FmtStr().GNLI2()}", innerException: exception);
				}
			}
			//
			_descriptionPath = descriptionPath;
		}

		protected override void FillEqualityKeyMaterial(int keyType, BinaryWriter writer) {
			base.FillEqualityKeyMaterial(keyType, writer);
			//
			if (keyType == FullEqualityKeyType)
				writer.Write($"{DescriptionPathDataComponentKey}:{DescriptionPath.ArgProp(nameof(DescriptionPath)).EnsureNotNull().Value.Value};");
		}

		public virtual bool Equals(DescriptionLocator other)
			=> ReferenceEquals(other, null) ? false : FullEqualityKey == other.FullEqualityKey;

		public sealed override bool Equals(DescriptionPackageLocator other)
			=> Equals(other: other as DescriptionLocator);

		public override bool Equals(object other)
			=> Equals(other: other as DescriptionLocator);

		public override int GetHashCode()
			=> __FullEqualityComparer.GetHashCode(value: this);

		public static bool operator ==(DescriptionLocator a, DescriptionLocator b)
			=> Equals(a: a, b: b);

		public static bool operator !=(DescriptionLocator a, DescriptionLocator b)
			=> !Equals(a: a, b: b);

	}

}