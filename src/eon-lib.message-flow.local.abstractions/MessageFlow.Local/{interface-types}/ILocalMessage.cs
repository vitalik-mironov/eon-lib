using System;

namespace Eon.MessageFlow.Local {

	/// <summary>
	/// Представляет сообщение.
	/// </summary>
	public interface ILocalMessage
		:IOxyDisposable {

		/// <summary>
		/// Возвращает штамп-времени создания экземпляра сообщения в локальной среде.
		/// <para>Обращение к свойству не подвержено влияюнию состояния выгрузки объекта.</para>
		/// </summary>
		DateTimeOffset LocalCreationTimestamp { get; }

	}

}