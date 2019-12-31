namespace Eon.ComponentModel.Dependencies {

	/// <summary>
	/// Определяет программный интерфейс зафиксированной и неизменной модели разрешения функциональных зависимостей для какой-либо заданной области функциональной зависимости (см. <seealso cref="IDependencyScope"/>).
	/// </summary>
	/// <typeparam name="TScope">Тип области функциональной зависимости (см. <seealso cref="IDependencyScope"/>).</typeparam>
	public interface IDependencyResolutionModel<out TScope>
		:IDependencyResolutionModel
		where TScope : class, IDependencyScope {

		/// <summary>
		/// Возвращает область функциональной зависимости, с которой ассоциирована данная модель.
		/// <para>Не может быть null.</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		TScope Scope { get; }

	}

}