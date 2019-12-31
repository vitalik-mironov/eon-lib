namespace Eon.MessageFlow.Local {

	/// <summary>
	/// Определяет функционал и данные сообщения рассылки.
	/// </summary>
	/// <typeparam name="TPayload">Тип данных сообщения.</typeparam>
	public interface ILocalMessage<out TPayload>
		:ILocalMessage {

		/// <summary>
		/// Возвращает данные сообщения.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </summary>
		/// <returns>Объект <typeparamref name="TPayload"/>.</returns>
		TPayload Payload { get; }

	}

}