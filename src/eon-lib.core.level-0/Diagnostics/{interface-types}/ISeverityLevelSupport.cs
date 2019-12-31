namespace Eon.Diagnostics {

	/// <summary>
	/// Представляет свойство <see cref="SeverityLevel"/>, определяющее степень серьёзности (см. <seealso cref="Diagnostics.SeverityLevel"/>), ассоциированную с к.л. объектом.
	/// </summary>
	public interface ISeverityLevelSupport {

		/// <summary>
		/// Возвращает степень серьёзности, ассоциированную с данным объектом.
		/// <para>Свойство является немутабельным.</para>
		/// <para>Может быть null.</para>
		/// </summary>
		SeverityLevel? SeverityLevel { get; }

	}

}