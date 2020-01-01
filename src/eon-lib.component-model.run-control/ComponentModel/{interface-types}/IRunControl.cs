using System;
using System.Threading;
using System.Threading.Tasks;
using Eon.Context;

namespace Eon.ComponentModel {

	/// <summary>
	/// Представляет управляющий элемент запуска/перезапуска/остановки компонента.
	/// </summary>
	public interface IRunControl
		:IEonDisposable {

		/// <summary>
		/// Опции работы элемента.
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки.</para>
		/// </summary>
		/// <returns>Значение <see cref="RunControlOptions"/>.</returns>
		RunControlOptions Options { get; }

		/// <summary>
		/// Возвращает компонент, запуском/остановкой которого управляет данный элемент.
		/// </summary>
		object Component { get; }

		/// <summary>
		/// Возвращает признак, определяющий наличие запроса остановки компонента <see cref="Component"/>.
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки.</para>
		/// </summary>
		/// <returns>Значение <see cref="bool"/>.</returns>
		bool HasStopRequested { get; }

		/// <summary>
		/// Возвращает признак, указывающий, что выполняется запуск компонента.
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки.</para>
		/// </summary>
		/// <returns>Значение <see cref="bool"/>.</returns>
		bool IsStarting { get; }

		/// <summary>
		/// Возвращает признак, указывающий, что запуск компонента успешно выполнен.
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки.</para>
		/// </summary>
		/// <returns>Значение <see cref="bool"/>.</returns>
		bool IsStarted { get; }

		/// <summary>
		/// Событие, возникающее при поступлении запроса остановки (см. <seealso cref="HasStopRequested"/>).
		/// <para>Событие вызывается асинхронно и единажды для каждого цикла запуск-остановка.</para>
		/// </summary>
		event EventHandler<RunControlEventArgs> StopRequested;

		event EventHandler<RunControlBeforeStartEventArgs> BeforeStart;

		/// <summary>
		/// Occurs immediately after successive start attempt completion.
		/// <para>Warning! This event fires asynchronously, no any exceptions thrown by handlers are handled.</para>
		/// </summary>
		event EventHandler<RunControlAfterStartEventArgs> AfterStart;

		/// <summary>
		/// Intended for start/stop attempt logging purposes. Occurs when any start/stop attempt has completed.
		/// <para>Warning! This event fires asynchronously, no any exceptions thrown by handlers are handled.</para>
		/// </summary>
		event EventHandler<RunControlLogAttemptEventArgs> LogAttempt;

		Task<IRunControlAttemptSuccess> StartAsync(TaskCreationOptions options, IContext ctx = default);

		Task<IRunControlAttemptSuccess> StartAsync(IContext ctx = default);

		Task<IRunControlAttemptSuccess> StopAsync(TaskCreationOptions options, IContext ctx = default, bool finiteStop = default);

		Task<IRunControlAttemptSuccess> StopAsync(IContext ctx = default, bool finiteStop = default);

		Task WaitStartCompletionAsync(TaskCreationOptions options = default, IContext ctx = default);

	}

}