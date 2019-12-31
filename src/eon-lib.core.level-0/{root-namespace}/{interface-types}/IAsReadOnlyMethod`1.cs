
namespace Eon {

	/// <summary>
	/// Defines a special method returning the read-only copy of the instance the type of which implements this method.
	/// </summary>
	/// <typeparam name="T">Тип.</typeparam>
	public interface IAsReadOnlyMethod<out T> {

		/// <summary>
		/// Gets read-only copy of this instance.
		/// <para>If this instance is read-only already then method just returns this instance.</para>
		/// </summary>
		T AsReadOnly();

	}

}