using System.Linq.Expressions;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Eon.Globalization;
using Eon.Resources.XResource;
using Eon.Runtime.Serialization;

namespace Eon.Data.Filters.Criteria.Operators {

	[DataContract]
	public class GreaterThan
		:CriterionOperatorBase {

		static readonly ILocalizable<string> __DisplayTextViewSource = XResourceUtilities.GetTextXResourceAccessor(type: typeof(GreaterThan)).GetLocalizableText("UI/DisplayText");

		public GreaterThan() { }

		public GreaterThan(bool isReadOnly = false)
			: base(isReadOnly: isReadOnly) { }

		[JsonConstructor]
		protected GreaterThan(SerializationContext ctx)
			: base(ctx: ctx) { }

		public override ILocalizable<string> DisplayName
			=> __DisplayTextViewSource;

		public override BinaryExpression BuildExpression(Expression left, Expression right) {
			left.EnsureNotNull(nameof(left));
			right.EnsureNotNull(nameof(right));
			//
			return Expression.GreaterThan(left, right);
		}

		protected sealed override void CreateReadOnlyCopy(out CriterionOperatorBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locCopy);
			readOnlyCopy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out GreaterThan readOnlyCopy)
			=> readOnlyCopy = new GreaterThan(isReadOnly: true);

	}

}