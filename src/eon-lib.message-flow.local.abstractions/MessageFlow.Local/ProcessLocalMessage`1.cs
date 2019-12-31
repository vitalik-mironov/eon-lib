using System.Threading.Tasks;

using Eon.Context;

namespace Eon.MessageFlow.Local {

	public delegate Task ProcessLocalMessage<in TPayload>(ILocalSubscription subscription, ILocalMessage<TPayload> message, IContext ctx = default);

}