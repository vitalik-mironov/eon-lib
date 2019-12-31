using System.Runtime.Serialization;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

namespace Eon.Data.Storage {

	[DataContract]
	public class PlainTextStorageConnectionStringSettings
		:AsReadOnlyValidatableBase, IPlainTextStorageConnectionStringSettings, IAsReadOnly<PlainTextStorageConnectionStringSettings> {

		/// <summary>
		/// Value: '4096'.
		/// </summary>
		public static readonly int MaxLengthOfConnectionString = 4096;

		[DataMember(Order = 0, Name = nameof(ConnectionStringText), IsRequired = true)]
		string _connectionStringText;

		[DataMember(Order = 1, Name = nameof(SkipSecretTextSubstitution), IsRequired = true)]
		bool _skipSecretTextSubstitution;

		public PlainTextStorageConnectionStringSettings()
			: this(connectionStringText: default) { }

		public PlainTextStorageConnectionStringSettings(string connectionStringText = default, bool skipSecretTextSubstitution = default, bool isReadOnly = default)
			: base(isReadOnly: isReadOnly) {
			_connectionStringText = connectionStringText;
			_skipSecretTextSubstitution = skipSecretTextSubstitution;
		}

		public PlainTextStorageConnectionStringSettings(IPlainTextStorageConnectionStringSettings other, bool isReadOnly = default)
			: this(
					connectionStringText: other.EnsureNotNull(nameof(other)).Value.ConnectionStringText,
					skipSecretTextSubstitution: other.SkipSecretTextSubstitution,
					isReadOnly: isReadOnly) { }

		[JsonConstructor]
		protected PlainTextStorageConnectionStringSettings(SerializationContext ctx)
			: base(ctx: ctx) { }

		public bool IsDisabled
			=> false;

		public string ConnectionStringText {
			get => _connectionStringText;
			set {
				EnsureNotReadOnly();
				_connectionStringText = value;
			}
		}

		public bool SkipSecretTextSubstitution {
			get => _skipSecretTextSubstitution;
			set {
				EnsureNotReadOnly();
				_skipSecretTextSubstitution = value;
			}
		}

		protected sealed override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase copy) {
			CreateReadOnlyCopy(copy: out var locCopy);
			copy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out PlainTextStorageConnectionStringSettings copy)
			=> copy = new PlainTextStorageConnectionStringSettings(other: this, isReadOnly: true);

		protected override void OnValidate() {
			ConnectionStringText
				.ArgProp(name: nameof(ConnectionStringText))
				.EnsureNotNull()
				.EnsureHasMaxLength(maxLength: MaxLengthOfConnectionString)
				.EnsureNotEmptyOrWhiteSpace();
		}

		public new PlainTextStorageConnectionStringSettings AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				CreateReadOnlyCopy(copy: out var copy);
				return copy;
			}
		}

		IStorageConnectionStringSettings IAsReadOnlyMethod<IStorageConnectionStringSettings>.AsReadOnly()
			=> AsReadOnly();

		IPlainTextStorageConnectionStringSettings IAsReadOnlyMethod<IPlainTextStorageConnectionStringSettings>.AsReadOnly()
			=> AsReadOnly();

		public Task<string> GetConnectionStringRawAsync(IContext ctx = default)
			=> Task.FromResult(result: ConnectionStringText);

	}

}