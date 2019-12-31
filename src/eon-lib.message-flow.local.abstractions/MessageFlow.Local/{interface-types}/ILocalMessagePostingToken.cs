namespace Eon.MessageFlow.Local {

	/// <summary>
	/// Представляет токен доставки сообщения <see cref="ILocalMessage"/>.
	/// </summary>
	public interface ILocalMessagePostingToken {

		/// <summary>
		/// Возвращает доставляемое сообщение.
		/// </summary>
		/// <returns>Объект <see cref="ILocalMessage"/>.</returns>
		/// <remarks>Не может быть null.</remarks>
		ILocalMessage Message { get; }

		/// <summary>
		/// Возвращает количество подписок <see cref="ILocalSubscription"/>, по которым выполняется доставка сообщения.
		/// </summary>
		/// <returns>Значение <see cref="int"/>.</returns>
		int PostingCount { get; }

		/// <summary>
		/// Возвращает признак, определяющий необходимость выгрузки сообщения после доставки всем получателям.
		/// </summary>
		/// <returns>Значение <see cref="bool"/>.</returns>
		bool DisposeMessageAtEnd { get; }

	}

}