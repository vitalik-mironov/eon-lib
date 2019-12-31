using System;

namespace Eon.ComponentModel {

	public interface IAsyncEventSubscribeManager<TEventArgs>
		:IDisposable
		where TEventArgs : class {

		IDisposable Subscribe(AsyncEventHandler<TEventArgs> asyncSubscriber);

		IDisposable Subscribe(EventHandler2<TEventArgs> subscriber);

		IDisposable Subscribe(IAsyncEventSubscriber<TEventArgs> subscriber);

		IDisposable Subscribe(IAsyncEventSubscriber<TEventArgs> subscriber, bool ownsSubscriber);

	}

}