using System.Globalization;
using System.Threading.Tasks;

using Eon.Context;

namespace Eon.Text {

	public interface ITextTemplateVarSubstitution {

		string Substitute(in TextTemplateVar var, CultureInfo culture = default, IContext ctx = default);

		Task<string> SubstituteAsync(TextTemplateVar var, CultureInfo culture = default, IContext ctx = default);

	}

}