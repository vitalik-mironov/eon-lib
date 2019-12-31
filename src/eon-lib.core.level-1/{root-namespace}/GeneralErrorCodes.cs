using Eon.Diagnostics;

using static Eon.Diagnostics.ErrorCode;

namespace Eon {

	/// <summary>
	/// Defines the codes (<see cref="IErrorCode"/>) for general errors.
	/// <para>'<see cref="DefaultIdentifierPrefix"/>-0' series codes.</para>
	/// </summary>
	// TODO: Put strings into the resources.
	//
	public static partial class GeneralErrorCodes {

		/// <summary>
		/// '<see cref="DefaultIdentifierPrefix"/>-0.0' series codes.
		/// </summary>
		public static partial class Operation {

			/// <summary>
			/// <see cref="DefaultIdentifierPrefix"/>-0.0.0, <see cref="SeverityLevel.Lowest"/>.
			/// <para>Операция не реализована.</para>
			/// </summary>
			public static readonly IErrorCode NotImplemented =
				new ErrorCode(
					identifier: $"{DefaultIdentifierPrefix}-0.0.0",
					description: "Операция не реализована.",
					severityLevel: SeverityLevel.Lowest);

			/// <summary>
			/// <see cref="DefaultIdentifierPrefix"/>-0.0.1, <see cref="SeverityLevel.Lowest"/>.
			/// <para>Операция не поддерживается.</para>
			/// </summary>
			public static readonly IErrorCode NotSupported =
				new ErrorCode(
					identifier: $"{DefaultIdentifierPrefix}-0.0.1",
					description: "Операция не поддерживается.",
					severityLevel: SeverityLevel.Lowest);

			// ...

			/// <summary>
			/// '<see cref="DefaultIdentifierPrefix"/>-0.10' series codes.
			/// </summary>
			public static partial class Params {

				/// <summary>
				/// <see cref="DefaultIdentifierPrefix"/>-0.10.0, <see cref="SeverityLevel.Lowest"/>.
				/// <para>Невозможно выполнить операцию. Недостаточно параметров или параметры заданы неверно.</para>
				/// </summary>
				public static readonly IErrorCode Illegal =
					new ErrorCode(
						identifier: $"{DefaultIdentifierPrefix}-0.10.0",
						description: "Невозможно выполнить операцию. Недостаточно параметров или параметры заданы неверно.",
						severityLevel: SeverityLevel.Lowest);

			}

		}

	}

}