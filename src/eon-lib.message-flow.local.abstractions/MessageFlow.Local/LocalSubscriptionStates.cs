using System;

namespace Eon.MessageFlow.Local {

	/// <summary>
	/// Описывают состояния подписки <see cref="ILocalSubscription"/>.
	/// </summary>
	[Flags]
	public enum LocalSubscriptionStates
		:int {

		/// <summary>
		/// Значение по умолчанию. Ничего не означает.
		/// </summary>
		None = 0,

		/// <summary>
		/// Подписка активирована.
		/// </summary>
		Activated = 0x1,

		/// <summary>
		/// Подписка деактивирована.
		/// </summary>
		Deactivated = 0x2,

		/// <summary>
		/// Объект издателя владеет объектом подписки, что в том числе означает выгрузку объекта подписки при выгрузке объекта издателя.
		/// </summary>
		PublisherOwnsSubscription = 0x4,

		/// <summary>
		/// Действие подписки приостановлено.
		/// </summary>
		Suspended = 0x8,

		/// <summary>
		/// Объект подписки владеет объектом подписчика, что в том числе означает выгрузку объекта подписчика при выгрузке объекта подписки.
		/// </summary>
		OwnsSubscriber = 0x10

	}


}