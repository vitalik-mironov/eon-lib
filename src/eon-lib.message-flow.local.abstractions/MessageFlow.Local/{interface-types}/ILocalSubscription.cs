using System;
using System.Threading.Tasks;

namespace Eon.MessageFlow.Local {

	/// <summary>
	/// Подписка на получение подписчиком <see cref="ILocalSubscriber"/> сообщений от издателя (публикатора) <see cref="ILocalPublisher"/>.
	/// </summary>
	public interface ILocalSubscription
		:IDisposable, IEquatable<ILocalSubscription> {

		/// <summary>
		/// Возвращает текущее состояние подписки.
		/// <para>Состояние подписки представляется комбинацией флагов <see cref="LocalSubscriptionStates"/>.</para>
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки.</para>
		/// </summary>
		/// <returns>Значение <see cref="LocalSubscriptionStates"/>.</returns>
		LocalSubscriptionStates State { get; }

		/// <summary>
		/// Возвращает признак активности подписки.
		/// <para>В общем случае активность подписки определяется её состоянием <seealso cref="State"/> и состоянием выгрузки (см. <seealso cref="IOxyDisposable.Disposing"/>, <seealso cref="IOxyDisposable.IsDisposed"/>).</para>
		/// <para>Обращение к свойству не подвержено влиянию состояния выгрузки.</para>
		/// </summary>
		bool IsActive { get; }

		/// <summary>
		/// Выполняет активацию подписки.
		/// <para>Подписка может быть активирована однократно. После активации к состоянию подписки <see cref="State"/> добавляется флаг <see cref="LocalSubscriptionStates.Activated"/>.</para>
		/// </summary>
		Task ActivateAsync(TaskCreationOptions options = default);

		/// <summary>
		/// Выполняет деактивацию подписки.
		/// <para>Подписка может быть деактивирована однократно, однако, повторный вызов данного метода считается допустимым.</para>
		/// <para>После деактивации к состоянию подписки <see cref="State"/> добавляется флаг <see cref="LocalSubscriptionStates.Deactivated"/>.</para>
		/// <para>Обращение к методу не подвержено влиянию состояния выгрузки.</para>
		/// </summary>
		Task DeactivateAsync();

		/// <summary>
		/// Возвращает объект издателя <see cref="ILocalPublisher"/>, который будет поставлять сообщения согласно данной подписке.
		/// <para>Не может быть null.</para>
		/// </summary>
		/// <returns>Объект <see cref="ILocalPublisher"/>.</returns>
		ILocalPublisher Publisher { get; }

		/// <summary>
		/// Возвращает объект подписчика (см. <see cref="ILocalSubscriber"/>), который будет получать и обрабатывать сообщения согласно данной подписке.
		/// <para>Не может быть null.</para>
		/// </summary>
		/// <returns>Объект подписчика <see cref="ILocalSubscriber"/>.</returns>
		ILocalSubscriber Subscriber { get; }

		/// <summary>
		/// Выполняет предобработку издания сообщения <see cref="ILocalMessage"/>.
		/// </summary>
		/// <param name="state">
		/// Объект состояния предобработки издания сообщения (см. <seealso cref="ILocalPublicationFilterState"/>).
		/// <para>Не может быть null.</para>
		/// </param>
		/// <returns>Объект <see cref="Task{T}"/>.</returns>
		Task<LocalPublicationFilterResult> PublicationFilterAsync(ILocalPublicationFilterState state);

		/// <summary>
		/// Событие предобработки издания сообщения <see cref="ILocalMessage"/>.
		/// </summary>
		event EventHandler<LocalPublicationFilterEventArgs> PublicationFilter;

		/// <summary>
		/// Переводит подписку в состояние, когда действие подписки приостановлено.
		/// </summary>
		/// <returns>Объект <see cref="Task"/>.</returns>
		Task SuspendAsync();

		/// <summary>
		/// Выводит подписку из состояния, когда действие подписки приостановлено.
		/// <para>Если подписка на момент обращения к методу деактивирована, то будет вызвано исключение <see cref="InvalidOperationException"/>.</para>
		/// </summary>
		/// <returns>Объект <see cref="Task"/>.</returns>
		Task ResumeAsync();

		/// <summary>
		/// Выводит подписку из состояния, когда действие подписки приостановлено.
		/// <para>В отличие от операции <see cref="ResumeAsync"/>, операция не вызывает исключения, если подписка ранее была деактивирована.</para>
		/// </summary>
		/// <returns>Объект <see cref="Task{TResult}"/>.</returns>
		Task<bool> TryResumeAsync();

	}

}