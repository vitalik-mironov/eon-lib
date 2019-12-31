using System;
using System.Runtime.Serialization;

using Eon.Runtime.Serialization;

namespace Eon.Data.EfCore {

	[DataContract]
	public class DbProviderInfoProps
		:AsReadOnlyValidatableBase, IAsReadOnly<DbProviderInfoProps>, IDbProviderInfoProps {

		[DataMember(Order = 0, Name = nameof(ProviderInvariantName), IsRequired = true)]
		string _providerInvariantName;

		[DataMember(Order = 1, Name = nameof(ProviderManifestToken), IsRequired = true)]
		string _providerManifestToken;

		[DataMember(Order = 2, Name = nameof(DefaultSchemaName), IsRequired = true)]
		string _defaultSchemaName;

		public DbProviderInfoProps()
			: this(isReadOnly: false) { }

		public DbProviderInfoProps(string providerInvarianName = default, string providerManifestToken = default, string defaultSchemaName = default, bool isReadOnly = default)
			: base(isReadOnly: isReadOnly) {
			//
			_providerInvariantName = providerInvarianName;
			_providerManifestToken = providerManifestToken;
			_defaultSchemaName = defaultSchemaName;
		}

		public DbProviderInfoProps(IDbProviderInfoProps other, bool isReadOnly = default)
			: this(providerInvarianName: other.EnsureNotNull(nameof(other)).Value.ProviderInvariantName, providerManifestToken: other.ProviderManifestToken, defaultSchemaName: other.DefaultSchemaName, isReadOnly: isReadOnly) { }

		protected DbProviderInfoProps(SerializationContext ctx)
			: base(ctx: ctx) { }

		public string ProviderInvariantName {
			get => _providerInvariantName;
			set {
				EnsureNotReadOnly();
				_providerInvariantName = value;
			}
		}

		public string ProviderManifestToken {
			get => _providerManifestToken;
			set {
				EnsureNotReadOnly();
				_providerManifestToken = value;
			}
		}

		public string DefaultSchemaName {
			get => _defaultSchemaName;
			set {
				EnsureNotReadOnly();
				_defaultSchemaName = value;
			}
		}

		protected sealed override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locReadOnlyCopy);
			readOnlyCopy = locReadOnlyCopy;
		}

		protected virtual void CreateReadOnlyCopy(out DbProviderInfoProps readOnlyCopy)
			=> readOnlyCopy = new DbProviderInfoProps(other: this, isReadOnly: true);

		public new DbProviderInfoProps AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				CreateReadOnlyCopy(out var readOnlyCopy);
				return readOnlyCopy;
			}
		}

		IDbProviderInfoProps IAsReadOnlyMethod<IDbProviderInfoProps>.AsReadOnly()
			=> AsReadOnly();

		protected override void OnValidate() {
			ProviderInvariantName
				.ArgProp(nameof(ProviderInvariantName))
				.EnsureNotNullOrEmpty();
			ProviderManifestToken
				.ArgProp(nameof(ProviderManifestToken))
				.EnsureNotNull();
			DefaultSchemaName
				.ArgProp(nameof(DefaultSchemaName))
				.EnsureNotNull();
		}

		public override string ToString()
			=>
			$"{GetType().Name}:"
			+ $"{Environment.NewLine}\t{ProviderInvariantName}:{_providerInvariantName.FmtStr().GNLI2()}"
			+ $"{Environment.NewLine}\t{ProviderManifestToken}:{_providerManifestToken.FmtStr().GNLI2()}";

	}

}