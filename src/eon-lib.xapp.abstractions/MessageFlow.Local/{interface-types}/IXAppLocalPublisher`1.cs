using Eon.MessageFlow.Local.Description;

namespace Eon.MessageFlow.Local {

	public interface IXAppLocalPublisher<out TDescription>
		:IXAppScopeInstance<TDescription>, ILocalPublisher
		where TDescription : class, IXAppLocalPublisherDescription { }

}