using System.Threading.Tasks;

namespace Eon.ComponentModel {

	public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs eventArgs)
		where TEventArgs : class;

}