using System;
using System.Threading.Tasks;

using Eon.Context;

namespace Eon.MessageFlow.Local {

	/// <summary>
	/// Представляет подписчика сообщений <seealso cref="ILocalMessage"/>.
	/// </summary>
	public interface ILocalSubscriber
		:IDisposeNotifying {

		/// <summary>
		/// Выполняет проверку на наличие у данного подписчика активной подписки (см. <seealso cref="ILocalSubscription.IsActive"/>).
		/// <para>Если активной подписки нет (подписка не активирована еще, деактивирована или вообще не установлена), то метод вызывает исключение <seealso cref="InvalidOperationException"/>.</para>
		/// <para>В общем случае подписка устанавливается методом <seealso cref="OnSubscriptionActivationAsync(ILocalSubscription, IContext)"/>.</para>
		/// </summary>
		void EnsureHasActiveSubscription();

		/// <summary>
		/// Уведомляет подписчика об активации подписки.
		/// </summary>
		/// <param name="subscription">
		/// Подписка.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <param name="ctx">
		/// Контекст выполнения операции.
		/// </param>
		/// <returns>Объект задачи <seealso cref="Task"/>.</returns>
		Task OnSubscriptionActivationAsync(ILocalSubscription subscription, IContext ctx = default);

		/// <summary>
		/// Уведомляет подписчика о деактивации подписки <see cref="ILocalSubscription"/>.
		/// </summary>
		/// <param name="subscription">
		/// Подписка.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="ctx">
		/// Контекст выполнения операции.
		/// </param>
		/// <returns>Объект задачи <seealso cref="Task"/>.</returns>
		Task OnSubscriptionDeactivationAsync(ILocalSubscription subscription, IContext ctx = default);

		/// <summary>
		/// Выполняет асинхронную обработку сообщения, полученного по подписке (см. <paramref name="subscription"/>).
		/// </summary>
		/// <param name="subscription">
		/// Подписка, по которой получено сообщение.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="message">
		/// Сообщение.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </param>
		/// <param name="ctx">
		/// Контекст выполнения операции.
		/// </param>
		/// <returns>Объкт задачи <see cref="Task"/>.</returns>
		Task ProcessMessagePostAsync(ILocalSubscription subscription, ILocalMessage message, IContext ctx = default);

	}

}