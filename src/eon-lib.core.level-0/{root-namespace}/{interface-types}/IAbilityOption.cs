
namespace Eon {

	/// <summary>
	/// Определяет свойство <seealso cref="IsDisabled"/>, декларирующее опцию функционального использования компонента.
	/// </summary>
	public interface IAbilityOption {

		/// <summary>
		/// Возвращает признак, указывающий доступность функционального использования компонента.
		/// </summary>
		bool IsDisabled { get; }

	}

}