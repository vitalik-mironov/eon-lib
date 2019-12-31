namespace Eon.ComponentModel {

	public interface IAsyncEventManager<TEventArgs>
		:IAsyncEventFireManager<TEventArgs>, IAsyncEventSubscribeManager<TEventArgs>
		where TEventArgs : class { }

}