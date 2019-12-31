using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;

using Eon.Globalization;
using Eon.Runtime.Serialization;

namespace Eon.Data.Filters.Criteria.Operators {

	[DataContract]
	public abstract class CriterionOperatorBase
		:AsReadOnlyValidatableBase, IEquatable<CriterionOperatorBase>, ICriterionOperator {

		#region Static members

		public static readonly Equals EqualsOperator = new Equals(isReadOnly: true);

		public static readonly NotEquals NotEquals = new NotEquals(isReadOnly: true);

		public static readonly GreaterThan GreaterThan = new GreaterThan(isReadOnly: true);

		public static readonly LessThan LessThan = new LessThan(isReadOnly: true);

		public static readonly GreaterThanOrEquals GreaterThanOrEquals = new GreaterThanOrEquals(isReadOnly: true);

		public static readonly LessThanOrEquals LessThanOrEquals = new LessThanOrEquals(isReadOnly: true);

		public static readonly StringNotMatchRegex StringNotMatchRegex = new StringNotMatchRegex(isReadOnly: true, comparison: StringCriterionOperatorBase.DefaultComparison, regexOptions: StringMatchRegex.DefaultRegexOptions);

		public static readonly StringMatchRegex StringMatchRegex = new StringMatchRegex(isReadOnly: true, comparison: StringCriterionOperatorBase.DefaultComparison, regexOptions: StringMatchRegex.DefaultRegexOptions);

		public static readonly StringNotContains StringNotContains = new StringNotContains(isReadOnly: true, comparison: StringCriterionOperatorBase.DefaultComparison);

		public static readonly StringContains StringContains = new StringContains(isReadOnly: true, comparison: StringCriterionOperatorBase.DefaultComparison);

		public static readonly StringStartsWith StringStartsWith = new StringStartsWith(isReadOnly: true, comparison: StringCriterionOperatorBase.DefaultComparison);

		public static readonly StringEndsWith StringEndsWith = new StringEndsWith(isReadOnly: true, comparison: StringCriterionOperatorBase.DefaultComparison);

		public static readonly StringEquals StringEquals = new StringEquals(isReadOnly: true, comparison: StringCriterionOperatorBase.DefaultComparison);

		public static readonly StringNotEquals StringNotEquals = new StringNotEquals(isReadOnly: true, comparison: StringCriterionOperatorBase.DefaultComparison);

		#endregion

		protected CriterionOperatorBase(bool isReadOnly = default)
			: base(isReadOnly: isReadOnly) { }

		protected CriterionOperatorBase(SerializationContext ctx)
			: base(ctx: ctx) { }

		public abstract ILocalizable<string> DisplayName { get; }

		public abstract BinaryExpression BuildExpression(Expression left, Expression right);

		public virtual bool Equals(CriterionOperatorBase other) {
			if (ReferenceEquals(other, this))
				return true;
			else if (other is null)
				return false;
			else
				return GetType().Equals(o: other.GetType());
		}

		public bool Equals(ICriterionOperator other)
			=> Equals(other as CriterionOperatorBase);

		public override int GetHashCode()
			=> GetType().GetHashCode();

		public sealed override bool Equals(object other)
			=> Equals(other as CriterionOperatorBase);

		protected sealed override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locReadOnlyCopy);
			readOnlyCopy = locReadOnlyCopy;
		}

		protected abstract void CreateReadOnlyCopy(out CriterionOperatorBase readOnlyCopy);

		public new CriterionOperatorBase AsReadOnly() {
			if (IsReadOnly)
				return this;
			else {
				CreateReadOnlyCopy(out var readOnlyCopy);
				return readOnlyCopy;
			}
		}

		ICriterionOperator IAsReadOnlyMethod<ICriterionOperator>.AsReadOnly()
			=> AsReadOnly();

		protected override void OnValidate() { }

		public override string ToString()
			=> DisplayName?.Value ?? base.ToString();

	}

}