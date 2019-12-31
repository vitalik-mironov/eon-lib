using System.Globalization;

namespace Eon.Globalization {

	/// <summary>
	/// Определяет функциональность локализуемого значения <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">Тип значения.</typeparam>
	public interface ILocalizable<out T>
		where T : class {

		/// <summary>
		/// Возвращает значение, характерное для культуры <see cref="CultureInfo"/> текущего потока.
		/// </summary>
		/// <returns>Значение <typeparamref name="T"/>.</returns>
		T Value { get; }

		/// <summary>
		/// Возвращает значение, характерное для указанной культуры <see cref="CultureInfo"/>.
		/// </summary>
		/// <param name="culture">Культура. Может быть <see langword="null"/>. В случае, когда значение есть <see langword="null"/>, будет использоваться культура текущего потока.</param>
		/// <returns>Значение <typeparamref name="T"/>.</returns>
		T GetForCulture(CultureInfo culture);

	}

}