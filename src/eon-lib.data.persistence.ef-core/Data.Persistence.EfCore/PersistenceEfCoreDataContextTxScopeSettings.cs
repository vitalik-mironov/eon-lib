using System;
using System.Runtime.Serialization;

using Eon.Description;
using Eon.Runtime.Options;
using Eon.Runtime.Serialization;

using Microsoft.EntityFrameworkCore.Storage;

using Newtonsoft.Json;

namespace Eon.Data.Persistence.EfCore {

	[DataContract]
	public class PersistenceEfCoreDataContextTxScopeSettings
		:AsReadOnlyValidatableBase, IAsReadOnly<PersistenceEfCoreDataContextTxScopeSettings>, ISettings {

		#region Static & constant members

		/// <summary>
		/// Gets the current default settings set by <see cref="PersistenceEfCoreDataContextTxScopeSettingsOption"/> (see <see cref="RuntimeOptions"/>).
		/// </summary>
		public static PersistenceEfCoreDataContextTxScopeSettings Default
			=> PersistenceEfCoreDataContextTxScopeSettingsOption.Require();

		#endregion

		[DataMember(Order = 0, Name = nameof(RollbackThroughDispose), IsRequired = true, EmitDefaultValue = true)]
		bool _rollbackThroughDispose;

		public PersistenceEfCoreDataContextTxScopeSettings()
			: this(rollbackThroughDispose: default) { }

		public PersistenceEfCoreDataContextTxScopeSettings(bool rollbackThroughDispose = default, bool isReadOnly = default)
			: base(isReadOnly: isReadOnly) {
			//
			_rollbackThroughDispose = rollbackThroughDispose;
		}

		public PersistenceEfCoreDataContextTxScopeSettings(PersistenceEfCoreDataContextTxScopeSettings other, bool isReadOnly = false)
			: this(rollbackThroughDispose: other.EnsureNotNull(nameof(other)).Value.RollbackThroughDispose, isReadOnly: isReadOnly) { }

		[JsonConstructor]
		protected PersistenceEfCoreDataContextTxScopeSettings(SerializationContext ctx)
			: base(ctx: ctx) { }

		bool IAbilityOption.IsDisabled
			=> false;

		/// <summary>
		/// Gets or sets whether invoke <see cref="IDbContextTransaction.Rollback"/> to rollback the transaction or just rely on implementation of <see cref="IDisposable.Dispose"/> method for <see cref="IDbContextTransaction"/>.
		/// </summary>
		public bool RollbackThroughDispose {
			get => _rollbackThroughDispose;
			set {
				EnsureNotReadOnly();
				_rollbackThroughDispose = value;
			}
		}

		protected sealed override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase copy) {
			CreateReadOnlyCopy(out var locCopy);
			copy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out PersistenceEfCoreDataContextTxScopeSettings copy)
			=> copy = new PersistenceEfCoreDataContextTxScopeSettings(other: this, isReadOnly: true);

		public new PersistenceEfCoreDataContextTxScopeSettings AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				CreateReadOnlyCopy(out var сopy);
				return сopy;
			}
		}

		protected override void OnValidate() { }

	}

}