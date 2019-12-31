using System;

namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Представляет базовую спецификацию запроса функциональной зависимости.
	/// </summary>
	public interface IDependencyResolutionSpecs
		:IDisposable {

		/// <summary>
		/// Возвращает тип функциональной зависимости.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		Type DependencyType { get; }

		/// <summary>
		/// Возвращает признак, определяющий поведение при безуспешном разрешении функциональной зависимости.
		/// <para>Значение true определяет обязательное требование разрешения функциональной зависимости.</para>
		/// <para>Если функциональная зависимость не разрешена и данный признак имеет значение true, то процесс разрешения будет завершен исключением.</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		bool EnsureResolution { get; }

		/// <summary>
		/// Возвращает признак, указывающий требование получить новый экземпляр зависимости (см. также <seealso cref="IDependencyHandler2.CanShareDependency"/>).
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		bool IsNewInstanceRequired { get; }

		/// <summary>
		/// Возвращает кортеж аргументов (параметров), которые обязательно должны быть использованы при создании нового экземпляра зависимости, если кортеж указан (т.е. свойство != null).
		/// <para>Если кортеж не указан, то конкретный обработчик функциональных зависимостей (см. <seealso cref="IDependencyHandler2"/>, <seealso cref="IDependencyHandler2"/>) в зависимости от своей реализации может использовать для создания нового экземпляра "свои", используемые по умолчанию аргументы (см. для примера <seealso cref="CtorFactoryDependencyHandler{TArg1, TDependencyInstance}.CtorFactoryDependencyHandler(bool, IArgsTuple{TArg1})"/>.</para>
		/// <para>Свойство является немутабельным.</para>
		/// <para>Может быть null.</para>
		/// </summary>
		IArgsTuple NewInstanceFactoryArgs { get; }

		/// <summary>
		/// Возвращает признак, указывающий запрет на инициализацию нового экземпляра зависимости, если тот реализует интерфейс <seealso cref="IInitializable"/> (или другой специальный интерфейс инициализации). Таким образом, при наличии данного запрета, новый экземпляр будет создан, но не инициализирован (см. <seealso cref="IInitializable.InitializeAsync()"/>).
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		bool PreventNewInstanceInitialization { get; }

		/// <summary>
		/// Возвращает первичную модель разрешения функциональных зависимостей.
		/// <para>Указанная модель (если она задана) будет использована в ходе операции разрешения перед моделью, определенной для области функциональной зависимости контекста разрешения (см. <seealso cref="IDependencyResolutionContext.Scope"/>).</para>
		/// <para>Может быть <see langword="null"/>.</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		IDependencyResolutionModel PrimaryResolutionModel { get; }

		/// <summary>
		/// Возвращает критерий выборки функциональной зависимости.
		/// <para>Критерий выборки используется для согласования результата разрешения зависимости, полученного от одного из исполнителей разрешения (<see cref="IDependencyHandler2"/>), с клиентом зависимости.</para>
		/// <para>Критерий выборки может быть использован, например, когда зависимость должна обладать некоторыми свойствами, известными клиенту.</para>
		/// </summary>
		IDependencyResolutionSelectCriterion SelectCriterion { get; }

		/// <summary>
		/// Возвращает реестр выгружаемых объектов, в который должны помещаться все выгружаемые объекты (реализующие тип <seealso cref="IDisposable"/>), создаваемые в ходе разрешения функциональной зависимости в том числе и сам результат разрешения функциональной зависимости, если он является новым экземпляром.
		/// <para>Может быть null.</para>
		/// </summary>
		IDisposeRegistry DisposeRegistry { get; }

	}

}