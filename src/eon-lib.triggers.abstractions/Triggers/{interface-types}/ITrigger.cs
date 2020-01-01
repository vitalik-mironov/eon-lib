using System;
using System.Threading.Tasks;

using Eon.ComponentModel;

namespace Eon.Triggers {

	/// <summary>
	/// Базовый функционал триггера.
	/// </summary>
	public interface ITrigger
		:IDisposable, IAbilityOption {

		Task<TriggerSignalEventArgs> NextSignalAwaitable();

		/// <summary>
		/// Выполняет однократный перевод триггера в сигнальное состояние.
		/// <para>Если на момент вызова метода триггер отключен (см. <see cref="IAbilityOption.IsDisabled"/>) или для триггера поступил запрос выгрузки (см. <see cref="IEonDisposable.IsDisposeRequested"/>), то перевод триггера в сигнальное состояние не выполняется.</para>
		/// </summary>
		/// <param name="signalProps">
		/// Свойства сигнала: штамп времени, ИД корреляции и др.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		bool Signal(ITriggerSignalProperties signalProps);

		/// <summary>
		/// Возвращает элемент управления активации/деактивации триггера.
		/// </summary>
		/// <returns>Объект <see cref="IRunControl{TComponent}"/>.</returns>
		IRunControl<ITrigger> ActivateControl { get; }

	}

}