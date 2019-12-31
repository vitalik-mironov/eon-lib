using System.Collections.Generic;
using System.Linq.Expressions;

using Eon.Data.Filters.Criteria.Operators;

namespace Eon.Data.Filters.Criteria {

	public interface ICriterion {

		IReadOnlyList<ICriterionOperator> DefinedOperators { get; }

		ICriterionOperator DefaultOperator { get; }

		ICriterionOperand Left { get; set; }

		bool IsLeftReadOnly { get; }

		ICriterionOperator Operator { get; set; }

		ICriterionOperand Right { get; set; }

		bool IsRightReadOnly { get; }

		Expression BuildExpression(Expression input = default);

	}

}