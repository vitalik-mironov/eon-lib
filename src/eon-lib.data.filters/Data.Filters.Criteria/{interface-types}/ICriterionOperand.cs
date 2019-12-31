using System;
using System.Linq.Expressions;

namespace Eon.Data.Filters.Criteria {

	public interface ICriterionOperand {

		Type TypeConstraint { get; }

		Expression BuildExpression(Expression input = default);

	}

}