using System;

namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Определяет функционал контекста операции разрешения функциональной зависимости.
	/// </summary>
	public interface IDependencyResolutionContext
		:IDisposable {

		/// <summary>
		/// Возвращает последовательный идентификатор, присвоенный данному контексту.
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		long SequentialId { get; }

		/// <summary>
		/// Возвращает область функциональной зависимости, которой принадлежит данный контекст.
		/// <para>Не может быть null.</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		IDependencyScope Scope { get; }

		/// <summary>
		/// Возвращает спецификацию (параметры) разрешения функциональной зависимости для данного контекста.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		IDependencyResolutionSpecs Specs { get; }

		/// <summary>
		/// Выполняет проверку, был ли уже задействован исполнитель разрешения функциональной зависимости <paramref name="handler"/> в ходе данной операции разрешения.
		/// </summary>
		/// <param name="handler">Исполнитель разрешения  функциональной зависимости.</param>
		/// <returns>Значение <see cref="bool"/>.</returns>
		bool HasHandlerInvolved(IDependencyHandler2 handler);

		/// <summary>
		/// Выполняет попытку задействовать исполнителя разрешения функциональной зависимости <paramref name="handler"/> в ходе данной операции разрешения.
		/// <para>Результат попытки зависит от того, был ли ранее указанный исполнитель задействован в ходе данной операции разрешения и сколько раз (во избежание возникновения бесконечного цикла разрешения функциональной зависимости, количество использования конкретного исполнителя может ограничиваться).</para>
		/// <para>Метод не предназначен для прямого вызова из кода. Метод используется компонентами, координирующими процесс разрешения функциональной зависимости.</para>
		/// </summary>
		/// <param name="handler">Исполнитель разрешения функциональной зависимости.</param>
		/// <param name="referrer">
		/// Исполнитель разрешения функциональной зависимости, который передает разрешение зависимости указанному исполнителю <paramref name="handler"/> (см. <seealso cref="DependencyResult"/>).
		/// </param>
		/// <returns>
		/// Значение <see cref="bool"/>.
		/// <para>True — исполнитель задействован.</para>
		/// <para>False — исполнитель не задействован. Либо исполнитель ранее уже был задействован, либо количество раз, когда исполнитель был задействован, достигло макс. допустимого значения, либо другая причина, в зависимости от реализации данного интерфейса.</para>
		/// </returns>
		bool TryInvolveHandler(IDependencyHandler2 handler, IDependencyHandler2 referrer = default);

		/// <summary>
		/// Проверяет соответствие экземляра функциональной зависимости <paramref name="instance"/> критерию выбора, определенного для данной операции разрешения функциональной зависимости.
		/// </summary>
		/// <param name="instance">Экземляр функциональной зависимости.</param>
		/// <returns>Значение <see cref="bool"/>.</returns>
		bool IsMatchSelectCriterion(object instance);

		/// <summary>
		/// Возвращает последовательность уже задействованных исполнителей разрешения функциональной зависимости в ходе данной операции разрешения.
		/// </summary>
		/// <returns>Массив объектов <see cref="IDependencyHandler2"/>.</returns>
		IDependencyHandler2[ ] GetInvolvedHandlerChain();

		int ExecutionsCountLimitPerExecutor { get; }

		bool IsAdvancedLoggingEnabled { get; }

	}

}