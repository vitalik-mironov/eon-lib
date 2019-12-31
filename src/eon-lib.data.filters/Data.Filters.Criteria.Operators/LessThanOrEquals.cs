using System.Linq.Expressions;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Eon.Globalization;
using Eon.Resources.XResource;
using Eon.Runtime.Serialization;

namespace Eon.Data.Filters.Criteria.Operators {

	[DataContract]
	public class LessThanOrEquals
		:CriterionOperatorBase {

		static readonly ILocalizable<string> __DisplayTextViewSource = XResourceUtilities.GetTextXResourceAccessor(type: typeof(LessThanOrEquals)).GetLocalizableText("UI/DisplayText");

		public LessThanOrEquals() { }

		public LessThanOrEquals(bool isReadOnly = false)
			: base(isReadOnly: isReadOnly) { }

		[JsonConstructor]
		protected LessThanOrEquals(SerializationContext ctx)
			: base(ctx: ctx) { }

		public override ILocalizable<string> DisplayName
			=> __DisplayTextViewSource;

		public override BinaryExpression BuildExpression(Expression left, Expression right) {
			left.EnsureNotNull(nameof(left));
			right.EnsureNotNull(nameof(right));
			//
			return Expression.LessThanOrEqual(left, right);
		}

		protected sealed override void CreateReadOnlyCopy(out CriterionOperatorBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locCopy);
			readOnlyCopy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out LessThanOrEquals readOnlyCopy)
			=> readOnlyCopy = new LessThanOrEquals(isReadOnly: true);

	}

}