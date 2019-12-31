using System;
using Eon.Metadata;

namespace Eon.Triggers.Description {

	/// <summary>
	/// Определяет описание (конфигурацию) триггера, переход в сигнальное состояние которого совершается периодически (см. <see cref="Period"/>).
	/// </summary>
	public interface IPeriodTriggerDescription
		:ITriggerDescription {

		/// <summary>
		/// Возвращает/устанавливает периодичность перехода триггера в сигнальное состояние.
		/// <para>В валидном состоянии (<see cref="IMetadata.IsValidated"/>):</para>
		/// <para>• Не может быть менее, чем <see cref="TriggerUtilities.PeriodMin"/>;</para>
		/// <para>• Не может быть более, чем <see cref="TriggerUtilities.PeriodMax"/>.</para>
		/// </summary>
		TimeSpan Period { get; set; }

		TimeSpan PeriodVariance { get; set; }

	}

}