using System;

using Eon.Collections;
using Eon.ComponentModel.Dependencies;

namespace Eon.Data.TypeSystem {

	/// <summary>
	/// Определяет спецификацию информации о системе типов.
	/// </summary>
	public interface IDataTypeSystemInfo
		:IDependencySupport {

		/// <summary>
		/// Возвращает уникальный идентификатор данной системы типов.
		/// </summary>
		/// <returns>Значение <see cref="Guid"/>.</returns>
		Guid UniqueIdentifier { get; }

		/// <summary>
		/// Возвращает набор всех типов, входящих в данную систему типов.
		/// </summary>
		/// <returns>Последовательность <see cref="ICountableEnumerable{T}"/>.</returns>
		ICountableEnumerable<Type> AllTypes { get; }

	}

}