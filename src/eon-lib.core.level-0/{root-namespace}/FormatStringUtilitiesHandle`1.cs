namespace Eon {

	/// <summary>
	/// Представляет дескриптор утилит 'FormatStringUtilities'.
	/// </summary>
	/// <typeparam name="T">Тип объекта.</typeparam>
	public readonly struct FormatStringUtilitiesHandle<T> {

		/// <summary>
		/// Возвращает объект, текстовое предсталение которого требуется получить.S
		/// </summary>
		public readonly T Object;

		public FormatStringUtilitiesHandle(T obj) {
			Object = obj;
		}

		public FormatStringUtilitiesHandle<T> AsChanged(T obj)
			=> new FormatStringUtilitiesHandle<T>(obj: obj);

		public override string ToString()
			=> Object?.ToString() ?? FormatStringUtilitiesCoreL0.GetNullValueText();

	}

}