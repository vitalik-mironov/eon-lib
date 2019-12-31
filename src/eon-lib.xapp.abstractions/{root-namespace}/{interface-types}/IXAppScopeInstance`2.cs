using Eon.Description;

namespace Eon {

	public interface IXAppScopeInstance<out TDescription, out TApp>
		:IXAppScopeInstance<TDescription>
		where TDescription : class, IDescription
		where TApp : class, IXApp<IXAppDescription> {

		new TApp App { get; }

	}

}