namespace Eon {

	public delegate void EventHandler<in TSender, in TEventArgs>(TSender sender, TEventArgs e)
		where TSender : class
		where TEventArgs : class;

}