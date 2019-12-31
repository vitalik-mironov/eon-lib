using Eon.Diagnostics;

namespace Eon.Metadata {

	/// <summary>
	/// Определяет объекты <see cref="IErrorCode"/> кодов ошибок компонентов пространства имен <see cref="Metadata"/>.
	/// <para>Коды серии 'Oxy-5'.</para>
	/// </summary>
	// TODO: Put strings into the resources.
	//
	public static class MetadataErrorCodes {

		/// <summary>
		/// Oxy-5.0, <seealso cref="SeverityLevel.High"/>.
		/// <para>Сбой операции над метаданными.</para>
		/// </summary>
		public static readonly IErrorCode Fault = new ErrorCode(identifier: "Oxy-5.0", description: "Сбой операции над метаданными.", severityLevel: SeverityLevel.High);

		/// <summary>
		/// Коды серии 'Oxy-5.1'
		/// </summary>
		public static partial class Validation {

			/// <summary>
			/// Oxy-5.1.0, <seealso cref="SeverityLevel.High"/>.
			/// <para>Ошибка в метаданных.</para>
			/// </summary>
			public static readonly IErrorCode NotValid = new ErrorCode(identifier: "Oxy-5.1.0", description: "Ошибка в метаданных.", severityLevel: SeverityLevel.High);

		}

		/// <summary>
		/// Коды серии 'Oxy-5.2'
		/// </summary>
		public static partial class Reference {

			/// <summary>
			/// Oxy-5.2.0, <seealso cref="SeverityLevel.High"/>.
			/// <para>Метаданные не найдены по ссылке.</para>
			/// </summary>
			public static readonly IErrorCode NotResolved = new ErrorCode(identifier: "Oxy-5.2.0", description: "Метаданные не найдены по ссылке.", severityLevel: SeverityLevel.High);

		}


	}

}