namespace Eon.ComponentModel {

	/// <summary>
	/// Определяет базовый функционал управляющего элемента запуска/перезапуска/остановки компонента <typeparamref name="TComponent"/>.
	/// </summary>
	/// <typeparam name="TComponent">Тип компонента.</typeparam>
	public interface IRunControl<out TComponent>
		:IRunControl {

		/// <summary>
		/// Возвращает компонент, которым управляет данный элемент.
		/// </summary>
		new TComponent Component { get; }

	}

}