using System;

namespace Eon.Data.Filters.Criteria.Operators {

	public interface IStringCriterionOperator
		:ICriterionOperator {

		StringComparison? Comparison { get; set; }

		new IStringCriterionOperator AsReadOnly();

	}

}