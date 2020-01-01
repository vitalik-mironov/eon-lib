#region Compilation conditional symbols

#define DO_NOT_USE_EON_LOGGING_API

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;

using Eon.ComponentModel;
using Eon.Description;
using Eon.Diagnostics;
using Eon.MessageFlow.Local;
using Eon.MessageFlow.Local.Description;

using IActivationList = Eon.IActivationList<Eon.Description.IActivationListDescription>;
using IXAppInitializationList = Eon.IXAppInitializationList<Eon.Description.IXAppInitializationListDescription>;

namespace Eon {

	/// <summary>
	/// Компонент <seealso cref="IXAppScopeInstance"/>, представляющий Eon-приложение.
	/// </summary>
	/// <typeparam name="TDescription">Ограничение типа описания (конфигурации) приложения.</typeparam>
	public interface IXApp<out TDescription>
		:IXAppScopeInstance<TDescription>
		where TDescription : class, IXAppDescription {

		/// <summary>
		/// Возвращает признак, указывающий, установлен ли для этого приложения управляющий элемент запуска/выключения XApp-контейнера, которым управляется приложение (см. <seealso cref="ContainerControl"/>).
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		bool HasContainerControl { get; }

		/// <summary>
		/// Возвращает управляющий элемент запуска/выключения XApp-контейнера, которым управляется это приложение.
		/// <para>Обращение к свойству вызовет исключение <seealso cref="InvalidOperationException"/>, если таковой элемент отсутствует (см. <seealso cref="HasContainerControl"/>).</para>
		/// </summary>
		IXAppContainerControl ContainerControl { get; }

		/// <summary>
		/// Возвращает ИД данного экземпляра приложения.
		/// <para>Не может быть <see langword="null"/>, <seealso cref="string.Empty"/>.</para>
		/// <para>Данный идентификатор не является постоянным и для каждого экземпляра приложения уникален.</para>
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки.</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		string AppInstanceId { get; }

		/// <summary>
		/// Gets run control of this app.
		/// <para>Can't be <see langword="null"/>.</para>
		/// </summary>
		IRunControl<IXApp<IXAppDescription>> RunControl { get; }

		/// <summary>
		/// Возвращает компонент издателя/подписки сообщений.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// <para>Обращение к свойству вызовет исключение <seealso cref="InvalidOperationException"/>, если свойство еще не инициализировано.</para>
		/// <para>Свойство является немутабельным.</para>
		/// </summary>
		IXAppLocalPublisher<IXAppLocalPublisherDescription> AppMessageFlow { get; }

		/// <summary>
		/// Возвращает список инициализации приложения — <seealso cref="IXAppInitializationList"/>.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// <para>Обращение к свойству вызовет исключение <seealso cref="InvalidOperationException"/>, если свойство еще не инициализировано.</para>
		/// </summary>
		IXAppInitializationList AppInitializationList { get; }

		/// <summary>
		/// Возвращает список компонентов <seealso cref="IActivationList"/>, активируемых вместе с запуском приложения.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// <para>Обращение к свойству вызовет исключение <seealso cref="InvalidOperationException"/>, если свойство еще не инициализировано.</para>
		/// </summary>
		IActivationList AppStartActivationList { get; }

		/// <summary>
		/// Возвращает признак, указывающий на наличие запроса остановки и выгрузки приложения (см. <seealso cref="ShutdownAppAsync"/>).
		/// </summary>
		bool HasAppShutdownRequested { get; }

		/// <summary>
		/// Возвращает токен отмены, который переходит в сигнальное состоянии при запросе остановки и выгрузки приложения (см. <seealso cref="ShutdownAppAsync"/>).
		/// <para>Обращение к свойству вызовет исключение <seealso cref="InvalidOperationException"/>, если свойство еще не инициализировано.</para>
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки.</para>
		/// </summary>
		CancellationToken AppShutdownToken { get; }

		/// <summary>
		/// Возвращает признак, указывающий, была ли завершена операция остановки и выгрузки приложения (см. <seealso cref="ShutdownAppAsync"/>).
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки.</para>
		/// </summary>
		bool HasAppShutdownFinished { get; }

		IUnhandledExceptionObserver UnhandledExceptionObserver { get; }

		/// <summary>
		/// Событие, возникающее после успешного выполнения операции остановки и выгрузки приложения (см. <seealso cref="ShutdownAppAsync"/>).
		/// </summary>
		event EventHandler AppShutdownFinished;

		/// <summary>
		/// Производит остановку и выгрузку приложения.
		/// <para>Этот метод используется управляющим элементом XApp-контейнера, которым управляется это приложение (см. <seealso cref="ContainerControl"/>).</para>
		/// <para>Поэтому для остановки и выгрузки приложения рекомендуется использовать именно соответствующий API управляющего элемента контейнера. В случае, когда приложение не связано с контейнером (см. <seealso cref="HasContainerControl"/>), для остановки и выгрузки приложения используется этот метод.</para>
		/// </summary>
		/// <returns>Объект задачи <see cref="Task"/>.</returns>
		Task ShutdownAppAsync();

	}

}