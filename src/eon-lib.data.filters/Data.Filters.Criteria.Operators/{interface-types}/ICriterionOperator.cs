using System;
using System.Linq.Expressions;

namespace Eon.Data.Filters.Criteria.Operators {

	public interface ICriterionOperator
		:IEquatable<ICriterionOperator>, IAsReadOnly<ICriterionOperator>, IValidatable {

		BinaryExpression BuildExpression(Expression left, Expression right);

	}

}