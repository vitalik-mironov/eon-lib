using Eon.Diagnostics;

namespace Eon {
	using static ErrorCode;

	/// <summary>
	/// Defines the codes <see cref="IErrorCode"/> for dispose state errors.
	/// <para>'<see cref="DefaultIdentifierPrefix"/>-1' series codes.</para>
	/// </summary>
	// TODO: Put strings into the resources.
	//
	public static class DisposableErrorCodes {

		/// <summary>
		/// <see cref="DefaultIdentifierPrefix"/>-1.0.0, <seealso cref="SeverityLevel.Medium"/>.
		/// <para>Операция невозможна, так как объект выгружается или уже выгружен.</para>
		/// </summary>
		public static readonly IErrorCode Object_DisposingOrDisposed =
			new ErrorCode(
				identifier: $"{DefaultIdentifierPrefix}-1.0.0",
				description: "Операция невозможна, так как объект выгружается или уже выгружен.",
				severityLevel: SeverityLevel.Medium);

		/// <summary>
		/// <see cref="DefaultIdentifierPrefix"/>-1.0.1, <seealso cref="SeverityLevel.Medium"/>.
		/// <para>Операция невозможна, так как для объекта ранее поступил запрос выгрузки.</para>
		/// </summary>
		public static readonly IErrorCode Object_DisposeRequested =
			new ErrorCode(
				identifier: $"{DefaultIdentifierPrefix}-1.0.1",
				description: "Операция невозможна, так как для объекта ранее поступил запрос выгрузки.",
				severityLevel: SeverityLevel.Medium);

	}

}