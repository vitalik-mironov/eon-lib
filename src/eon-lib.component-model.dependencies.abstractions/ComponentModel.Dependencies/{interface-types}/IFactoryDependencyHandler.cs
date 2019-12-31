using System;

namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Представляет обработчик функциональной зависимости, получающий новый экземпляр зависимости посредством какого-либо фабричного компонента (метода, конструктора, класса и т.д.).
	/// </summary>
	public interface IFactoryDependencyHandler
		:IDependencyHandler2 {

		/// <summary>
		/// Возвращает признак, указывающий, что данный обработчик имеет аргументы (параметры, см. <seealso cref="FactoryArgs"/>), с которыми должен быть вызван фабричный компонент, создающий новый экземпляр зависимости.
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		bool HasFactoryArgs { get; }

		/// <summary>
		/// Возвращает аргументы (параметры, см. <seealso cref="FactoryArgs"/>), с которыми должен быть вызван фабричный компонент, создающий новый экземпляр зависимости.
		/// <para>При обращении к данному свойству будет вызвано исключение <seealso cref="InvalidOperationException"/>, если аргументы отсутствуют (см. <seealso cref="HasFactoryArgs"/>).</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		IArgsTuple FactoryArgs { get; }

	}

}