using System.Threading.Tasks;

using Eon.Context;
using Eon.Description;
using Eon.Triggers;

namespace Eon {

	public interface IResetServant<out TDescription>
		:IXAppScopeInstance<TDescription>, IActivatableXAppScopeInstance
		where TDescription : class, IResetServantDescription {

		Task<IXAppScopeInstance> ResetComponentAsync(ITriggerSignalProperties triggerSignalProps = default, bool doFailureResponse = default, IContext ctx = default);

	}

}