using System.Globalization;

namespace Eon.Globalization {

	public interface ICultureInfoProvider {

		CultureInfo GetCulture(string cultureName);

	}

}