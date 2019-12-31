namespace Eon.Data.Persistence {

	/// <summary>
	/// Представляет контекст постоянного хранения данных.
	/// </summary>
	public interface IPersistenceDataContext<out TContextDiscriminator>
		:IPersistenceDataContext {

		/// <summary>
		/// Возвращает поставщика контекста данных, которым был создан данный контекст и которому он (контекст данных) принадлежит.
		/// <para>Не может быть <see langword="null"/>.</para>
		/// </summary>
		new IPersistenceDataContextProvider<IPersistenceDataContext> Provider { get; }
		
	}

}