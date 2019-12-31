using System.Linq.Expressions;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Eon.Globalization;
using Eon.Resources.XResource;
using Eon.Runtime.Serialization;

namespace Eon.Data.Filters.Criteria.Operators {

	[DataContract]
	public class NotEquals
		:CriterionOperatorBase {

		static readonly ILocalizable<string> __DisplayTextViewSource = XResourceUtilities.GetTextXResourceAccessor(type: typeof(NotEquals)).GetLocalizableText("UI/DisplayText");

		public NotEquals() { }

		public NotEquals(bool isReadOnly = false)
			: base(isReadOnly: isReadOnly) { }

		[JsonConstructor]
		protected NotEquals(SerializationContext ctx)
			: base(ctx: ctx) { }

		public override ILocalizable<string> DisplayName
			=> __DisplayTextViewSource;

		public override BinaryExpression BuildExpression(Expression left, Expression right) {
			left.EnsureNotNull(nameof(left));
			right.EnsureNotNull(nameof(right));
			//
			return Expression.NotEqual(left, right);
		}

		protected sealed override void CreateReadOnlyCopy(out CriterionOperatorBase readOnlyCopy) {
			CreateReadOnlyCopy(out var locCopy);
			readOnlyCopy = locCopy;
		}

		protected virtual void CreateReadOnlyCopy(out NotEquals readOnlyCopy)
			=> readOnlyCopy = new NotEquals(isReadOnly: true);


	}

}