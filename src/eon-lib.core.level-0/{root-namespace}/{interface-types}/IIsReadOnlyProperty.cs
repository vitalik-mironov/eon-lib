
namespace Eon {

	/// <summary>
	/// Определяет свойство <see cref="IsReadOnly"/>, указывающее, находится ли объект в состоянии только для чтения.
	/// </summary>
	public interface IIsReadOnlyProperty {

		/// <summary>
		/// Возвращает признак, указывающий, находится ли данный объект в состоянии только для чтения.
		/// </summary>
		bool IsReadOnly { get; }

	}

}