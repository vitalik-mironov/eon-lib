namespace Eon.Data {

	/// <summary>
	/// Контекст данных (v2).
	/// </summary>
	public interface IDataContext2
		:IDataContext {

		/// <summary>
		/// Возвращает поставщика контекста данных, которым был создан данный контекст и, соответственно, с ним ассоциирован.
		/// </summary>
		IDataContextProvider<IDataContext2> Provider { get; }

		bool PreferNoTracking { get; }

	}

}
