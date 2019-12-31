using System;

namespace Eon.Diagnostics {

	/// <summary>
	/// Определяет опции форматирования текстового представления объекта сведений об исключении.
	/// </summary>
	[Flags]
	public enum ExceptionInfoFormattingOptions {

		/// <summary>
		/// Соответствует <seealso cref="IncludeMessage"/> | <seealso cref="IncludeNumberingMarker"/>.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Выводить текст сообщения исключения.
		/// </summary>
		IncludeMessage = 1,

		/// <summary>
		/// Выводить тип исключения.
		/// </summary>
		IncludeType = 2,

		/// <summary>
		/// Выводить трассировку стека исключения.
		/// </summary>
		IncludeStackTrace = 4,

		/// <summary>
		/// Выводить идентификатор кода ошибки (<see cref="IErrorCode.Identifier"/>), назначенного исключению.
		/// </summary>
		IncludeErrorCodeIdentifier = 8,

		/// <summary>
		/// Выводить описание кода ошибки (<see cref="IErrorCode.Description"/>), назначенного исключению.
		/// </summary>
		IncludeErrorCodeDescription = 16,

		/// <summary>
		/// Выводить маркер нумерации.
		/// </summary>
		IncludeNumberingMarker = 32,

		IncludeErrorCode = IncludeErrorCodeIdentifier | IncludeErrorCodeDescription,

		/// <summary>
		/// Не выводить сведения о внутренних исключениях.
		/// </summary>
		ExcludeInnerExceptions = 64,

		/// <summary>
		/// Представляет комбинацию всех опций кроме <see cref="ExcludeInnerExceptions"/>.
		/// </summary>
		Full = IncludeMessage | IncludeErrorCode | IncludeNumberingMarker | IncludeStackTrace | IncludeType,

		FullWithoutStackTrace = IncludeMessage | IncludeType | IncludeNumberingMarker | IncludeErrorCode

	}

}