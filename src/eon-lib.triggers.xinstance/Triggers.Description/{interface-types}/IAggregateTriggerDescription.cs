using System.Collections.Generic;

namespace Eon.Triggers.Description {

	/// <summary>
	/// Описание агрегированного триггера.
	/// </summary>
	public interface IAggregateTriggerDescription
		:ITriggerDescription {

		/// <summary>
		/// Возвращает набор тригеров.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </summary>
		IEnumerable<ITriggerDescription> Triggers { get; set; }

	}

}