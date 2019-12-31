using Eon.Description;

namespace Eon.MessageFlow.Local {

	/// <summary>
	/// Определяет спецификацию настроек работы издателя <see cref="ILocalPublisher"/>.
	/// </summary>
	public interface ILocalPublisherSettings
		:ISettings, IAsReadOnly<ILocalPublisherSettings> {

		/// <summary>
		/// Возвращает метрику параллелизма доставки сообщений.
		/// <para>Не может быть менее 1.</para>
		/// </summary>
		/// <returns>Значение <see cref="int"/>.</returns>
		int PostingDop { get; set; }

	}

}