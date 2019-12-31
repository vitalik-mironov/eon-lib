using System.Linq.Expressions;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Eon.Globalization;
using Eon.Resources.XResource;
using Eon.Runtime.Serialization;

namespace Eon.Data.Filters.Criteria.Operators {

	[DataContract]
	public class LessThan
		:CriterionOperatorBase {

		static readonly ILocalizable<string> __DisplayTextViewSource = XResourceUtilities.GetTextXResourceAccessor(type: typeof(LessThan)).GetLocalizableText("UI/DisplayText");

		public LessThan() { }

		public LessThan(bool isReadOnly = false)
			: base(isReadOnly: isReadOnly) { }

		[JsonConstructor]
		protected LessThan(SerializationContext ctx)
			: base(ctx: ctx) { }

		public override ILocalizable<string> DisplayName
			=> __DisplayTextViewSource;

		public override BinaryExpression BuildExpression(Expression left, Expression right) {
			left.EnsureNotNull(nameof(left));
			right.EnsureNotNull(nameof(right));
			//
			return Expression.LessThan(left, right);
		}

		protected sealed override void CreateReadOnlyCopy(out CriterionOperatorBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locCopy);
			readOnlyCopy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out LessThan readOnlyCopy)
			=> readOnlyCopy = new LessThan(isReadOnly: true);


	}

}