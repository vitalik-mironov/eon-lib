namespace Eon.Runtime.Options {

	public sealed class UseDescriptionLocatorDefaultsOption
		:RuntimeOptionBase {

		#region Static & constant members

		/// <summary>
		/// Value: '<see langword="true"/>'.
		/// </summary>
		public static readonly UseDescriptionLocatorDefaultsOption Fallback;

		static UseDescriptionLocatorDefaultsOption() {
			Fallback = new UseDescriptionLocatorDefaultsOption(use: true);
			RuntimeOptions.Option<UseDescriptionLocatorDefaultsOption>.SetFallback(option: Fallback);
		}

		public static bool Require()
			=> RuntimeOptions.Option<UseDescriptionLocatorDefaultsOption>.Require().Use;

		#endregion

		readonly bool _use;

		public UseDescriptionLocatorDefaultsOption(bool use) {
			_use = use;
		}

		public bool Use
			=> _use;

	}

}