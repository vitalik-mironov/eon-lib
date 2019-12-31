
namespace Eon {

	/// <summary>
	/// Defines specs of an instance supporting read-only copy make.
	/// </summary>
	/// <typeparam name="T">Type.</typeparam>
	public interface IAsReadOnly<out T>
		:IIsReadOnlyProperty, IAsReadOnlyMethod<T>
		where T : IAsReadOnly<T> { }

}