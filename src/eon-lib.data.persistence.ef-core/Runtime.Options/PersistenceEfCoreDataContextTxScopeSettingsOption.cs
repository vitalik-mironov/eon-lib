using Eon.Data.Persistence.EfCore;

namespace Eon.Runtime.Options {

	public sealed class PersistenceEfCoreDataContextTxScopeSettingsOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value:
		/// <para><see cref="PersistenceEfCoreDataContextTxScopeSettings.RollbackThroughDispose"/>: <see langword="false"/>.</para>
		/// </summary>
		public static readonly PersistenceEfCoreDataContextTxScopeSettingsOption Fallback;

		static PersistenceEfCoreDataContextTxScopeSettingsOption() {
			Fallback = new PersistenceEfCoreDataContextTxScopeSettingsOption(settings: new PersistenceEfCoreDataContextTxScopeSettings(rollbackThroughDispose: false, isReadOnly: true));
			RuntimeOptions.Option<PersistenceEfCoreDataContextTxScopeSettingsOption>.SetFallback(option: Fallback);
		}

		public static PersistenceEfCoreDataContextTxScopeSettings Require()
			=> RuntimeOptions.Option<PersistenceEfCoreDataContextTxScopeSettingsOption>.Require().Settings;

		#endregion

		public PersistenceEfCoreDataContextTxScopeSettingsOption(PersistenceEfCoreDataContextTxScopeSettings settings) {
			settings.EnsureNotNull(nameof(settings)).EnsureReadOnly().EnsureNotDisabled().EnsureValid();
			//
			Settings = settings;
		}

		public PersistenceEfCoreDataContextTxScopeSettings Settings { get; }

	}

}