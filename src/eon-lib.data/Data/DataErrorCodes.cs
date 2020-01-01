using Eon.Diagnostics;

namespace Eon.Data {
	using static ErrorCode;

	/// <summary>
	/// Defines the codes <see cref="IErrorCode"/> for data errors.
	/// <para>'Eon-2' serires codes.</para>
	/// </summary>
	// TODO: Put strings into the resources.
	//
	public static partial class DataErrorCodes {

		/// <summary>
		/// '<see cref="DefaultIdentifierPrefix"/>-2.0' series codes.
		/// </summary>
		public static class Entity {

			/// <summary>
			/// <see cref="DefaultIdentifierPrefix"/>-2.0.0, <seealso cref="SeverityLevel.Lowest"/>.
			/// <para>Ресурс или сущность данных не найдены.</para>
			/// </summary>
			public static readonly IErrorCode NotFound = new ErrorCode(identifier: $"{DefaultIdentifierPrefix}-2.0.0", severityLevel: SeverityLevel.Lowest, description: "Ресурс или сущность данных не найдены.");

			/// <summary>
			/// <see cref="DefaultIdentifierPrefix"/>-2.0.1, <seealso cref="SeverityLevel.Lowest"/>.
			/// <para>Данные (объект) были изменены или удалёны. Необходимо выполнить повторное считывание данных.</para>
			/// </summary>
			public static readonly IErrorCode OptimisticConcurrencyLoss = new ErrorCode(identifier: $"{DefaultIdentifierPrefix}-2.0.1", severityLevel: SeverityLevel.Lowest, description: "Данные (объект) были изменены или удалёны. Необходимо выполнить повторное считывание данных.");

		}

	}

}