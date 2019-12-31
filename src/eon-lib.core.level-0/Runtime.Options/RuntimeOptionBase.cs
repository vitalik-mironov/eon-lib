namespace Eon.Runtime.Options {

	public abstract class RuntimeOptionBase {

		readonly RuntimeOptions.Identity _identity;

		protected RuntimeOptionBase() {
			_identity = RuntimeOptions.OptionApiInvoker.Create(optionType: GetType()).Identity;
		}

		public RuntimeOptions.Identity Identity
			=> _identity;

	}

}