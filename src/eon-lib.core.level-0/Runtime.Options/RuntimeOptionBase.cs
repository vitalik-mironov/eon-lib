namespace Eon.Runtime.Options {

	public abstract class RuntimeOptionBase {
		
		protected RuntimeOptionBase() {
			Identity = RuntimeOptions.OptionApiInvoker.Create(optionType: GetType()).Identity;
		}

		public RuntimeOptions.Identity Identity { get; }

	}

}