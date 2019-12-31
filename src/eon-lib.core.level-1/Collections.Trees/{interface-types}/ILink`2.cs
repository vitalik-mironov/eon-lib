
namespace Eon.Collections.Trees {

	/// <summary>
	/// Объект, представляющий немутабельную связь "источник-приемник".
	/// <para>Используется для выражения связи двух элементов в структурах данных.</para>
	/// </summary>
	/// <typeparam name="TSource">Тип "источника".</typeparam>
	/// <typeparam name="TTarget">Тип "приёмника".</typeparam>
	public interface ILink<out TSource, out TTarget> {

		/// <summary>
		/// Возвращает объект — "источник" (или левый элемент) связи.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		TSource Source { get; }

		/// <summary>
		/// Возвращает объект — "приёмник" (или правый элемент) связи.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		TTarget Target { get; }

		/// <summary>
		/// Возвращает объект, ассоциированный с данной связью.
		/// <para>Может быть <see langword="null"/>.</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		object Tag { get; }

	}

}