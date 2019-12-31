using System;

using Eon.ComponentModel.Dependencies;
using Eon.Description;

namespace Eon {

	public abstract class XInstanceBase<TDescription>
		:XInstanceBase
		where TDescription : class, IDescription {

		TDescription _description;

		protected XInstanceBase(IXInstance scope, TDescription description)
			: base(scope: scope, description: description) {
			_description = description;
		}

		protected XInstanceBase(IServiceProvider outerServiceProvider = default, IDependencySupport outerDependencies = default)
			: base(outerServiceProvider: outerServiceProvider, outerDependencies: outerDependencies) {
			_description = null;
		}

		protected XInstanceBase(TDescription description, IServiceProvider outerServiceProvider = default, IDependencySupport outerDependencies = default)
			: base(description: description, outerServiceProvider: outerServiceProvider, outerDependencies: outerDependencies) {
			_description = description;
		}

		// TODO: Put strings into the resources.
		//
		public new TDescription Description {
			// См. также XInstanceBase.Description.
			//
			get {
				if (HasDescription)
					return ReadDA(ref _description);
				else
					throw new EonException(message: $"Component have not a settings.{Environment.NewLine}\tComponent:{this.FmtStr().GNLI2()}");
			}
		}

		protected override void Dispose(bool explicitDispose) {
			_description = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}