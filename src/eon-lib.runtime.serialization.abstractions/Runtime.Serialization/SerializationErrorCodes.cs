using Eon.Diagnostics;

namespace Eon.Runtime.Serialization {

	/// <summary>
	/// Определяет объекты <see cref="IErrorCode"/> кодов ошибок компонентов пространства имен <see cref="Eon.Runtime.Serialization"/>.
	/// <para>Коды серии 'Eon-4'.</para>
	/// </summary>
	// TODO: Put strings into the resources.
	//
	public static class SerializationErrorCodes {

		/// <summary>
		/// Коды серии 'Eon-4.0'
		/// </summary>
		public static partial class Serialization {

			/// <summary>
			/// Eon-4.0.0, <see cref="SeverityLevel.Medium"/>.
			/// <para>Ошибка сериализации объекта (данных).</para>
			/// </summary>
			public static readonly IErrorCode Fault =
				new ErrorCode(
					identifier: "Eon-4.0.0",
					description: "Ошибка сериализации объекта (данных).",
					severityLevel: SeverityLevel.Medium);

		}

		/// <summary>
		/// Коды серии 'Eon-4.1'
		/// </summary>
		public static partial class Deserialization {

			/// <summary>
			/// Eon-4.1.0, <see cref="SeverityLevel.Medium"/>.
			/// <para>Ошибка десериализации объекта (данных).</para>
			/// </summary>
			public static readonly IErrorCode Fault =
				new ErrorCode(
					identifier: "Eon-4.1.0",
					description: "Ошибка десериализации объекта (данных).",
					severityLevel: SeverityLevel.Medium);

		}

	}

}