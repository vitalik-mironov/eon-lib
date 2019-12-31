namespace Eon.Diagnostics {

	/// <summary>
	/// Представляет спецификацию идентификационной информации об ошибке.
	/// </summary>
	public interface IErrorCode {

		/// <summary>
		/// Возвращает степень серьезности, связянной с ошибкой.
		/// </summary>
		SeverityLevel? SeverityLevel { get; }

		/// <summary>
		/// Возвращает идентификатор ошибки.
		/// <para>Не может быть null, <see cref="string.Empty"/> или строкой, содержащей только пробельные символы.</para>
		/// </summary>
		string Identifier { get; }

		/// <summary>
		/// Возвращает краткое описание ошибки.
		/// <para>Может быть null.</para>
		/// </summary>
		string Description { get; }

		IErrorCode ChangeSeverityLevel(SeverityLevel? severityLevel);

		IErrorCode ChangeDescription(string description);

	}

}