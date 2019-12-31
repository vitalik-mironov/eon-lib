using Eon.Description;

namespace Eon {

	public abstract class XAppBase<TDescription>
		:XAppBase, IXApp<TDescription>
		where TDescription : class, IXAppDescription {

		TDescription _description;

		private protected XAppBase(IXAppCtorArgs<TDescription> args)
			: base(args: args) {
			//
			_description = args.Description;
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