using System;

namespace Eon.Threading {

	/// <summary>
	/// Представляет результат interlocked-операции установки значения (см. <seealso cref="InterlockedUtilities.Update{T}(ref T, Transform2{T})"/> и др.) ссылки <typeparamref name="T"/> в указанной переменной.
	/// </summary>
	/// <typeparam name="T">Тип значения.</typeparam>
	public interface IInterlockedUpdateResult<out T>
		where T : class {

		/// <summary>
		/// Возвращает оригинальную ссылку.
		/// <para>Обращение к свойству вызовет исключение <seealso cref="InvalidOperationException"/>, если объект не является валидным (см. <seealso cref="IsValid"/>).</para>
		/// </summary>
		T Original { get; }

		/// <summary>
		/// Возвращает текущую (после выполнения операции) ссылку.
		/// <para>Обращение к свойству вызовет исключение <seealso cref="InvalidOperationException"/>, если объект не является валидным (см. <seealso cref="IsValid"/>).</para>
		/// </summary>
		T Current { get; }

		/// <summary>
		/// Возвращает признак, указывающий, что ссылка была заменена.
		/// <para>Обращение к свойству вызовет исключение <seealso cref="InvalidOperationException"/>, если объект не является валидным (см. <seealso cref="IsValid"/>).</para>
		/// </summary>
		bool IsUpdated { get; }

		/// <summary>
		/// Возвращает признак, обозначающий, что указанная для замены новая ссылка представляет объект, эквивалентный объекту, на который указывает оригинальная ссылка.
		/// <para>Обращение к свойству вызовет исключение <seealso cref="InvalidOperationException"/>, если объект не является валидным (см. <seealso cref="IsValid"/>).</para>
		/// </summary>
		bool IsProposedSameAsOriginal { get; }

		/// <summary>
		/// Возвращает признак, указывающий на валидность объекта.
		/// </summary>
		bool IsValid { get; }

		/// <summary>
		/// Выполняется проверку данного объекта на предмет его валидности (см. <seealso cref="IsValid"/>.
		/// <para>Метод вызывает исключение <seealso cref="InvalidOperationException"/>, если объект не является валидным.</para>
		/// </summary>
		void EnsureValid();

	}

}