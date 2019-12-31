using System;
using Eon.Metadata;

namespace Eon.Triggers.Description {

	/// <summary>
	/// Описание агрегированного триггера с возможностью конфигурации ограничения частоты выходного сигнала (см. <see cref="SignalFrequencyLimit"/>).
	/// </summary>
	public interface IFrequencyLimitTriggerDescription
		:IAggregateTriggerDescription {

		/// <summary>
		/// Возвращает/устанавливает минимально допустимый интервал между выходными сигналами триггера.
		/// <para>В валидном состоянии (см. <see cref="IMetadata.IsValidated"/>):</para>
		/// <para>• Не может быть менее <see cref="TimeSpan.Zero"/>.</para>
		/// </summary>
		TimeSpan SignalFrequencyLimit { get; set; }

	}

}