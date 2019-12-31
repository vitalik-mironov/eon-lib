using Eon.ComponentModel;

namespace Eon {

	public interface IActivatableXAppScopeInstance<out TXAppScopeInstance>
		:IActivatableXAppScopeInstance
		where TXAppScopeInstance : class, IXAppScopeInstance {

		new IRunControl<TXAppScopeInstance> ActivateControl { get; }

	}

}