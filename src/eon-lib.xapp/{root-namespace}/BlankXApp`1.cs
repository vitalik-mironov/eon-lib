using Eon.Description;

namespace Eon {

	public class BlankXApp<TDescription>
		:CustomXAppBase<TDescription>
		where TDescription : class, ICustomXAppDescription {

		public BlankXApp(IXAppCtorArgs<TDescription> args)
			: base(args: args) { }

	}

}