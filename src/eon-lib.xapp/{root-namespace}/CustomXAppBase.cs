using Eon.Description;

namespace Eon {

	public abstract class CustomXAppBase<TDescription>
		:XAppBase<TDescription>
		where TDescription : class, ICustomXAppDescription {

		protected CustomXAppBase(IXAppCtorArgs<TDescription> args)
			: base(args: args) { }

	}

}