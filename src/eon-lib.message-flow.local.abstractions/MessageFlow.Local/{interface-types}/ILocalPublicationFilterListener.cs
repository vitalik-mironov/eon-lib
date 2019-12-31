namespace Eon.MessageFlow.Local {

	/// <summary>
	/// Определяет метод обработчика события предварительной обработки издания сообщения <see cref="ILocalMessage"/>.
	/// <para>Предварительная обработка издания сообщения позволяет отменить отправку сообщения к.л. подписчику или выполнить к.л. другие операции.</para>
	/// <para>Отмена отправки сообщения на этапе предварительной обработки позволяет скорректировать работу издателя <see cref="ILocalPublisher"/> в части бесполезной работы. Например, когда заранее известно, что отправляемое сообщение подписчику не нужно, то и нет смысла ставить сообщение в очередь отправки данному отправителю.</para>
	/// </summary>
	public interface ILocalPublicationFilterListener {

		/// <summary>
		/// Обрабатывает событие предварительной обработки издания сообщения <see cref="ILocalMessage"/>.
		/// <para>Данный метод позволяет отменить отправку сообщения для к.л. подписчика путем установки свойства <see cref="OnceCanceledEventArgs.Cancel"/> аргументов события <paramref name="e"/>.</para>
		/// </summary>
		/// <param name="sender">Источник события. Как правило, это издатель <see cref="ILocalPublisher"/>.</param>
		/// <param name="e">Аргменты события <see cref="LocalPublicationFilterEventArgs"/>.</param>
		void PublicationFilter(object sender, LocalPublicationFilterEventArgs e);

	}

}