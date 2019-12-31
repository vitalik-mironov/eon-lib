namespace Eon.ComponentModel {

	public delegate void EventHandler2<in TEventArgs>(object sender, TEventArgs e)
		where TEventArgs : class;

}