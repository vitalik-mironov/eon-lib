using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Eon.Text.RegularExpressions {

	public static class RegexUtilities {

		/// <summary>
		/// Возвращает последовательность всех явно именнованных групп захвата указанного объекта регулярного выражения.
		/// <para>(подробнее см. <seealso cref="Regex.GetGroupNames"/>)</para>
		/// </summary>
		/// <param name="regex">
		/// Объект регулярного выражения.
		/// <para>Не может быть null.</para>
		/// </param>
		/// <returns>
		/// Объект <seealso cref="IEnumerable{T}"/>.
		/// </returns>
		public static IEnumerable<string> GetExplicitGroupNames(this Regex regex)
			=>
			regex
			.EnsureNotNull(nameof(regex))
			.Value
			.GetGroupNames()
			// Данный фильтр нужен для того, чтобы исключить неименованные группы (т.е. те, которые явно в регуляроном выражении не именованы). Подробнее см. справку метода Regex.GetGroupNames().
			//
			.Where(locGroupName => locGroupName.Length > 0 && char.IsLetter(locGroupName[ 0 ]));

	}

}