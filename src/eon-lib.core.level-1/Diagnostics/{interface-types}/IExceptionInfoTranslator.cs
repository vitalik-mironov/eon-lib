using System;

namespace Eon.Diagnostics {

	/// <summary>
	/// Представляет транслятор <seealso cref="ExceptionInfo"/> из объекта <seealso cref="Exception"/>.
	/// <para>Для регистрации трансляторов см. <seealso cref="ExceptionInfoUtilities.RegisterTranslator(Type, IExceptionInfoTranslator)"/> и <seealso cref="ExceptionInfoUtilities.UnregisterTranslator(Type, IExceptionInfoTranslator)"/>.</para>
	/// </summary>
	public interface IExceptionInfoTranslator {

		/// <summary>
		/// Выполняет парсинг (конструирование) объекта <seealso cref="ExceptionInfo"/> из указанного объекта исключения <paramref name="exception"/>.
		/// </summary>
		/// <param name="exception">
		/// Объект исключения, из которого выполняется парсинг объекта информации об исключении <seealso cref="ExceptionInfo"/>.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <returns>Объект <seealso cref="ExceptionInfo"/>.</returns>
		ExceptionInfo FromException(Exception exception);

	}

}