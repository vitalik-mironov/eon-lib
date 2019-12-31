using System;
using System.Collections.Generic;

namespace Eon {

	/// <summary>
	/// Представляет кортеж аргументов нулевой длины.
	/// </summary>
	public interface IArgsTuple {

		/// <summary>
		/// Возвращает длину кортежа.
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		int ArgsCount { get; }

		/// <summary>
		/// Возвращает немутабабельный список типов аргументов.
		/// <para>Свойство является немутабельным.</para>
		/// <para>Не может быть null.</para>
		/// </summary>
		IList<Type> ArgsTypes { get; }

		/// <summary>
		/// Возвращает немутабабельный список конкретных типов аргументов (т.е. типов значений аргументов).
		/// <para>Свойство является немутабельным.</para>
		/// <para>Не может быть null.</para>
		/// </summary>
		IList<Type> ArgsConcreteTypes { get; }

	}

}