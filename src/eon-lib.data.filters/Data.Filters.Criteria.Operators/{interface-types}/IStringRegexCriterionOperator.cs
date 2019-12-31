using System.Text.RegularExpressions;

namespace Eon.Data.Filters.Criteria.Operators {

	public interface IStringRegexCriterionOperator
		:IStringCriterionOperator {

		RegexOptions? RegexOptions { get; set; }

		new IStringRegexCriterionOperator AsReadOnly();

	}

}