
namespace Eon.Diagnostics {

	/// <summary>
	/// Определяет свойство, представляющее информацию о внутреннем стеке исключений в виде <see cref="ExceptionInfo"/>.
 /// <para>Используется при конструировании объекта <see cref="ExceptionInfo"/> для получения информации о внутреннем стеке исключения от объектов <see cref="System.Exception"/>, реализующих данный интерфейс.</para>
	/// </summary>
	public interface IInnerExceptionInfoProvider {

		/// <summary>
		/// Возвращает объект <see cref="ExceptionInfo"/>, представляющий сведения о внутреннем стеке исключения.
		/// <para>Не может быть null.</para>
		/// </summary>
		ExceptionInfo InnerExceptionInfo { get; }

	}

}