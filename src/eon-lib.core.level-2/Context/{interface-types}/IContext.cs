using System;

namespace Eon.Context {

	/// <summary>
	/// Контекст какой-либо операции.
	/// <para>См. <see cref="ContextUtilities"/>.</para>
	/// </summary>
	public interface IContext
		:IDisposeNotifying {

		/// <summary>
		/// Возвращает признак, указывающий на наличие у данного контекста окружающего контекста (см. <seealso cref="OuterContext"/>).
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки (см. <see cref="IDisposable"/>).</para>
		/// </summary>
		bool HasOuterContext { get; }

		/// <summary>
		/// Возвращает окружающий контекст.
		/// <para>Обращение к данному свойству при отсутствии окружающего контекста (см. <seealso cref="HasOuterContext"/>) вызовет исключение <seealso cref="InvalidOperationException"/>.</para>
		/// </summary>
		IContext OuterContext { get; }

		/// <summary>
		/// Возвращает ИД корреляции данного контекста.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки (см. <see cref="IDisposable"/>).</para>
		/// </summary>
		XCorrelationId CorrelationId { get; }

		/// <summary>
		/// Возвращает полный ИД корреляции для данного контекста.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки (см. <see cref="IDisposable"/>).</para>
		/// </summary>
		XFullCorrelationId FullCorrelationId { get; }

		/// <summary>
		/// Возвращает тэг (к.л. пользовательский объект), ассоциированный только с данным контекстом.
		/// <para>Следует учитывать, что значение данного свойства не "наследуется" от окружающего контекста (см. <see cref="OuterContext"/>).</para>
		/// <para>Может быть <see langword="null"/>.</para>
		/// </summary>
		object LocalTag { get; }

	}

}