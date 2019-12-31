
using Eon.Description;

namespace Eon {
	using IXApp = IXApp<IXAppDescription>;

	public abstract class XAppScopeInstanceBase<TDescription>
		:XAppScopeInstanceBase
		where TDescription : class, IDescription {

		TDescription _description;

		protected XAppScopeInstanceBase(IXApp app, TDescription description)
			: base(app: app, description: description) { }

		protected XAppScopeInstanceBase(IXAppScopeInstance scope, TDescription description)
			: base(scope: scope, description: description) {
			_description = description;
		}

		public new TDescription Description
			=> ReadDA(ref _description);

		protected override void Dispose(bool explicitDispose) {
			_description = null;
			//
			base.Dispose(explicitDispose);
		}

	}

}