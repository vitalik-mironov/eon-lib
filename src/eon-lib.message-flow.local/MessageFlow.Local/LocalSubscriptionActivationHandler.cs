using System.Threading.Tasks;

using Eon.Context;

namespace Eon.MessageFlow.Local {

	public delegate Task LocalSubscriptionActivationHandler(ILocalSubscription subscription, IContext ctx = default);

}