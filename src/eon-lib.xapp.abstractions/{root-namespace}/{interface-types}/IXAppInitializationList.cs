using System;
using System.Collections.Generic;
using Eon.Description;

namespace Eon {

	/// <summary>
	/// Представляет список инициализации Eon-приложения (см. <seealso cref="IXApp{TDescription}"/>) — список компонентов, которые должны быть созданы и инициализированы при инициализации приложения.
	/// </summary>
	/// <typeparam name="TDescription">Ограничение типа описания (конфигурации) данного компонента.</typeparam>
	public interface IXAppInitializationList<out TDescription>
		:IXAppScopeInstance<TDescription>
		where TDescription : class, IXAppInitializationListDescription {

		/// <summary>
		/// Возвращает признак функционального использования данного списка.
		/// <para>Данное свойство является немутабельным.</para>
		/// </summary>
		bool IsDisabled { get; }

		/// <summary>
		/// Возвращает набор компонентов, которые были созданы и инициализированы данным компонентом (списком) при инициализации приложения.
		/// <para>Состав набора определяется также свойством <see cref="IsDisabled"/>. Если список отключен, то данный набор будет пуст. При этом указанный признак не влияет на инициализацию самого данного списка.</para>
		/// <para>Если обращение к свойству производится до завершения инициализации данного компонента, то вызывается исключение <see cref="InvalidOperationException"/>.</para>
		/// </summary>
		IEnumerable<IXInstance> InitializedItems { get; }

	}

}