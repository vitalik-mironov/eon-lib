using Eon.Diagnostics;

namespace Eon.ComponentModel.Dependencies {
	using static ErrorCode;

	/// <summary>
	/// Defines the codes <see cref="IErrorCode"/> for dependency errors.
	/// <para>'<see cref="DefaultIdentifierPrefix"/>-3' series codes.</para>
	/// </summary>
	// TODO: Put strings into the resources.
	//
	public static class DependencyErrorCodes {

		/// <summary>
		/// <see cref="DefaultIdentifierPrefix"/>-3.0, <seealso cref="SeverityLevel.High"/>.
		/// <para>Сбой работы с функциональными зависимостями.</para>
		/// </summary>
		public static readonly IErrorCode Fault =
			new ErrorCode(
				identifier: $"{DefaultIdentifierPrefix}-3.0",
				severityLevel: SeverityLevel.High,
				description: "Сбой работы с функциональными зависимостями.");

		/// <summary>
		/// <see cref="DefaultIdentifierPrefix"/>-3.1.0, <seealso cref="SeverityLevel.High"/>.
		/// <para>Требуемая функциональная зависимость не найдена.</para>
		/// </summary>
		public static readonly IErrorCode Resolution_NotResolved =
			new ErrorCode(
				identifier: $"{DefaultIdentifierPrefix}-3.1.0",
				severityLevel: SeverityLevel.High,
				description: "Требуемая функциональная зависимость не найдена.");

	}

}