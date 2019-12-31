namespace Eon.Diagnostics {

	/// <summary>
	/// Представляет свойство <see cref="VerbosityLevel"/>, определяющее уровень детализации (см. <seealso cref="Diagnostics.VerbosityLevel"/>), ассоциированный с к.л. объектом.
	/// </summary>
	public interface IVerbosityLevelSupport {

		/// <summary>
		/// Возвращает уровень детализации, ассоциированный с данным объектом.
		/// <para>Свойство является немутабельным.</para>
		/// <para>Может быть null.</para>
		/// </summary>
		VerbosityLevel? VerbosityLevel { get; }

	}

}