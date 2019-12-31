namespace Eon {

	/// <summary>
	/// Представляет область (компонент), для которой может устанавливаться состояние доступности редактирования объектов, покрываемых этой областью (подчиняющихся этому компоненту).
	/// </summary>
	public interface IReadOnlyScope
		:IIsReadOnlyProperty {

		/// <summary>
		/// Возвращает признак, указывающий, имеет ли данная область (компонент) устойчивое (т.е. не может быть изменено) состояние недоступности редактирования (см. <seealso cref="ReadOnlyState"/>, <seealso cref="ReadOnlyStateTag.IsPermanentReadOnly"/>).
		/// </summary>
		bool IsPermanentReadOnly { get; }

		/// <summary>
		/// Возвращает состояние доступности редактирования для данной области.
		/// <para>Не может быть null.</para>
		/// </summary>
		ReadOnlyStateTag ReadOnlyState { get; }

		/// <summary>
		/// Возвращает внешнюю область доступности редактирования.
		/// <para>Может быть null.</para>
		/// </summary>
		IReadOnlyScope OuterReadOnlyScope { get; }

		/// <summary>
		/// Устанавливает состояние доступности редактирования для данной области.
		/// </summary>
		/// <param name="isReadOnly">Признак, указывющий на доступность/недоступность редактирования.</param>
		/// <param name="isPermanent">Признак, указывающий на немутабельность (перманентность) устанавливаемого состояния доступности редактирования.</param>
		void SetReadOnly(bool isReadOnly, bool isPermanent);

		/// <summary>
		/// Устанавливает состояние доступности редактирования для данной области.
		/// <para>Необходимо учитывать, что указанное состояние доступности редактирования устанавливается данным методом как мутабельное (т.е. не перманентное).</para>
		/// </summary>
		/// <param name="isReadOnly">Признак, указывющий на доступность/недоступность редактирования.</param>
		void SetReadOnly(bool isReadOnly);

	}

}